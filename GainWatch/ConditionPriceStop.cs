using NLog;
using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// The idea here is to not lose too much on a loser, but let a winner ride awhile
	/// </summary>
	public class ConditionLadderStop : Condition{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 

		#region data
		/// <summary>
		/// The name of this Element
		/// </summary>
		public static string			ElementName {get {return "LadderStop";}}
		/// <summary>
		/// Set when the next tick will be the first for this condition
		/// </summary>
		bool							FirstTick = true;
		/// <summary>
		/// The percent of the Last price the ladder rung starts as
		/// </summary>
		double							Percent;
		/// <summary>
		/// The price on the other side of which this condition becomes true
		/// </summary>
		private double					PriceExit;
		/// <summary>
		/// The price on the other side of which we will recalculate the PriceExit & PriceRecalc
		/// </summary>
		private double					PriceRecalc;
		/// <summary>
		/// The width of each rung of the ladder.  Also, the max number of cents per share to lose
		/// </summary>
		private	double					Width;
		/// <summary>
		/// How many rungs have we made?
		/// </summary>
		private int						Rungs;
		private Trip					Trip;
		/// <summary>
		/// If the rung is divisible by this number (and it's not zero, then widen at this rung)
		/// </summary>
		private	int						Widen;
		#endregion

		public							ConditionLadderStop( Stobj parent, XmlNode node ):base(parent,node){
			Percent	= Double.Parse(GetAttribute(node,"Percent"))/100;
			Widen	= int.Parse(GetAttribute(node,"Widen"));
		}
		public override void Reset() {
			Width = Percent*MyStrategy.Position.Symbol.Tick.Last;
			FirstTick = true;
			base.Reset ();
		}

		public override bool			TestCondition(){
			FirstTick = TestLadder();
			if (FirstTick){
				// tear down the ladder
				Trip = null;
			}
			return FirstTick;
		}
		private bool					TestLadder(){
			Tick tick = MyStrategy.Position.Symbol.Tick;
			if (FirstTick){
				// Set up the ladder
				Trip = MyStrategy.Position.Trip;
				if (Trip==null)
					throw new Exception(string.Format("The {0} condition is only appropriate to exit a position.",ElementName));
				double w = Width * ((int)Trip.Type);
				PriceExit	= tick.Last - w;
				PriceRecalc	= tick.Last + w;
				Rungs = 1;									// We've created ground floor and #1
				if (log.IsDebugEnabled)
					log.Debug(String.Format("Initialized: exit={0:c} last={1:c} recalc={2:c}",PriceExit,tick.Last,PriceRecalc));
			} else {
				// First of all, are we on the wrong side of the stop price?
				if (Trip.Type==Trip.Types.Long){
					if (tick.Last < PriceExit)
						return true;
				} else {
					if (tick.Last > PriceExit)
						return true;
				}

				// Not time to exit yet? Is it time to recalc?
				bool recalcNow = false;
				if (Trip.Type==Trip.Types.Long){
					if (tick.Last > PriceRecalc)
						recalcNow = true;
				} else {
					if (tick.Last < PriceRecalc)
						recalcNow = true;
				}
				if (recalcNow){
					double w = Width * ((int)Trip.Type);
					PriceRecalc = tick.Last + w;
					if (log.IsDebugEnabled)
						log.Debug(String.Format("Recalcing: recalc={0:c}",PriceRecalc));
					if ( !(Widen>0 && (Rungs%Widen)==0) ){
						PriceExit += w;
						if (log.IsDebugEnabled)
							log.Debug(String.Format("Recalcing: exit  ={0:c}",PriceExit));
					}
					Rungs++;
				}
				MyStrategy.Position.Symbol.TickPoint("Exit",PriceExit);
				MyStrategy.Position.Symbol.TickPoint("Recalc",PriceRecalc);
			}
			return false;
		}
		public override string			ToStringLine(){
			return base.ToStringLine()+"(Initial Rung Width="+100*Percent+"%, Widen="+Widen+")";
		}
	}
}
