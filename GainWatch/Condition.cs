using NLog;
using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch
{
	/// <summary>
	/// Condition
	/// </summary>
	public abstract class Condition : Stobj{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public						Condition( Stobj parent, XmlNode node ):base(parent,node){}
		public void					GetOperator(XmlNode node){
			string o = GetAttribute( node, "Operator");
			if (o!=null)
				switch(o){
					case "=":	Operator = Operators.Equal;					break;
					case "<":	Operator = Operators.LessThan;				break;
					case ">":	Operator = Operators.GreaterThan;			break;
					case "<=":	Operator = Operators.LessThanOrEqual;		break;
					case ">=":	Operator = Operators.GreaterThanOrEqual;	break;
					case "+":	Operator = Operators.Plus;					break;
					case "-":	Operator = Operators.Minus;					break;
					default: throw new Exception("Condition: "+Name+" unknown operator: "+o);
				}
			else
				throw new Exception("Condition: "+Name+" must provide an operator");
		}
		/// <summary>
		/// Did a reset just occur on this poll?
		/// </summary>
		public bool					IsReset;
		public static Condition		Make(Stobj parent, XmlNode node){
			Condition c;
			string type = GetStaticAttribute(node,"Type");
			if		(type==ConditionGainPercent.ElementName)			c = new ConditionGainPercent(parent,node);
			else if (type==ConditionLossPercent.ElementName)			c = new ConditionLossPercent(parent,node);
			else if (type==ConditionTimeOfDay.ElementName)				c = new ConditionTimeOfDay(parent,node);
			else if (type==ConditionTrailingStopPercent.ElementName)	c = new ConditionTrailingStopPercent(parent,node);
			else if	(type==ConditionGainPrice.ElementName)				c = new ConditionGainPrice(parent,node);
			else if (type==ConditionLossPrice.ElementName)				c = new ConditionLossPrice(parent,node);
			else if (type==ConditionWaitEntry.ElementName)				c = new ConditionWaitEntry(parent,node);
			else if (type==ConditionIndicatorsCross.ElementName)		c = new ConditionIndicatorsCross(parent,node);
			else if (type==ConditionIndicatorSlope.ElementName)			c = new ConditionIndicatorSlope(parent,node);
			else if (type==ConditionPriceSlope.ElementName)				c = new ConditionPriceSlope(parent,node);
			else if (type==ConditionLadderStop.ElementName)				c = new ConditionLadderStop(parent,node);
			else if (type==ConditionConsistentTrend.ElementName)		c = new ConditionConsistentTrend(parent,node);
			else if (type==ConditionTripCount.ElementName)				c = new ConditionTripCount(parent,node);
			else if (type==ConditionTripExists.ElementName)				c = new ConditionTripExists(parent,node);
			else
				throw new Exception("Invalid CONDITION Type="+type);
			if (node.Name=="NCONDITION")
				c.Negate=true;
			return c;
		}
		public	bool				Negate=false;
		/// <summary>
		/// Cached access to TheRule (which takes relatively longer to compute)
		/// </summary>
		private Rule				_myRule = null;
		protected Rule				MyRule{
			get{
				if (_myRule==null)
					_myRule=TheRule;
				return _myRule;
			}
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
		public enum					Operators{
			None,
			Equal,
			LessThan,
			GreaterThan,
			LessThanOrEqual,
			GreaterThanOrEqual,
			Plus,
			Minus
		}
		public Operators			Operator = Operators.None;
		public virtual void			Poll(){
			IsReset = false;
		}
		public virtual void			Reset(){
			IsReset = true;
		}
		public bool					Test(){
			bool result = TestCondition();
			if (Negate==true)
				return !result;
			else
				return result;
		}
		public abstract bool		TestCondition();
		public override string		ToStringLine(){return "   CONDITION:"+base.ToStringLine();}
	}
}
