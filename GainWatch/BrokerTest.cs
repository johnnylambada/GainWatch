using NLog;
using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Windows.Forms;

namespace LinuxWithin.GainWatch{

	/// <summary>
	/// Abstract class covering the commonalities between brokerage houses
	/// </summary>
	public class BrokerTest : Broker{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 

		private int					MarketFillSeconds;		// Number of seconds to wait for the worst price on a market order

		public override string		Name {get {return "Test Broker";}}
		public						BrokerTest(Stack args):base(args){
			MarketFillSeconds = int.Parse((string)args.Pop());
		}
		public override bool		IsReal{get{return false;}}
		public override	bool		MarketEnterContinue(Position pos){
			BrokerTestInfo	info;
			double			last	= pos.Symbol.Tick.Last;
			if (pos.Trip.BrokerInfo == null){
				info = new BrokerTestInfo();
				pos.Trip.BrokerInfo = (object) info;
				info.WaitTime		= pos.Symbol.Tick.Time.AddSeconds(MarketFillSeconds);
				pos.Trip.PriceIn	= last;		// Start with the tick that triggered the event
			} else
				info				= (BrokerTestInfo) pos.Trip.BrokerInfo;
			if (pos.Symbol.Tick.Time<info.WaitTime){
				if (last<pos.Trip.PriceIn){
					if (pos.Trip.Type == Trip.Types.Short){
						pos.Trip.PriceIn = last;
					}
				} else {
					if (pos.Trip.Type == Trip.Types.Long){
						pos.Trip.PriceIn = last;
					}
				}
				return false;
			} else {
				pos.Trip.BrokerInfo = null;
				return true;
			}
		}
		public override bool		MarketExitContinue(Position pos){
			BrokerTestInfo	info;
			double			last	= pos.Symbol.Tick.Last;
			if (pos.Trip==null){
				if (log.IsInfoEnabled)
					log.Info("MARKET ORDER ABORTED");
				return true;
			}
			if (pos.Trip.BrokerInfo == null){
				info = new BrokerTestInfo();
				pos.Trip.BrokerInfo = (object) info;
				info.WaitTime		= pos.Symbol.Tick.Time.AddSeconds(MarketFillSeconds);
				pos.Trip.TimeOut	= pos.Symbol.Tick.Time;
				pos.Trip.PriceOut	= last;		// Start with the tick that triggered the event
			} else
				info				= (BrokerTestInfo) pos.Trip.BrokerInfo;
			if (pos.Symbol.Tick.Time<info.WaitTime){
				if (last>pos.Trip.PriceOut){
					if (pos.Trip.Type == Trip.Types.Short){
						pos.Trip.PriceOut = last;
					}
				} else {
					if (pos.Trip.Type == Trip.Types.Long){
						pos.Trip.PriceOut = last;
					}
				}
				return false;
			} else {
				pos.Broker.Funds	+= pos.Trip.Gain;
				pos.Trip.BrokerInfo = null;
				pos.TripFinish();
				return true;
			}
		}
		public override bool		LimitExitContinue(Position pos){
			double			PercentOfVolume = 0.03;
			BrokerTestInfo	info;
			double			last	= pos.Symbol.Tick.Last;
			if (pos.Trip.BrokerInfo == null){
				info				= new BrokerTestInfo();
				info.Quantity		= pos.Trip.Quantity;
				pos.Trip.BrokerInfo	= (object)info;
				pos.Trip.TimeOut	= pos.Symbol.Tick.Time;
				pos.Trip.PriceOut	= pos.Trip.PriceOutLimit;
			} else
				info				= (BrokerTestInfo) pos.Trip.BrokerInfo;
			if ( 
				(last>=pos.Trip.PriceOutLimit && pos.Trip.Type == Trip.Types.Long) ||
				(last<=pos.Trip.PriceOutLimit && pos.Trip.Type == Trip.Types.Short)
			){
				System.Int64 thisQuantity = (System.Int64) (PercentOfVolume * pos.Symbol.Tick.LastSize);
				if (thisQuantity >= info.Quantity){
					if (log.IsDebugEnabled)
						log.Debug("Found the rest, exiting position");
					pos.Trip.PriceOut	= pos.Trip.PriceOutLimit;
					pos.Trip.TimeOut	= pos.Symbol.Tick.Time;
					pos.Broker.Funds	+= pos.Trip.Gain;
					pos.TripFinish();
					return true;
				} else {
					info.Quantity -= thisQuantity;
					if (log.IsDebugEnabled)
						log.Debug(String.Format("Found {0} shares, {1} left to find.",thisQuantity,info.Quantity));
				}
			}
			return false;
		}
		protected override string	Trade( Action act,  int shares, string equity, Terms term, double price){
			base.Trade(act,shares,equity,term,price);
			return "Test";
		}
	}
	/// <summary>
	/// The BrokerTestPosition class holds information specific to an instance of the BrokerTest for a particular Position
	/// </summary>
	public class BrokerTestInfo{
		public	DateTime			WaitTime;
		public	System.Int64		Quantity;		// Quantity still left to fill
	}
}
