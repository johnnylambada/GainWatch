using NLog;
using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace LinuxWithin.GainWatch{

	/// <summary>
	/// Quote Tracker quote source.  QuoteTracker can be used as a quote source. Currently
	/// we only use the GetLastQuote function, so here is the docs for that function:
	/// See http://www.quotetracker.com/qtserver.htm for the full details
	/// GetLastQuote(Ticker,Ticker,Ticker....) or GetLastQuote(*)
	/// Description: Returns "last quote" info for the specified tickers. Special Parameters:
	/// *       - returns quotes on all stocks in the system
	/// CURRENT - (V2.4.9C+) Returns quotes on all symbols in the current portfolio on the main QT screen.
	/// ACTIVE  - (V2.4.9C+) Returns quotes on symbols in all portfolios
	/// Returns
	/// Tckr,Dt,Tme,Lst,Bid,Ask,Chng,Tck,Vol,Hi,Low,BidSz,AskSz,LstVol,AvgTrd,Trds,Open,52WkLo,52WkHi
	/// 0000,01,002,003,004,005,0006,007,008,09,010,00011,00012,000013,000014,0015,0016,000017,000018
	/// </summary>
	public class QuotesQT : Quotes{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		private enum Parsed{
			Ticker,		Date,				Time,			Last,
			Bid,		Ask,				Change,			Tick,
			Volume,		High,				Low,			BidSize,
			AskSize,	LastVolume,			AvgTrade,		NumTrades,
			Open,		FiftyTwoWeekLow,	FiftyTwoWeekHigh
		}
		private Hashtable lastLine = new Hashtable();
		public override void Update( Symbol symbol ){
			ArrayList lines=null;
			try {
				lines = http("http://127.0.0.1:16239/Req?GetLastQuote("+symbol.Name+")");
			} catch ( Exception ){}
			if (lines!=null && lines.Count>=1){
				string line = (string) lines[0];
				// Don't send duplicate lines to symbol - it messes up error detection
				if (!lastLine.Contains(symbol.Name) || (string)lastLine[symbol.Name]!=line){
					lastLine[symbol.Name] = line;
					string[] parts = ((string)lines[0]).Split(',');
					Tick t		= new Tick();
					t.Time		= DateTime.Parse(parts[(int)Parsed.Time]);
					t.Last		= double.Parse(parts[(int)Parsed.Last]);
					t.Volume	= System.Int64.Parse(parts[(int)Parsed.Volume]);
					symbol.Bid	= double.Parse(parts[(int)Parsed.Bid]);
					symbol.Ask	= double.Parse(parts[(int)Parsed.Ask]);
					symbol.Update(t);
				}
			}
		}
		public override ArrayList SymbolNames(){
			if (symbolNames==null){
				symbolNames	= new ArrayList();
				ArrayList lines		= http(@"http://127.0.0.1:16239/Req?GetLastQuote(CURRENT)");
				if (lines.Count>=1){
					for(int i=0;i<lines.Count;i++){
						string[] parts = ((string)lines[i]).Split(',');
						symbolNames.Add(parts[(int)Parsed.Ticker]);
					}
				}
			}
			return symbolNames;
		}
		/// <summary>
		/// Go get a status page from quote tracker through it's web interface
		/// </summary>
		/// <param name="uri">The URI from which to retrieve the web page</param>
		/// <returns>The web page</returns>
		private	ArrayList		http( string uri ){
			WebClient		wc = new WebClient();
			byte[] bPage;
			string page;
			ArrayList lines = null;
			bPage	= wc.DownloadData(uri);
			page	= Encoding.ASCII.GetString(bPage).Replace("\r","");
			lines	= new ArrayList(page.Split('\n'));
			if (lines.Count<1 || ((string)lines[0])!="OK")
				throw new Exception(((lines.Count>0)?((string) lines[0]):""));
			lines.RemoveAt(0);
			return lines;
		}
		private						QuotesQT(){}
		public						QuotesQT(string[] args):base(){
			_timer.Interval = 1000;
			_timer.Tick +=new EventHandler(timer_Tick);
			_timer.Start();
			Date = DateTime.Now;
			if (log.IsInfoEnabled)
				log.Info("Quote Source: QuoteTracker Live Data");
		}
		private Timer				_timer = new Timer();
		/// <summary>
		/// This function is called 1x/second.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void			timer_Tick(object sender, EventArgs e) {
			_timer.Stop();
			foreach( Symbol s in Symbols.Values )
				Update(s);
			_timer.Start();
		}
        public new static Types Type { get { return Types.Backtest; } }
    }
}
