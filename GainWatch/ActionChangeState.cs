using NLog;
using System;
using System.Windows.Forms;
using System.Xml;

namespace LinuxWithin.GainWatch {
	/// <summary>
	/// Action to change plans
	/// </summary>
	public class ActionChangeState : Action{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ActionChangeState(Stobj parent, XmlNode node):base(parent,node){
			string newState = GetAttribute(node,"State");
			switch(newState){
				case "Setup":		NextState = Position.States.Setup;		break;
				case "WaitEntry":	NextState = Position.States.WaitEntry;	break;
				case "WaitExit":	NextState = Position.States.WaitExit;	break;
				case "Complete":	NextState = Position.States.Complete;	break;
				default:			throw new Exception("Invalid New State "+newState+" in Action:"+ElementName);
			}
		}
		public static string			ElementName {get {return "ChangeState";}}
		public	override bool			Fire(){
			if (log.IsDebugEnabled)
				log.Debug("Fire() New State="+NextState.ToString());

			TheStrategy.Position.State = NextState;

			// We need to go through this plan and set ineffect to false
			foreach(Plan p in TheStrategy.Plans)
				p.InEffect = false;
			return true;
		}
		public Position.States			NextState;
		public override string			ToStringLine(){return base.ToStringLine()+"("+NextState.ToString()+")";}
	}
}
