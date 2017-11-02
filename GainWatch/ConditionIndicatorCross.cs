using NLog;
using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Watch for the indicator to cross
	/// </summary>
/*
	public class ConditionIndicatorCross : Condition{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ConditionIndicatorCross( Stobj parent, XmlNode node ):base(parent,node){
			fudge = Double.Parse(GetAttribute(node,"Fudge"));
			indicator = Indicator.Make(MyStrategy.Position.Symbol, Name);
		}
		public static string			ElementName {get {return "IndicatorCross";}}
		private double					fudge = 0;
		private Indicator				indicator = null;
		public override bool			Test(){
			return FudgedTest();
			//return SimpleCrossTest();
		}

		private bool					SimpleCrossTest(){
			Symbol symbol = MyStrategy.Position.Symbol;
			// We want to do the trade if we've crossed

			// Only look 1x/minute to avoid whipsaws
			if (symbol.Tick.First!=true)
				return false;
			
			double prevLast = symbol.TimeTicks[symbol.TickTimeToIndex(symbol.Tick.Time)-1].Last;
			double last = MyStrategy.Position.Symbol.Tick.Last;

			if (prevLast < indicator.valu && indicator.valu < last){
				if (log.IsDebugEnabled)
					log.Debug(String.Format("LONG: {0} < {1} < {2} diff={3}", prevLast, indicator.valu, last, Math.Abs(last-prevLast)));
				
				if (Math.Abs(last-prevLast)>=fudge)
					MyStrategy.Position.ConfiguredType = Trip.Types.Long;
				else
					MyStrategy.Position.ConfiguredType = Trip.Types.Both;
				return true;
			}
			if (prevLast > indicator.valu && indicator.valu > last){
				if (log.IsDebugEnabled)
					log.Debug(String.Format("SHORT: {0} > {1} > {2} diff={3}", prevLast, indicator.valu, last, Math.Abs(last-prevLast)));
				if (Math.Abs(last-prevLast)>=fudge)
					MyStrategy.Position.ConfiguredType = Trip.Types.Short;
				else
					MyStrategy.Position.ConfiguredType = Trip.Types.Both;
				return true;
			}

			return false;
		}

		private bool					FudgedTest(){
			double last = MyStrategy.Position.Symbol.Tick.Last;
			double distance = last-indicator.valu;				// Positive for above the line
			Trip trip = MyStrategy.Position.Trip;

			if (trip==null){										// I'm not in a trade, should I get in?
				if (Math.Abs(distance)<fudge)						// Not if last is close to the indicator
					return false;
				// Calculate direction
				MyStrategy.Position.ConfiguredType = (distance>0)?Trip.Types.Long:Trip.Types.Short;
				return true;										// Get in
			} else {												// I'm in a trade, should I get out?
				if (trip.Type == Trip.Types.Short && distance>0)	// Gone wrong short?
					return true;									// Get out
				if (trip.Type == Trip.Types.Long && distance<0)		// Gone wrong long?
					return true;									// Get in
				return false;										// Stay
			}
		}
		public override string			ToStringLine(){return base.ToStringLine()+"("+fudge.ToString()+")";}
	}
*/
	/// <summary>
	/// Watch for two indicators to cross
	/// </summary>
	public class ConditionIndicatorsCross : Condition{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ConditionIndicatorsCross( Stobj parent, XmlNode node ):base(parent,node){
			Slow		= Indicator.Make(MyStrategy.Position.Symbol, GetAttribute(node,"Slow"));
			Fast		= Indicator.Make(MyStrategy.Position.Symbol, GetAttribute(node,"Fast"));
			Difference	= Double.Parse(GetAttribute(node,"Difference"));
			Window		= int.Parse(GetAttribute(node,"Window"));
		}
		public static string			ElementName {get {return "IndicatorsCross";}}
		private double					Difference;
		private Indicator				Fast = null;
		private Indicator				Slow = null;
		private int						Window;
		public override bool			TestCondition(){
			double prevFast;															// Get the value from the fast indicator
			double prevSlow;															// Get the value from the slow indicator
			Trip trip		= MyStrategy.Position.Trip;
			Symbol symbol	= MyStrategy.Position.Symbol;
			Tick prev		= symbol.GetPrevTick(symbol.Tick.Time.AddSeconds(-Window));	// Get tick from "Window" seconds ago

			if (prev==null)
				return false;

			if (Math.Abs(Fast.valu-Slow.valu)>=Difference){								// Enough of a difference to worry about?
				if (prev.Points != null && prev.Points.ContainsKey(Fast.Name))
					prevFast = (double)prev.Points[Fast.Name];
				else
					return false;

				if (prev.Points != null && prev.Points.ContainsKey(Slow.Name))
					prevSlow = (double)prev.Points[Slow.Name];
				else
					return false;
//				if (log.IsDebugEnabled)
//					log.Debug(String.Format("{0} {1} diff={2}",prevFast,prevSlow,Math.Abs(prevFast-prevSlow));
				if (Fast.valu > Slow.valu && prevFast < prevSlow ){
					MyStrategy.Position.Type = Trip.Types.Long;
					if (trip==null)
						return true;
					else {
						if (trip.Type!=Trip.Types.Long)
							return true;
						else
							return false;
					}
				}
				if (Fast.valu < Slow.valu && prevFast > prevSlow ){
					MyStrategy.Position.Type = Trip.Types.Short;
					if (trip==null)
						return true;
					else {
						if (trip.Type!=Trip.Types.Short)
							return true;
						else
							return false;
					}
				}
			}
			return false;
		}
		public override string			ToStringLine(){
			return base.ToStringLine()+"(fast="+Fast.Name+", slow="+Slow.Name+", diff="+Difference+", window="+Window+")";
		}
	}
	/// <summary>
	/// Watch for a specific slope of an indicator
	/// </summary>
	public class ConditionIndicatorSlope : Condition{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ConditionIndicatorSlope( Stobj parent, XmlNode node ):base(parent,node){
			indicator	= Indicator.Make(MyStrategy.Position.Symbol, GetAttribute(node,"Indicator"));
			Difference	= Double.Parse(GetAttribute(node,"Difference"));
			Window		= int.Parse(GetAttribute(node,"Window"));
		}
		public static string			ElementName {get {return "IndicatorSlope";}}
		private double					Difference;
		private Indicator				indicator = null;
		private int						Window;
		public override bool			TestCondition(){
			double prevIndicator;														// Get the value from the fast indicator
			Symbol symbol	= MyStrategy.Position.Symbol;
			Tick prev		= symbol.GetPrevTick(symbol.Tick.Time.AddSeconds(-Window));	// Get tick from "Window" seconds ago

			if (prev==null)
				return false;

			if (prev.Points != null && prev.Points.ContainsKey(indicator.Name))
				prevIndicator = (double)prev.Points[indicator.Name];
			else
				return false;

			if (Math.Abs(prevIndicator - indicator.valu) > Difference){
				if (prevIndicator < indicator.valu)
					MyStrategy.Position.Type = Trip.Types.Long;
				else
					MyStrategy.Position.Type = Trip.Types.Short;
				if (MyStrategy.Position.Trip==null || MyStrategy.Position.Trip.Type!=MyStrategy.Position.Type)
					return true;
			}
			return false;
		}
		public override string			ToStringLine(){
			return base.ToStringLine()+"(indicator="+indicator.Name+", Diff="+Difference+", Window="+Window+")";
		}
	}
	/// <summary>
	/// Watch for a specific slope of a price
	/// </summary>
	public class ConditionPriceSlope : Condition{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ConditionPriceSlope( Stobj parent, XmlNode node ):base(parent,node){
			Percent		= Double.Parse(GetAttribute(node,"Percent"))/100;
			Window		= int.Parse(GetAttribute(node,"Window"));
		}
		public static string			ElementName {get {return "PriceSlope";}}
		private double					Difference;
		private double					Percent;
		private int						Window;
		public override bool			TestCondition(){
			double prevPrice;
			Symbol symbol	= MyStrategy.Position.Symbol;
			Tick prev		= symbol.GetPrevTick(symbol.Tick.Time.AddSeconds(-Window));	// Get tick from "Window" seconds ago

			if (prev==null)
				return false;

			prevPrice = prev.Last;

			if (Math.Abs(prevPrice - symbol.Tick.Last) > Difference){
				if (prevPrice < symbol.Tick.Last)
					MyStrategy.Position.Type = Trip.Types.Long;
				else
					MyStrategy.Position.Type = Trip.Types.Short;
				if (MyStrategy.Position.Trip==null || MyStrategy.Position.Trip.Type!=MyStrategy.Position.Type)
					return true;
			}
			return false;
		}
		public override void Reset() {
			Difference = Percent*MyStrategy.Position.Symbol.Tick.Last;
			base.Reset ();
		}

		public override string			ToStringLine(){
			return base.ToStringLine()+"(Diff="+100*Percent+"%, Window="+Window+")";
		}
	}
}
