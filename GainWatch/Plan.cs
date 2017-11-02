using NLog;
using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch
{
	/// <summary>
	/// Plan
	/// </summary>
	public class Plan : Stobj{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public static string		ElementName {get {return "PLAN";}}
		private bool				_inEffect;
		public	bool				InEffect{
			get{ return _inEffect;	}
			set{if (log.IsDebugEnabled) log.Debug("Plan:"+Name+" InEffect is now "+value);
				_inEffect = value;
				if (_inEffect==true)
					Reset();
			}
		}
		public static Plan			Make(Stobj parent, XmlNode node){
			if (node.Name!=Plan.ElementName)
				throw new Exception(node.Name+" found where PLAN expected");
			return new Plan(parent,node);
		}
		public						Plan( Stobj parent, XmlNode node ):base(parent, node){
			Rule r;
			Children = Rules = new ArrayList();

			foreach(XmlNode n in node.ChildNodes)
				if (n.NodeType == XmlNodeType.Element){
					r = Rule.Make(this,n);
					Rules.Add(r);
				}

			string att = GetAttribute(node,"InEffect");
			if (att!=null && att=="True")
				_inEffect = true;					// Don't use InEffect, we don't want to Reset()/Poll()
		}
		public void					Reset(){
			if (log.IsDebugEnabled) log.Debug("Reset("+this.Name+")");
			foreach( Rule r in Rules){
				r.ActionCurrent = null;						// Kill any current action
				foreach( Condition c in r.Conditions){
					c.Reset();
					c.Poll();
				}
			}
		}
		public		ArrayList		Rules = null;
		public override string		ToStringLine(){return " "+base.ToStringLine();}
	}
}
