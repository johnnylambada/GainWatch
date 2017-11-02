using NLog;
using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch
{
	/// <summary>
	/// Rule
	/// </summary>
	public class Rule : Stobj{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public	ArrayList			Conditions = null;
		public	ArrayList			Actions = null;
		public Action				ActionCurrent = null;
		public static string		ElementName {get {return "RULE";}}
		public static Rule			Make(Stobj parent, XmlNode node){
			if (node.Name!=Rule.ElementName)
				throw new Exception(node.Name+" found where RULE expected");
			return new Rule(parent, node);
		}
		public						Rule( Stobj parent, XmlNode node ):base(parent, node){
			Conditions = new ArrayList();
			Actions = new ArrayList();
			foreach(XmlNode n in node.ChildNodes)
				if (n.NodeType==XmlNodeType.Element)
					if (n.Name=="CONDITION" || n.Name=="NCONDITION"){
						Conditions.Add(Condition.Make(this,n));
					} else if (n.Name=="ACTION") {
						Actions.Add(Action.Make(this,n));
					} else {
						throw new Exception(n.Name+" found where CONDITION or ACTION expected");
					}

			// children include both the action and the conditions
			Children = new ArrayList(Conditions);
			Children.AddRange(Actions);
		}
		public override string		ToStringLine(){return "  "+base.ToStringLine();}
	}
}
