using NLog;
using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Wait for entry condition
	/// </summary>
	public class ConditionTripCount : Condition{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public	int						Count=0;
		public							ConditionTripCount( Stobj parent, XmlNode node ):base(parent,node){
			GetOperator(node);
			Count = int.Parse(GetAttribute(node,"Count"));
		}
		public static string			ElementName {get {return "TripCount";}}

		public override bool			TestCondition(){
			switch(this.Operator){
				case Operators.GreaterThanOrEqual:
					return MyStrategy.Position.Trips.Count >= Count;
				case Operators.GreaterThan:
					return MyStrategy.Position.Trips.Count > Count;
				case Operators.LessThanOrEqual:
					return MyStrategy.Position.Trips.Count <= Count;
				case Operators.LessThan:
					return MyStrategy.Position.Trips.Count < Count;
				case Operators.Equal:
					return MyStrategy.Position.Trips.Count == Count;
				default:
					throw new Exception(ElementName+" condition used with an invalid operator: "+this.Operator.ToString());
			}
		}
		public override string			ToStringLine(){return base.ToStringLine()+"("+Count+")";}
	}
	public class ConditionTripExists : Condition{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ConditionTripExists( Stobj parent, XmlNode node ):base(parent,node){}
		public static string			ElementName {get {return "TripExists";}}

		public override bool			TestCondition(){
			if (MyStrategy.Position.Trip!=null)
				return true;
			else
				return false;
		}
	}
}
