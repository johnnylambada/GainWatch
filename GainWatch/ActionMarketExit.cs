using NLog;
using System;
using System.Windows.Forms;
using System.Xml;

namespace LinuxWithin.GainWatch {
	/// <summary>
	/// Action to enter the position at the market price
	/// </summary>
	public class ActionMarketEnter : Action{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ActionMarketEnter(Stobj parent, XmlNode node):base(parent,node){}
		public static string			ElementName {get {return "MarketEnter";}}
		public	override bool			Fire(){
			Position pos = MyStrategy.Position;
			pos.TripNew();
			return pos.Broker.MarketEnter(pos);
		}
		public override bool			FireAgain() {
			Position pos = MyStrategy.Position;
			return pos.Broker.MarketEnterContinue(pos);
		}
	}
	/// <summary>
	/// Action to exit the position at the market price
	/// </summary>
	public class ActionMarketExit : Action{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ActionMarketExit(Stobj parent, XmlNode node):base(parent,node){}
		public static string			ElementName {get {return "MarketExit";}}
		public	override bool			Fire(){
			Position pos		= MyStrategy.Position;
			if (pos.Trip==null)
				return true;
			pos.Trip.BrokerInfo	= null;
			return pos.Broker.MarketExit(pos);
		}
		public override bool			 FireAgain() {
			Position pos = MyStrategy.Position;
			return pos.Broker.MarketExitContinue(pos);
		}
	}
	/// <summary>
	/// Action to exit the position at the limit price
	/// </summary>
	public class ActionLimitExit : Action{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ActionLimitExit(Stobj parent, XmlNode node):base(parent,node){
			Percent = double.Parse(GetAttribute(node,"Percent"))/100;
		}
		public static string			ElementName {get {return "LimitExit";}}
		public	override bool			Fire(){
			Position pos		= MyStrategy.Position;
			return pos.Broker.LimitExit(pos,Percent);
		}
		public override bool			 FireAgain() {
			Position pos = MyStrategy.Position;
			return pos.Broker.LimitExitContinue(pos);
		}
		public	double					Percent;
	}
}
