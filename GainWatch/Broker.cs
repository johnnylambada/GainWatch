using NLog;
using System;
using System.Collections;
using System.Windows.Forms;

namespace LinuxWithin.GainWatch{

	/// <summary>
	/// Abstract class covering the commonalities between brokerage houses
	/// </summary>
	public abstract class Broker{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public enum					Action {Buy, Sell, Cover, Short};
		public enum					Terms {Market, Limit, Stop, StopLimit};
		public enum					CostingMethods{	None,Direct, IB, Freetrade};

		protected					Broker(Stack args){
			if (this.IsReal==true && Global.Quotes is IBacktest)
				throw new Exception("You cannot use a real broker with a test quote source");
			Funds		= double.Parse((string)args.Pop());
			Leverage	= int.Parse((string)args.Pop());
			Funds		*= Leverage;

			string cm	= (string) args.Pop();
			switch(cm){
				case "IB":
					CostingMethod = CostingMethods.IB;
					break;
				case "Freetrade":
					CostingMethod = CostingMethods.Freetrade;
					break;
				default:
					CostingMethod = CostingMethods.Direct;
					cost = double.Parse(cm);
					break;
			}
		}
		private double				cost;
		public double				Cost(Trip trip){
			switch (CostingMethod){
				case CostingMethods.Direct:
					return cost;
				case CostingMethods.IB:
					double c=0;
					if (trip.Quantity<=500)
						c=0.01*trip.Quantity;
					else
						c=0.01*500+0.005*(trip.Quantity-500);
					if (c<1)
						c=1;
					c*=2;
					return c;
				case CostingMethods.Freetrade:
					return 6;							// A Guess for now
				default:
					throw new Exception("No costing method assigned!");
			}
		}
		public CostingMethods		CostingMethod = CostingMethods.None;
		public double				Funds;
		public abstract bool		IsReal{get;}
		public int					Leverage=1;
		public static Broker		Make(string BrokerDescriptor){
			ArrayList al	= new ArrayList(BrokerDescriptor.Split(','));
			al.Reverse();
			Stack args = new Stack(al);
			if (args!=null && args.Count>0)
				switch((string)args.Pop()){
					case "Freetrade":	return new BrokerFreetrade(args);
					case "Test":		return new BrokerTest(args);
				}
			throw new Exception("Invalid BrokerDescriptor: "+BrokerDescriptor);
		}
		public bool					MarketEnter(Position pos){
			if (pos.Trip.BrokerInfo==null){
				if (pos.Trip.Type == Trip.Types.Long)
					Trade( Action.Buy, pos.Trip.Quantity, pos.Symbol.Name, Terms.Market, 0 );
				else
					Trade( Action.Short, pos.Trip.Quantity, pos.Symbol.Name, Terms.Market, 0 );
			}

			return MarketEnterContinue(pos);
		}
		public abstract	bool		MarketEnterContinue(Position pos);
		public bool					MarketExit(Position pos){
			if (pos.Trip==null)
				return true;
			if (pos.Trip.BrokerInfo==null){
				if (pos.Trip.Type == Trip.Types.Long)
					Trade( Action.Sell, pos.Trip.Quantity, pos.Symbol.Name, Terms.Market, 0 );
				else
					Trade( Action.Cover, pos.Trip.Quantity, pos.Symbol.Name, Terms.Market, 0 );
			}
			return MarketExitContinue(pos);
		}
		public abstract bool		MarketExitContinue(Position pos);
		public bool					LimitExit(Position pos, double percent){
			if (pos.Trip==null)
				return true;
			if (pos.Trip.BrokerInfo==null){
				if (pos.Trip.Type == Trip.Types.Long){
					pos.Trip.PriceOutLimit = pos.Trip.PriceIn + (pos.Trip.PriceIn*percent);
					Trade( Action.Sell, pos.Trip.Quantity, pos.Symbol.Name, Terms.Limit, pos.Trip.PriceOutLimit );
				}else{
					pos.Trip.PriceOutLimit = pos.Trip.PriceIn - (pos.Trip.PriceIn*percent);
					Trade( Action.Cover, pos.Trip.Quantity, pos.Symbol.Name, Terms.Limit, pos.Trip.PriceOutLimit );
				}
			}
			return LimitExitContinue(pos);
		}
		public abstract bool		LimitExitContinue(Position pos);
		public abstract string		Name{get;}
		protected virtual string	Trade( Action act,  int shares, string equity, Terms term, double price){
			if (log.IsDebugEnabled)
				log.Debug(act.ToString()+" "+shares+" of "+equity+(price>0?" at "+price:"")+" last="+Global.Quotes.Symbol(equity).Tick.Last);
			return null;
		}
	}
}
