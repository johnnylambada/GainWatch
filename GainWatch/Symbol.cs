using NLog;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Keeps track of a symbol
	/// </summary>
	public class Symbol{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public	double			Ask;
		public	System.Int64	AskSize;
		public	double			Bid;
		public	System.Int64	BidSize;
		private	string			_name;										// Stores the value of the Name property
		public	ArrayList		QuoteConsumers = new ArrayList();			// All IQuoteConsumers getting updates to this symbol
		private	Tick			tick = null;
		public	SortedList		Ticks = new SortedList();
		private	ArrayList		TickColumns = new ArrayList();

		/// <summary>
		/// Get the Tick at or after the time specified
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public	Tick			GetNextTick( DateTime time ){
			time=time.AddSeconds(-time.Second);
			if (Ticks.Contains(time) && Ticks[time]!=null)
				return (Tick)Ticks[time];

			// It isn't there.  Add it and find the next one
			Ticks[time] = null;
			int index = Ticks.IndexOfKey(time);

			if (index==Ticks.Count-1)				// This is the last one
				return null;

			Tick ret;
			while((ret=(Tick)Ticks.GetByIndex(index++))==null)
				if (index==Ticks.Count)
					return null;
			return ret;
		}
		/// <summary>
		/// Returns the first tick for the specified period
		/// </summary>
		/// <param name="time">Any time within the period</param>
		/// <param name="minutes">The number of minutes per period</param>
		/// <returns></returns>
		public Tick				GetFirstTickPeriod( DateTime time, int minutes ){
			if (Ticks==null)
				return null;

			int idx=0;
			Tick firstTick;
			while( (firstTick=(Tick)Ticks.GetByIndex(idx))==null )
				idx++;

			int period = ((time.Hour*60+time.Minute)-(firstTick.Time.Hour*60+firstTick.Time.Minute))/minutes;

			return	GetNextTick(firstTick.Time.AddMinutes(minutes * period));
		}
		/// <summary>
		/// Get the Tick at or before the time specified
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public	Tick			GetPrevTick( DateTime time ){
			if (Ticks.Contains(time) && Ticks[time]!=null)
				return (Tick)Ticks[time];

			// It isn't there.  Add it and find the next one
			Ticks[time] = null;
			int index = Ticks.IndexOfKey(time);

			if (index==0)				// This is the first one
				return null;

			Tick ret;
			while((ret=(Tick)Ticks.GetByIndex(--index))==null)
				if (index==0)
					return null;
			return ret;
		}
		/// <summary>
		/// Create the symbol object.  This should only happen from the Quote object.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public	static Symbol	Make(string name){
			Symbol symbol = new Symbol();
			symbol.Name = name;
			return symbol;
		}

		/// <summary>
		/// The name of the Symbol (ie QQQ)
		/// </summary>
		public	string			Name{
			get {return _name;}
			set {_name= value.ToUpper();}
		}

		/// <summary>
		/// Set the tick to a new value
		/// </summary>
		private void			setTick(Tick newTick){
			// Update the Ticks
			while( Ticks.Contains(newTick.Time ))
				newTick.Time = newTick.Time.AddMilliseconds(1);
			Ticks.Add(newTick.Time,newTick);

			tick = newTick;
		}
		/// <summary>
		/// Was the previous price skipped?
		/// </summary>
		private Tick			SkippedTick = new Tick();	// Starts as true so we always grab the first Tick
		/// <summary>
		/// No public constructor
		/// </summary>
		private					Symbol(){}
		public Tick				Tick{get{return tick;}}
		//public int				TickTimeToIndex(DateTime time){
		//	return time.Hour*60+time.Minute;
		//}
		/// <summary>
		/// Add a new point of interest to this tick
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public	void			TickPoint( string name, double valu ){
			if (!TickColumns.Contains(name))
				TickColumns.Add(name);
			if (Tick.Points==null)
				Tick.Points=new Hashtable(TickColumns.Count);
			Tick.Points[name]=valu;
		}
		public void				Update(Tick t){
			// This is where the bad data checks should go.  We'll be able to quarantine ticks and play them when
			// they are found to be good
			//if (log.IsDebugEnabled) log.Debug("last="+t.Last+" vol="+t.Volume);
			if (SkippedTick!=null){
				SkippedTick = null;	// We didn't skip this one
				setTick(t);
			} else {
				if (t.Last!=0.0){
					if ( (Math.Abs(tick.Last-t.Last)/t.Last) >0.005){
						SkippedTick=t;	// Skip this one, it's probably bogus
						if (log.IsDebugEnabled) log.Debug("Skipping a tick, was="+tick.Last+" new="+t.Last);
					} else {
						setTick(t);
					}
				}
			}
			// I should only call updated if I liked the tick.  It's the thing that tells the consumers about the
			// new quote.  An interesting question is whether I should call updated on all of the ticks that are
			// quarantined or just the tick that broke the quarantine
			Updated();
		}
		/// <summary>
		/// This symbol has been updated, tell all the consumers about it
		/// </summary>
		public	void			Updated(){
			ArrayList thisList;
			ArrayList nextList;

			// First the Indicators
			thisList = new ArrayList(QuoteConsumers);
			nextList = new ArrayList(thisList);
			foreach( IQuoteConsumer q in thisList )
				if (q is Indicator){
					q.QuoteUpdated(Name);
					nextList.Remove(q);
				}

			// Then the positions
			thisList = nextList;
			nextList = new ArrayList(thisList);
			foreach( IQuoteConsumer q in thisList )
				if (q is Position){
					q.QuoteUpdated(Name);
					nextList.Remove(q);
				}
			// Finally everything else (UI, etc)
			thisList = nextList;
			foreach( IQuoteConsumer q in thisList )
				q.QuoteUpdated(Name);
		}
		public void WriteCSV(string filename){
			StreamWriter		csv= new StreamWriter(filename,false);
			csv.WriteLine(String.Format("{0},{1:d}",Name,Global.Quotes.Date));
			string head = "Time,Last";
			foreach( string tc in TickColumns)
				head += ",\""+tc+"\"";
			csv.WriteLine(head);
			foreach( Tick t in Ticks.Values){
				if (t!=null){
					string line = string.Format("{0:hh:mm:ss},{1}",t.Time,t.Last);
					if (t.Points!=null){
						foreach( string tc in TickColumns){
							line += ",";
							if (t.Points.Contains(tc))
								line += t.Points[tc].ToString();
						}
					}
					csv.WriteLine(line);
				}
			}
			csv.Close();
		}
	}
}