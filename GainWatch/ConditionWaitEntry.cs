using NLog;
using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Wait for entry condition
	/// </summary>
	public class ConditionWaitEntry : Condition{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ConditionWaitEntry( Stobj parent, XmlNode node ):base(parent,node){}
		public static string			ElementName {get {return "WaitEntry";}}

		int asksHit = 0;
		public override bool			TestCondition(){
			if (MyStrategy.Position.Symbol.Tick.Last > MyStrategy.Position.Symbol.Ask){
				//log.Debug("ask="+MyStrategy.Position.Symbol.Ask+" last="+MyStrategy.Position.Symbol.Tick.Last);
				asksHit++;
			}else{
				asksHit=0;
			}

			if (asksHit>3)
				return true;
			else
				return false;
		}
		public override string			ToStringLine(){return base.ToStringLine();}
	}
}
