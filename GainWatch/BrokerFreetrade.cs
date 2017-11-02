using NLog;
using System;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Windows.Forms;

namespace LinuxWithin.GainWatch{

	/// <summary>
	/// Abstract class covering the commonalities between brokerage houses
	/// </summary>
	public class BrokerFreetrade : Broker{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		Regex						rDV_DATA		= new Regex( "DV_DATA.*VALUE=\"(.*)\"");
		private CookieContainer		cookieJar		= new CookieContainer();
		public						BrokerFreetrade(Stack args):base(args) {
			Account = (string)args.Pop();
			Password= (string)args.Pop();
		}
		public string				Account;
		public override string		Name {get {return "Freetrade";}}
		public override bool		IsReal{get{return true;}}
		public override	bool		MarketEnterContinue(Position pos){
			throw new Exception("Not here yet");
		}
		public override bool		MarketExitContinue(Position pos){
			throw new Exception("Not here yet");
		}
		public override bool		LimitExitContinue(Position pos){
			throw new Exception("Not here yet");
		}
		public string				Password;
		protected override string	Trade( Action act,  int shares, string equity, Terms term, double price){
			throw new Exception("Not here yet");
			base.Trade(act,shares,equity,term,price);
			string				DV_DATA;
			string				ticket="";
			HttpWebRequest		req;
			HttpWebResponse		resp;
			Stream				respStream, reqStream;
			StreamReader		str;
			string				respHTML;
			Match m;
			string				data;
			byte[]				dataBytes;

			//
			// Get a DV_DATA
			//
			req =					(HttpWebRequest)WebRequest.Create("https://wwws.freetrade.com/apps/LogIn/");
			req.CookieContainer =	cookieJar;
			resp =					(HttpWebResponse)req.GetResponse();
			respStream =			resp.GetResponseStream();
			str =					new StreamReader( respStream, Encoding.ASCII );
			respHTML =				str.ReadToEnd();
			respStream.Close();

			if ( (m=rDV_DATA.Match(respHTML)).Success){
				DV_DATA = m.Groups[1].ToString();
			} else {
				throw new Exception("Could not find DV_DATA");
			}

			//
			// Now log into Freetrade, setting the cookies
			//
			data="";
			data+="pagehandler=PHLogIn";
			data+="&USERGROUP=ACCT";
			data+="&USERID="+Account;
			data+="&DV_DATA="+DV_DATA;
			data+="&PASSWORD="+Password;
			data+="&logon=Login Now";
			data+="&COMPANY=FREE";
			dataBytes =				System.Text.Encoding.ASCII.GetBytes(data);
			req =					(HttpWebRequest)WebRequest.Create("https://wwws.freetrade.com/cgi-bin/apps/LogInMain/");
			req.CookieContainer =	cookieJar;
			req.ContentLength =		dataBytes.Length;
			req.ContentType=		"application/x-www-form-urlencoded";
			req.Method =			"POST";
			reqStream =				req.GetRequestStream();
			reqStream.Write(dataBytes,0,dataBytes.Length);
			reqStream.Close();
			resp =					(HttpWebResponse)req.GetResponse();
			respStream =			resp.GetResponseStream();
			str =					new StreamReader( respStream, Encoding.ASCII );
			respHTML =				str.ReadToEnd();
			respStream.Close();
			if (log.IsDebugEnabled){
				log.Debug("LOGIN RESULT:");
				log.Debug(respHTML);
			}

			//
			// get the trading page (fill it in then send it)
			//
			data="";
			data+="mode=advanced";
			data+="&pagehandler=PHAdvancedOrderTicket";
			data+="&jsactive=true";
			data+="&special=none";
			data+="&tif=D";	//D=Day/moc=Mkt On Close/eow=End of Wk/eom=EO Month/G=GTC/gtd=Good thru Dat
			data+="&TIF_MONTH=0";
			data+="&TIF_DAY=00";
			data+="&TIF_YEAR=0";
			data+="&placeorder=\"Place Order\"";
			data+="&collisionid="+Account;
			switch(act){
				case Broker.Action.Buy: data+="&action=buy"; break;
				case Broker.Action.Cover: data+="&action=buy_cover"; break;
				case Broker.Action.Sell: data+="&action=sell"; break;
				case Broker.Action.Short: data+="&action=sellshort"; break;
			}
			data+="&qty="+shares;
			data+="&symbol="+equity;
			switch(term){
				case Broker.Terms.Limit: data+="&terms=L"; break;
				case Broker.Terms.Market: data+="&terms=M"; break;
				case Broker.Terms.Stop: data+="&terms=S"; break;
				case Broker.Terms.StopLimit: data+="&terms=X"; break;
			}
			data+="&limit_price="+price;

			dataBytes =				System.Text.Encoding.ASCII.GetBytes(data);
			req =					(HttpWebRequest)WebRequest.Create("https://wwws.freetrade.com/cgi-bin/apps/EquityOrder");
			req.CookieContainer =	cookieJar;
			req.ContentLength =		dataBytes.Length;
			req.ContentType=		"application/x-www-form-urlencoded";
			req.Method =			"POST";
			reqStream =				req.GetRequestStream();
			reqStream.Write(dataBytes,0,dataBytes.Length);
			reqStream.Close();
			resp =					(HttpWebResponse)req.GetResponse();
			respStream =			resp.GetResponseStream();
			str =					new StreamReader( respStream, Encoding.ASCII );
			respHTML =				str.ReadToEnd();
			respStream.Close();
			if (log.IsDebugEnabled){
				log.Debug("TRADE RESULT:");
				log.Debug(respHTML);
			}

			return ticket;
		}
	}
}