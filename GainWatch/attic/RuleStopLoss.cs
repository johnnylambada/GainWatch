using System;

namespace LinuxWithin.GainWatch
{
	/// <summary>
	/// Abstract base class covering the commonalities between rules
	/// </summary>
	public class RuleStopLoss : Rule{
		public override string Name{get{return"StopLoss";}}
		protected double StopLossPrice;
		public RuleStopLoss( double price ):base(){
			StopLossPrice = price;
		}
		public override bool Test(){
			if (Pos.Type==Position.Types.Long){
				if (Pos.Symbol.Price<StopLossPrice && Pos.Symbol.PricePrevious<StopLossPrice)
					return true;
			} else {
				if (Pos.Symbol.Price>StopLossPrice && Pos.Symbol.PricePrevious>StopLossPrice)
					return true;
			}
			return false;
		}
	}
}
