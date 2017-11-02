using NLog;
using System;
using System.Xml;

namespace LinuxWithin.GainWatch {
	/// <summary>
	/// Action
	/// </summary>
	public abstract class Action : Stobj{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public								Action(Stobj parent, XmlNode node):base(parent,node){}
		public virtual bool					FireAgain(){return true;}
		public	abstract bool				Fire();
		public static Action				Make(Stobj parent, XmlNode node){
			Action c;
			string type = GetStaticAttribute(node,"Type");
			if		(type==ActionChangePlan.ElementName)			c = new ActionChangePlan(parent,node);
			else if (type==ActionActivatePlan.ElementName)			c = new ActionActivatePlan(parent,node);
			else if (type==ActionDeactivatePlan.ElementName)		c = new ActionDeactivatePlan(parent,node);
			else if (type==ActionMarketEnter.ElementName)			c = new ActionMarketEnter(parent,node);
			else if (type==ActionMarketExit.ElementName)			c = new ActionMarketExit(parent,node);
			else if (type==ActionChangeState.ElementName)			c = new ActionChangeState(parent,node);
			else if (type==ActionFlipType.ElementName)				c = new ActionFlipType(parent,node);
			else if (type==ActionSetType.ElementName)				c = new ActionSetType(parent,node);
			else if (type==ActionLimitExit.ElementName)				c = new ActionLimitExit(parent,node);
			else
				throw new Exception("Invalid ACTION Type="+type);
			return c;
		}
		/// <summary>
		/// Cached access to TheStrategy (which takes relatively longer to compute)
		/// </summary>
		private Strategy			_myStrategy = null;
		protected Strategy			MyStrategy{
			get{
				if (_myStrategy==null)
					_myStrategy=TheStrategy;
				return _myStrategy;
			}
		}
		public override string				ToStringLine(){return "   ACTION   :"+base.ToStringLine();}
	}
}
