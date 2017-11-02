using NLog;
using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// The idea here is to not lose too much on a loser, but let a winner ride awhile
	/// </summary>
	public class ConditionConsistentTrend : Condition{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 

		#region data
		/// <summary>
		/// The name of this Element
		/// </summary>
		public static string			ElementName {get {return "ConsistentTrend";}}
		/// <summary>
		/// The number of seconds the trend must be consistent for
		/// </summary>
		private	int						Seconds;
		/// <summary>
		/// The number of seconds for each check
		/// </summary>
		private	int						Step;
		#endregion

		public							ConditionConsistentTrend( Stobj parent, XmlNode node ):base(parent,node){
			Seconds	= int.Parse(GetAttribute(node,"Seconds"));
			Step	= int.Parse(GetAttribute(node,"Step"));
		}

		public override bool			TestCondition(){
			// This condition is pretty lame right now.  It can be sped up considerably by
			// adding a next time to check -- that is, realizing that the cond can't be true
			// until some time in the future
			Symbol		symbol	= MyStrategy.Position.Symbol;
			Tick		tick	= symbol.Tick;
			DateTime	time	= tick.Time;
			double		price	= tick.Last;
			Trip.Types	type;

			DateTime past	= time.AddSeconds(-Seconds);

			// Check the first to determine the direction
			time = time.AddSeconds(-Step);
			tick = symbol.GetPrevTick(time);
			if (tick==null)
				return false;
			if (tick.Last==price)								// Not enough activity
				return false;
			if (tick.Last<price)
				type = Trip.Types.Long;
			else
				type = Trip.Types.Short;

			// Now check the rest of the steps to see if the direciton agrees
			while(time > past){
				price= tick.Last;
				time = time.AddSeconds(-Step);
				Tick prevTick = tick;
				tick = symbol.GetPrevTick(time);
				if (tick==prevTick)								// Not enough activity
					return false;
				if (tick==null)
					return false;
				if ( (type==Trip.Types.Long && tick.Last>price) || (type==Trip.Types.Short && tick.Last<price))
					return false;
			}
			MyStrategy.Position.Type = type;
			return true;
		}
		public override string			ToStringLine(){
			return base.ToStringLine()+"(Seconds="+Seconds+", Step="+Step+")";
		}
	}
}
