using NLog;
using System;
using System.Windows.Forms;
using System.Xml;

namespace LinuxWithin.GainWatch {
	public abstract class ActionPlan:Action{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		protected string				ReferencedPlanName = null;
		protected Plan					ReferencedPlan = null;
		public							ActionPlan(Stobj parent, XmlNode node):base(parent,node){
			// We capture the new Plan name now.  After the entire strategy is built, we fill in "ReferencedPlan"
			ReferencedPlanName = GetAttribute(node,"Plan");
		}
		public override void			FinishMaking() {
			foreach(Plan p in TheStrategy.Plans)
				if ( ReferencedPlanName == p.Name){
					ReferencedPlan = p;
					break;
				}
			if (ReferencedPlan==null)
				throw new Exception("Plan: "+ThePlan.Name+" Action:"+Name+" refs bad Plan:"+ReferencedPlanName);
			base.FinishMaking ();
		}
		public override string			ToStringLine(){return base.ToStringLine()+"("+ReferencedPlanName+")";}
	}
	/// <summary>
	/// Action to change plans
	/// </summary>
	public class ActionChangePlan : ActionPlan{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ActionChangePlan(Stobj parent, XmlNode node):base(parent,node){}
		public static string			ElementName {get {return "ChangePlan";}}

		public	override bool			Fire(){
			if (log.IsDebugEnabled)
				log.Debug("Fire() old plan="+ThePlan.Name+" new="+ReferencedPlan.Name);
			ThePlan.InEffect = false;			// Deactivate current plan
			ReferencedPlan.InEffect = true;		// Activate the new one
			return true;
		}
	}
	/// <summary>
	/// Action to activate a plan
	/// </summary>
	public class ActionActivatePlan : ActionPlan{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ActionActivatePlan(Stobj parent, XmlNode node):base(parent,node){}
		public static string			ElementName {get {return "ActivatePlan";}}

		public	override bool			Fire(){
			if (log.IsDebugEnabled)
				log.Debug("Fire() activating "+ReferencedPlan.Name);
			ReferencedPlan.InEffect = true;		// Activate the plan
			return true;
		}
	}
	/// <summary>
	/// Action to deactivate a plan
	/// </summary>
	public class ActionDeactivatePlan : ActionPlan{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ActionDeactivatePlan(Stobj parent, XmlNode node):base(parent,node){}
		public static string			ElementName {get {return "DeactivatePlan";}}

		public	override bool			Fire(){
			if (log.IsDebugEnabled)
				log.Debug("Fire() deactivating "+ReferencedPlan.Name);
			ReferencedPlan.InEffect = false;		// Deactivate the plan
			return true;
		}
	}

}
