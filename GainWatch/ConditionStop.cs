using NLog;
using System;
using System.Collections;
using System.Xml;

// This can be expanded for ConditionGainAmount/ConditionLossAmount by cloning the percent code but changing
// ConditionPercent::Poll() to do an amount calculation
namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Gain Percent condition
	/// </summary>
	public class ConditionGainPercent : ConditionPercent{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ConditionGainPercent(Stobj parent, XmlNode node ):base(parent,node){}
		public override int				Direction{get{return 1;}}
		public static string			ElementName {get {return "GainPercent";}}
	}
	/// <summary>
	/// Loss Percent condition
	/// </summary>
	public class ConditionLossPercent : ConditionPercent{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ConditionLossPercent(Stobj parent, XmlNode node ):base(parent,node){}
		public override int				Direction{get{return -1;}}
		public static string			ElementName {get {return "LossPercent";}}
	}
	public class ConditionTrailingStopPercent : ConditionStop{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public					ConditionTrailingStopPercent(Stobj parent, XmlNode node ):base(parent,node){
			Percent = double.Parse(GetAttribute(node,"Percent"))/100;
		}
		public override int		Direction{get{return -1;}}
		public static string	ElementName {get {return "TrailingStopPercent";}}
		private double			HighPrice;
		public	double			Percent;
		public override void	Poll(){
			// Set up the StopPrice
			double lastPrice = MyStrategy.Position.Symbol.Tick.Last;
			if (MyStrategy.Position.Trip.Type == Trip.Types.Long){
				if (lastPrice > HighPrice || IsReset){
					HighPrice = lastPrice;
					StopPrice = HighPrice + (Direction*HighPrice*Percent);
					if (log.IsDebugEnabled) log.Debug(string.Format("HighPrice={0} StopPrice={1}",HighPrice,StopPrice));
				}
			} else {
				if (lastPrice < HighPrice || IsReset){
					HighPrice = lastPrice;
					StopPrice = HighPrice + (Direction*HighPrice*Percent * -1);	// *-1 to reverse dir for short
					if (log.IsDebugEnabled) log.Debug(string.Format("HighPrice={0} StopPrice={1}",HighPrice,StopPrice));
				}
			}
			base.Poll();
		}
		public override bool	TestCondition() {
			Poll();
			return base.Test();
		}
		public override string			ToStringLine(){return base.ToStringLine()+"("+Percent*100+"%)";}
	}
	/// <summary>
	/// Generalized percent
	/// </summary>
	public abstract class ConditionPercent : ConditionStop{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ConditionPercent( Stobj parent, XmlNode node ):base(parent,node){
			Percent = double.Parse(GetAttribute(node,"Percent"))/100;
		}
		public	double					Percent;
		public override void			Poll(){
			// Set up the StopPrice
			if (IsReset){
				double inPrice = MyStrategy.Position.Trip.PriceIn;
				StopPrice =  inPrice + (Direction*inPrice*Percent*((double)MyStrategy.Position.Trip.Type));
			}
			base.Poll();
		}
		public override string			ToStringLine(){return base.ToStringLine()+"("+Percent*100+"%)";}
	}
	/// <summary>
	/// Gain Price
	/// </summary>
	public class ConditionGainPrice : ConditionStop{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ConditionGainPrice(Stobj parent, XmlNode node ):base(parent,node){
			Price = double.Parse(GetAttribute(node,"Price"));
		}
		public override int				Direction{get{return 1;}}
		public static string			ElementName {get {return "GainPrice";}}
		public override void			Poll(){
			if (IsReset)
				StopPrice =  Price;
			base.Poll();
		}
		public	double					Price;
	}
	/// <summary>
	/// Loss Price
	/// </summary>
	public class ConditionLossPrice : ConditionStop{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ConditionLossPrice(Stobj parent, XmlNode node ):base(parent,node){
			Price = double.Parse(GetAttribute(node,"Percent"));
		}
		public override int				Direction{get{return -1;}}
		public static string			ElementName {get {return "LossPrice";}}
		public override void			Poll(){
			if (IsReset)
				StopPrice =  Price;
			base.Poll();
		}
		public	double					Price;
	}
	public abstract class ConditionStop : Condition{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ConditionStop(Stobj parent, XmlNode node ):base(parent,node){}
		public abstract int				Direction{get;}
		private double					stopPrice = -1;
		public	double					StopPrice{
			get{return stopPrice;}
			set{
				stopPrice=value;
//				if (log.IsDebugEnabled) log.Debug("Stop price is "+stopPrice);
			}
		}
		public override bool			TestCondition(){
			double lastPrice = MyStrategy.Position.Symbol.Tick.Last;

			// We want the difference to be positive (or 0 for equality) when the condition is true
			double diff		= lastPrice - StopPrice;

			// When looking for a loss, flip the sign so that the loss on the long is a positive number
			diff			*= Direction;

			// If it's actually a short, flip the sign again to make the loss on the short positive
			if (MyStrategy.Position.Trip.Type == Trip.Types.Short)
				diff *= -1;

			// Ok, we have the difference and the sign correct
			// Return whether the condition has been met
			if (diff>=0)
				return true;
			else
				return false;
		}
	}
}
