using NLog;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Represents a trade that's underway
	/// </summary>
	public class Position : IQuoteConsumer{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 

		#region enumerations
		/// <summary>
		/// The possible states of the position
		/// Setup		- The position is still being set up
		/// GetIn		- The position is not yet taken, we're waiting for entry
		/// GetOut		- We're in the position, waiting to get out.
		/// Complete	- We've exited the position.
		/// </summary>
		public enum			States { Setup, WaitEntry, WaitExit, Complete };

		#endregion

		#region data
		public Broker		Broker;					// The brokerage house where this position is kept.
		public Strategy		EnterStrategy = null;	// How do we get into the position
		public Strategy		ExitStrategy = null;	// How do we get out
		public States		state = States.Setup;	// The current state of the position
		public Symbol		Symbol = null;			// The Symbol that this position is in
		public Trip			Trip;					// The round trip we're working on
		public ArrayList	Trips=new ArrayList();	// All of the trips
		public Trip.Types	Type;					// Are we long or short on this position
		#endregion

		/// <summary>
		/// The Bitmap function returns a bitmap that respresents the status of this position.  You get
		/// different information depending on the size of the bitmap that you ask for.
		/// </summary>
		/// <param name="s">The size of the requested bitmap</param>
		/// <returns></returns>
		public	Bitmap		Bitmap(Size s){
			//TODO:	This function can be tightened up quite a bit.  There are some constants in here, such as font size
			//		and such that should be calculated based on the size of the bitmap requested.  Depending on the amount
			//		of time it takes to calc this, it might also be nice to cache precalc'ed values and bitmap based on the
			//		last size requested.

			//TODO:	Create a Stobj function that will return the name without the -NUMBER part
			Color bg			= Color.White;
			Color fg			= Color.Black;

			Bitmap b			= new Bitmap(s.Width, s.Height);
			Graphics g			= Graphics.FromImage(b);
			Brush br			= new SolidBrush(fg);
			Font bigFont		= new Font("Arial", 12, FontStyle.Bold);
			Font medFont		= new Font("Arial", 8);
			Font lilFont		= new Font("Arial", 6);

			// Draw the various lines
			g.FillRectangle(new SolidBrush(bg),g.ClipBounds);	// Erase what was there
			g.DrawRectangle(new Pen(br,1),0,0,s.Width-1,s.Height-1);
			g.DrawLine(new Pen(br,1), new Point(s.Width/2,0), new Point(s.Width/2,s.Height-1));
			g.DrawLine(new Pen(br,1), new Point(3*s.Width/4,0),new Point(3*s.Width/4,s.Height-1));

			// Fill in the blanks
			g.DrawString(Symbol.Name, bigFont, br, 0, 0);
			g.DrawString(Broker.Name, medFont, br, 0, 1*s.Height/4);
			g.DrawString(State.ToString(), medFont, br, 0, 2*s.Height/4);
			if (Symbol.Tick!=null)
				g.DrawString(Symbol.Tick.Last.ToString("C"), medFont, br, 0, 3*s.Height/4);

			// Draw the strategy stop points
			SortedList sl = null;
			if (this.State==States.WaitEntry)
				sl = EnterStrategy.DataPoints();
			else if (this.State==States.WaitExit)
				sl = ExitStrategy.DataPoints();

			if (sl!=null && sl.Count>0){
				if (!sl.ContainsKey(Symbol.Tick.Last))
					sl.Add(Symbol.Tick.Last,	new StrategyDataPoint("NOW",Symbol.Tick.Last));
				if (Trip!=null && !sl.ContainsKey(Trip.PriceIn))
					sl.Add(Trip.PriceIn,		new StrategyDataPoint("In At",Trip.PriceIn));
				try {
					StringFormat nameFormat = new StringFormat();
					nameFormat.Alignment = StringAlignment.Far;
					double min = ((StrategyDataPoint) sl.GetByIndex(0)).Value;
					double max = ((StrategyDataPoint) sl.GetByIndex(sl.Count-1)).Value;
					if (min!=max){
						for(int i=0; i<sl.Count; i++){
							StrategyDataPoint dp = ((StrategyDataPoint) sl.GetByIndex(i));
							int y = 1+((int)( ((s.Height*0.9)*(max-dp.Value))/(max-min)));
							g.DrawString(dp.Name,
								lilFont, 
								br, 
								new RectangleF(new Point(1+2*s.Width/4,y),
								new SizeF(s.Width/4-2,lilFont.GetHeight(g))),
								nameFormat
								);
							g.DrawString(dp.Value.ToString("C"),lilFont, br, 1+3*s.Width/4, y);
						}
					}
				} catch{}
			}
			return b;
		}
		
		public void Dispose(){
			Global.Quotes.UnRegister(this, this.Symbol);
			Global.Positions.Remove(this);
		}
		/// <summary>
		/// Load the entry strategy from a strategy file
		/// </summary>
		/// <param name="fileName"></param>
		public	void		LoadEnterStrategy( string fileName ){	EnterStrategy = Strategy.Load(this,fileName);}

		/// <summary>
		/// Load the exit strategy from a strategy file
		/// </summary>
		/// <param name="fileName"></param>
		public	void		LoadExitStrategy( string fileName ){	ExitStrategy = Strategy.Load(this,fileName);	}

		public void QuoteUpdated(string name) {
			switch (State){
				case States.Setup:
					break;
				case States.WaitEntry:
					if (EnterStrategy != null)
						EnterStrategy.Poll();
					break;
				case States.WaitExit:
					if (ExitStrategy != null)
						ExitStrategy.Poll();
					break;
				case States.Complete:
					break;
			}
			if (Trip!=null)
				Symbol.TickPoint(Trip.Type.ToString(),Trip.PriceIn);
		}

		/// <summary>
		/// The position constructor
		/// </summary>
		/// <param name="broker">the broker to use for this position</param>
		/// <param name="type">Types.Long or Types.Short</param>
		/// <param name="symbol">What is the symbol for the stock for this position</param>
		public Position(Broker broker, Trip.Types type, string symbol){
			Broker			= broker;
			Type			= type;
			Symbol			= Global.Quotes.Register(this, symbol);
			Global.Positions.Add(this);
		}
		public string Report( string outputDirectory ){
			// If no activity, ignore
			if (Trips.Count==0)
				return null;

			string fileName = string.Format("{0}-{1:yyyyMMdd}",Symbol.Name,Global.Quotes.Date);
			string filePath = string.Format("{0}{1}",outputDirectory,fileName);

			// The HTML
			StreamWriter html = new StreamWriter(filePath+".html",false);
			html.WriteLine(@"<html><body>");
			html.WriteLine("<h1>Trades</h1>");
			html.WriteLine(@"<table>");
			double	gain = 0;
			long	trips = 0;
			foreach( Trip t in Trips ){
				html.WriteLine(t.Report(true));
				gain += t.Gain;
				trips++;
			}
			html.WriteLine(@"</table>");
			html.WriteLine(
				String.Format(
					"Total gain/(loss): <b><font color=\"{0}\">{1:c}</font></b> on {2}<br>",
					gain>=0?"green":"red",
					gain,
					Symbol.Name
				)
			);
			if (Trip!=null){
				html.WriteLine("<h1>Unfinished</h1>");
				html.WriteLine(@"<table>");
				html.WriteLine(Trip.Report(true));
				html.WriteLine(@"</table>");
			}
			if (EnterStrategy!=null){
				html.WriteLine("<h1>Entry Strategy</h1>");
				html.WriteLine("<pre>"+EnterStrategy.ToString()+"</pre>");
			}
			if (ExitStrategy!=null){
				html.WriteLine("<h1>Exit Strategy</h1>");
				html.WriteLine("<pre>"+ExitStrategy.ToString()+"</pre>");
			}
			html.WriteLine(@"</body></html>");
			html.Close();

			// The CSV
			if (Global.Csv)
				Symbol.WriteCSV(filePath+".csv");

			string s = "";
			if (true)		s+=	string.Format("<tr>");
			if (true)		s+=	string.Format(	"<td><a href='{0}'>{1}</a></td>",	fileName+".html",	Symbol.Name);
			if (true)		s+=	string.Format(	"<td align='right'><b><font color=\"{0}\">{1:c}</font></b></td>", gain>=0?"green":"red", gain);
			if (true)		s+=	string.Format(	"<td align='right'>{0:0,0}</td>",Symbol.Tick.Volume);
			if (true)		s+=	string.Format(	"<td align='right'>{0:0,0}</td>",Symbol.Ticks.Count);
			if (true)		s+=	string.Format(	"<td align='right'>{0}</td>",Trips.Count);
			if (true)		s+=	string.Format(	"<td align='right'>{0:c}</td>",Broker.Funds);
			if (Global.Csv)	s+=	string.Format(	"<td>{0}</td></td>",filePath+".csv");
			if (true)		s+=	string.Format("</tr>");
			return s;
		}
		public static string ReportHeading(){
			string s ="";
			if (true)		s+=	"<tr>";
			if (true)		s+=	"	<th>Symbol</th>";
			if (true)		s+=	"	<th align='right'>Gain</th>";
			if (true)		s+=	"	<th align='right'>Volume</th>";
			if (true)		s+=	"	<th align='right'>Ticks</th>";
			if (true)		s+=	"	<th align='right'>Trips</th>";
			if (true)		s+=	"	<th align='right'>EOD<br>Funds</th>";
			if (Global.Csv)	s+=	"	<th align='left'>CSV</th>";
			if (true)		s+=	"</tr>";
			return s;
		}
		public States State{
			get{ return state; }
			set{
//				try{
					switch(value){
						case States.WaitEntry:	
							EnterStrategy.Reset();
							break;
						case States.WaitExit:	ExitStrategy.Reset();	break;
						case States.Complete:
							if (log.IsInfoEnabled){
								double gain = 0;
								foreach( Trip t in Trips ){
									log.Info(t.Report(false));
									gain += t.Gain;
								}
								log.Info(String.Format("Total gain/(loss): {0:c} on {1}",gain,Symbol.Name));
								log.Info("-------------------------------");
							}
							break;
					}
/*				} catch (Exception e){
					throw new Exception("Cannot change state to "+value.ToString()+": "+e.Message);
				}
*/				state = value;
			}
		}
		/// <summary>
		/// Create a new trip
		/// </summary>
		/// <returns></returns>
		public Trip			TripNew(){
			if (Trip!=null)
				throw new Exception("There shouldn't be a trip here");
			Trip			= new Trip();
			Trip.Type		= Type;
			Trip.TimeIn		= Symbol.Tick.Time;
			Trip.Quantity	= (int) (100 * Math.Floor( Broker.Funds / Symbol.Tick.Last / 100));
			Trip.Cost		= Broker.Cost(Trip);
			if (Trip.Quantity==0)
				Trip.Quantity = (int) Math.Floor( Broker.Funds / Symbol.Tick.Last );
			return Trip;
		}
		/// <summary>
		/// Finish out the trip
		/// </summary>
		public void			TripFinish(){
			if (log.IsDebugEnabled) 
				log.Debug(Trip.Report(false));
			Trip.BrokerInfo = null;
			Trips.Add(Trip);			// Save the trip here
			Symbol.TickPoint(Trip.Type.ToString(),Trip.PriceOut);
			Trip = null;
		}
		public double	TripsTotal{
			get{
				double gain = 0;
				foreach( Trip t in Trips )
					gain += t.Gain;
				return gain;
			}
		}
	}
}
