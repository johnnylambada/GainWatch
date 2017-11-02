//#define OLDWAY
using NLog;
using System;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Summary description for Indicator.
	/// </summary>
	public abstract class Indicator : IQuoteConsumer{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		protected Symbol		symbol;
		public double			valu;						// The value of the indicator as of the last calculation
		public string			Name;

		public					Indicator(Symbol s, string name){
			Name = name;
			symbol = s;
			Global.Quotes.Register(this, s.Name);
		}
		public static Indicator	Make(Symbol sym, string type){
			string s = type.Split(',')[0];
			if (s.ToLower()=="ema")	return new IndicatorEMA(sym, type);
			else
				throw new Exception("Indicator::Make(): Don't know how to make a "+type);
		}
		public abstract void QuoteUpdated(string symbol);
	}

	public class IndicatorEMA : Indicator{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		private int				Minutes;					// Minutes per period
		private double			Multiplier;					// EMA multiplier
		private double			previousPeriod=-1;			// The value of the EMA as of the previous period
		private DateTime		nextPeriod;

		public					IndicatorEMA(Symbol s, string parms):base(s,parms){
			string[] p	= parms.Split(',');
			Minutes		= int.Parse(p[1]);
			Multiplier	= 2/(double.Parse(p[2])+1);
		}
		public override void QuoteUpdated(string sym){
			double last = symbol.Tick.Last;

			// Prime the pump
			if (previousPeriod==-1){
				previousPeriod		= last;
				valu				= last;
				nextPeriod			= symbol.Tick.Time.AddMinutes(Minutes);
				nextPeriod			= nextPeriod.AddSeconds(-nextPeriod.Second);
			}

			// Have we reached the next period yet?
			if (symbol.Tick.Time>=nextPeriod){
				previousPeriod		= valu;
				nextPeriod			= symbol.Tick.Time.AddMinutes(Minutes);
				nextPeriod			= nextPeriod.AddSeconds(-nextPeriod.Second);
//				log.Debug(String.Format("{0:hh:mm:ss}",symbol.Tick.Time));
			}
			valu = ( (last-previousPeriod)*Multiplier) + previousPeriod;
			symbol.TickPoint(Name,valu);
		}
	}
}
