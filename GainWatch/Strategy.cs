using NLog;
using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;

namespace LinuxWithin.GainWatch{
	public class StrategyDataPoint{
		public string	Name = null;
		public double	Value = 0;
		public StrategyDataPoint(string name, double val){
			Name=name;
			Value=val;
		}
	}
	/// <summary>
	/// Strategy
	/// </summary>
	public class Strategy : Stobj{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public static string		ElementName {get {return "STRATEGY";}}
		public string				FileName = null;
		public	SortedList			DataPoints(){
			SortedList sl = new SortedList();
			foreach(Plan p in Plans)
				if (p.InEffect)
					foreach( Rule r in p.Rules)
						foreach(Condition c in r.Conditions)
							if (c is ConditionStop)
								if (!sl.ContainsKey( ((ConditionStop)c).StopPrice))
									sl.Add(
										((ConditionStop)c).StopPrice,
										new StrategyDataPoint(c.Name,((ConditionStop)c).StopPrice));
			return sl;
		}
		public static	Strategy	Load( Position position, string fileName ){
			if (log.IsDebugEnabled) log.Debug("Load("+fileName+")");
			Strategy s = null;
			XmlDocument xml = new XmlDocument();
			xml.Load(fileName);
			foreach( XmlNode n in xml)
				if (n.NodeType == XmlNodeType.Element)
					s = Strategy.Make(position,n);
			if (s == null)
				throw new Exception("Unable to parse the strategy");
			s.FileName = fileName;
			return s;
		}
		public static Strategy		Make(Position position, XmlNode node){
			if (node.Name!=Strategy.ElementName)
				throw new Exception(node.Name+" found where STRATEGY expected");
			return new Strategy(position,node);
		}
		public	ArrayList			Plans = null;
		public	void				Poll(){
			foreach(Plan p in Plans)
				if (p.InEffect)
					foreach(Rule r in p.Rules){
						if (r.ActionCurrent==null){
							// If there is no action in progress, then go through the conditions
							bool test = true;
							foreach(Condition c in r.Conditions)
								if (test==true)
									test = c.Test();
								else
									c.Poll();
							if (test==true){
								if (log.IsDebugEnabled)
									log.Debug("Plan:"+p.Name+" Rule:"+r.Name+" is true!");
								foreach(Action a in r.Actions)
									if (a.Fire()==false){
										// Action.Fire() returns a flag saying that the action is complete
										r.ActionCurrent = a;
										break;
									}
							}
						} else {
							// If there is an action in progress, skip the conditions and finish the actions
							bool found=false;
							foreach(Action a in r.Actions){
								if (a==r.ActionCurrent)
									found=true;
								if (found==true){
									bool again;
									if (a==r.ActionCurrent)
										again = a.FireAgain();
									else
										again = a.Fire();
									if (again==false){
										r.ActionCurrent = a;
										break;
									}
								}
							}
						}
					}
		}
		private Position			_position = null;
		public	Position			Position{
			get{ return _position;	}
			set{ _position = value;	}
		}
		public void					Reset(){
			foreach(Plan p in Plans)
				if (p.InEffect)
					p.Reset();
		}
		public						Strategy( Position position, XmlNode node ):base(null, node){
			Position = position;
			Plan pl;
			Children = Plans = new ArrayList(node.ChildNodes.Count);
			foreach(XmlNode n in node.ChildNodes)
				if (n.NodeType == XmlNodeType.Element){
					pl = Plan.Make(this,n);
					Plans.Add(pl);
				}

			// Now spin through the tree and let each node finish itself up
			foreach(Plan p in Plans){
				foreach(Rule r in p.Rules){
					foreach(Condition c in r.Conditions){
						c.FinishMaking();
					}
					foreach(Action a in r.Actions){
						a.FinishMaking();
					}
					r.FinishMaking();
				}
				p.FinishMaking();
			}
			FinishMaking();
		}
		public override string		ToStringLine(){return base.ToStringLine();}
	}
}
