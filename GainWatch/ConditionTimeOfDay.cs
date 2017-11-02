using NLog;
using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Time Of Day condition
	/// </summary>
	public class ConditionTimeOfDay : Condition{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public	int						Offset = 0;
		public	DateTime				ConditionTime;
		public static string			ElementName {get {return "TimeOfDay";}}
		public	DateTime				Never = new DateTime(0);
		private	DateTime				TheTime;

		public							ConditionTimeOfDay( Stobj parent, XmlNode node ):base(parent,node){
			ConditionTime = Never;
			GetOperator(node);
			if (Operator == Operators.Plus)
				Offset = int.Parse(GetAttribute(node,"Offset"));
			else
				ConditionTime = DateTime.Parse(GetAttribute(node,"Time"));
		}
		public override void Reset() {
			if (Operator == Operators.Plus)
				TheTime = MyStrategy.Position.Symbol.Tick.Time.AddSeconds(Offset);
			else
				TheTime = ConditionTime;
			base.Reset ();
		}

		public override bool			TestCondition(){
			switch(this.Operator){
				case Operators.GreaterThanOrEqual:
				case Operators.GreaterThan:
				case Operators.Plus:
					return MyStrategy.Position.Symbol.Tick.Time >= TheTime;
				case Operators.LessThanOrEqual:
				case Operators.LessThan:
					return MyStrategy.Position.Symbol.Tick.Time <= TheTime;
				case Operators.Equal:
					throw new Exception(ElementName+": Don't test for equality, it'll never happen!");
				default:
					throw new Exception(ElementName+" condition used without an operator!");
			}
		}
		public override string			ToStringLine(){return base.ToStringLine()+string.Format("({0:HH:mm:ss})",ConditionTime);}
	}
}
