using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LinuxWithin.GainWatch{

	public interface IQuoteConsumer{
		void	QuoteUpdated( string name );
	}

	/// <summary>
	/// Abstract class covering the commonalities between Quote Sources
	/// </summary>
	public abstract class Quotes{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public static List<string>	Backtest(string Descriptor){
			string[] args = Descriptor.Split(',');
			if (args!=null || args.Length>0)
				switch(args[0]){
					case "Zip":					return QuotesZip.Backtest(args);
					case "File":				return QuotesFile.Backtest(args);
					case "TradeStationData":	return QuotesTradeStationData.Backtest(args);
                    case "IBLogger":            return QuotesIBLogger.Backtest(args);
					default:		return null;
				}
			return null;
		}
        protected Type QuoteType;
		public DateTime				Date;
		public DateTime				DateParse(string date){
			Regex r = new Regex(@"^([12][0-9][0-9][0-9])([0-1][0-9])([0-3][0-9])$");
			Match m;
			if ( (m=r.Match(date)).Success)
				return new DateTime(int.Parse(m.Groups[1].ToString()),int.Parse(m.Groups[2].ToString()),int.Parse(m.Groups[3].ToString()));
			else
				throw new Exception("'"+date+"' is not a valid date in the form YYYYMMDD");
		}
		public virtual void			Disable(){
			if (log.IsDebugEnabled)	log.Debug("Disable()ing the Quote engine");
		}
		public virtual void			Enable(){
			if (log.IsDebugEnabled)	log.Debug("Enable()ing the Quote engine");
		}
		public static Quotes		Make(string QuotesDescriptor){
			string[] args = QuotesDescriptor.Split(',');
			if (args!=null || args.Length>0)
				switch(args[0]){
					case "Zip":					return new QuotesZip(args);
					case "File":				return new QuotesFile(args);
					case "QT":					return new QuotesQT(args);
					case "TradeStationData":	return new QuotesTradeStationData(args);
                    case "IBLogger":            return new QuotesIBLogger(args);
				}
			throw new Exception("Invalid QuotesDescriptor: "+QuotesDescriptor);
		}
		protected					Quotes(){}
		public Symbol				Register(IQuoteConsumer registeree, string name){
			if (name==null)
				if (symbolNames==null || symbolNames.Count!=1){
					throw new Exception("This quote source does not support unidentified symbols");
				} else {
					name = (string)symbolNames[0];
				}
			name = name.ToUpper();
			Symbol s = (Symbol)Symbols[name];
			if (s==null){
				s = GainWatch.Symbol.Make(name);
				Symbols[s.Name] = s;
				Update(s);					// Get the price correct before revealing to symbol consumer
			}
			s.QuoteConsumers.Add(registeree);
			return s;
		}
		protected ArrayList			symbolNames = null;
		public abstract ArrayList	SymbolNames();
		protected Hashtable			Symbols = new Hashtable();
		public Symbol				Symbol(string name){
			name = name.ToUpper();
			Symbol s = (Symbol)Symbols[name];
			if (s!=null)
				return s;
			return null;
		}
		public enum					Types {Backtest,Realtime};
        //public static Types Type { get { throw new Exception("Can't call base class Type()"); } }
		public static Types			TypeOf(string QuotesDescriptor){
			string[] args = QuotesDescriptor.Split(',');
			if (args!=null || args.Length>0)
				switch(args[0]){
					case "Zip":					return QuotesZip.Type;
					case "File":				return QuotesFile.Type;
					case "QT":					return QuotesQT.Type;
					case "TradeStationData":	return QuotesTradeStationData.Type;
                    case "IBLogger":            return QuotesIBLogger.Type;
				}
			throw new Exception("Invalid QuotesDescriptor: "+QuotesDescriptor);
		}
		public void					UnRegister( IQuoteConsumer registeree, Symbol symbol ){
			symbol.QuoteConsumers.Remove(registeree);
			if (symbol.QuoteConsumers.Count==0)
				Symbols.Remove(symbol.Name);
		}
		/// <summary>
		/// The update function creates a new tick to send to the symbol.  The symbol may accept, reject
		/// or quarantine the tick.
		/// </summary>
		/// <param name="symbol"></param>
		public abstract void			Update( Symbol symbol );
	}
}
