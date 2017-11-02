using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace LinuxWithin.GainWatch {
	public class MultiValue {
		private int							current= 0;
		private	string[]					values = null;

		public static int					Combinations( SortedList SortedListOfMultiValues ){
			int combinations = 1;
			foreach( MultiValue mv in SortedListOfMultiValues.Values )
				combinations *= mv.Length;
			return combinations;
		}
		private bool						increment(){
			if (current==values.Length-1)
				return false;
			current++;
			return true;
		}
		public int							Length{get{return values.Length;}}
		public static bool					Next( SortedList SortedListOfMultiValues ){
			foreach( MultiValue mv in SortedListOfMultiValues.Values )
				if (mv.increment())
					return true;
				else
					mv.Reset();
			return false;
		}
		public								MultiValue(string source, char delimiter){
			values = source.Split(delimiter);
			Reset();
		}
		public void							Reset(){current = 0;}
		public string						Value{get{return values[current];}}
	}
	public abstract class Campaign {
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		protected Position					Position		= null;
		public SortedList					StrategyValues	= null;

		public static Campaign		Make(string QuotesDescriptor){
			if (Quotes.TypeOf(QuotesDescriptor)==Quotes.Types.Backtest)
				return new CampaignBacktest(QuotesDescriptor);
			else
				return new CampaignRealtime(QuotesDescriptor);
		}
		protected					Campaign(string QuotesDescriptor) {
			Global.Campaign = this;
			StrategyValues = new SortedList(Global.StrategyValues.Count);
			foreach(string key in Global.StrategyValues.Keys)
				StrategyValues.Add(key, new MultiValue((string)Global.StrategyValues[key], ';'));
			if (log.IsDebugEnabled)
				log.Debug("There will be a total of "+MultiValue.Combinations(StrategyValues)+" iterations.");
		}
		protected Position			PositionLoad(Trip trip){
			Position position = new Position(
				Broker.Make(Global.Config("Broker")),
				Global.Config("PositionType")=="Short"?Trip.Types.Short:Trip.Types.Long,
				Global.Config("PositionSymbol")
				);

			if (Global.Config("PositionEnterStrategy")!=null)
				position.LoadEnterStrategy(Global.Config("PositionEnterStrategy"));

			if (trip==null){
				if (Global.Config("PositionPriceIn")!=null && Global.Config("PositionQuantity")!=null){
					position.Trip			= new Trip();
					position.Trip.PriceIn	= double.Parse(Global.Config("PositionPriceIn"));
					position.Trip.Quantity	= int.Parse(Global.Config("PositionQuantity"));
				}
			} else {
				position.Trip = trip;
			}
			
			if (Global.Config("PositionExitStrategy")!=null)
				position.LoadExitStrategy(Global.Config("PositionExitStrategy"));

			// Polling will start when quotes are enabled
			if (position.EnterStrategy!=null)
				position.State = Position.States.WaitEntry;
			else
				position.State = Position.States.WaitExit;
			
			return position;
		}
	}
	public class CampaignBacktest : Campaign {
		public						CampaignBacktest(string QuotesDescriptor):base(QuotesDescriptor){
			if (MultiValue.Combinations(StrategyValues)>1)
				backtestMany(QuotesDescriptor);
			else
				backtestOne(QuotesDescriptor);
		}
		private void				backtestOne(string QuotesDescriptor){
			List<string> backtestQuoteSources = Quotes.Backtest(QuotesDescriptor);
			if (backtestQuoteSources==null)
				throw new Exception("Backtest quote source error");

			StreamWriter html = new StreamWriter(Global.OutputDir+"index.html",false);
			html.WriteLine(@"<html><body>");
			html.WriteLine("<h1>Results</h1>");
			html.WriteLine(@"<table>");

			double total		= 0;
			double totalTrips	= 0;
			double funds		= 0;
			Trip	trip		= null;
			html.WriteLine(Position.ReportHeading());
			foreach( string q in backtestQuoteSources){
				Global.Quotes	= Quotes.Make(q);

				// Set up the position
				Position		= PositionLoad(trip);
				if (funds!=0)
					Position.Broker.Funds = funds;

				// Do the backtest
				Global.Quotes.Enable();
				trip	= Position.Trip;
				funds	= Position.Broker.Funds;						// Remember the resultant broker funds

				// Create the output for this run
				string line = Position.Report(Global.OutputDir);
				if (line!=null){
					total += Position.TripsTotal;
					totalTrips += Position.Trips.Count;
					html.WriteLine(line);
				}
				Position.Dispose();
				Position = null;
			}
			html.WriteLine(@"</table>");
			html.WriteLine(
				String.Format(
				"Total gain/(loss): <b><font color=\"{0}\">{1:c}</font></b><br>{2} trips<br>{3:c} Broker Funds",
				total>=0?"green":"red",
				total,
				totalTrips,
				funds
				)
				);
			html.WriteLine(@"</body></html>");
			html.Close();
		}
		private void				backtestMany(string QuotesDescriptor){
			StreamWriter csv = new StreamWriter(Global.OutputDir+"results.csv",false);
			string heading=MultiValue.Combinations(StrategyValues)+",";
			foreach( string key in StrategyValues.Keys )
				heading += key+",";
			heading += "msecs,wins,losses,trips,funds";
			csv.WriteLine(heading);
			DateTime startTime	= DateTime.Now;
			double seq			= 0;
			do{
				List<string> backtestQuoteSources = Quotes.Backtest(QuotesDescriptor);
				if (backtestQuoteSources==null)
					throw new Exception("Backtest quote source error");

				double totalTrips	= 0;
				double funds		= 0;
				double wins			= 0;
				double losses		= 0;
				Trip	trip		= null;
				foreach( string q in backtestQuoteSources){
					Global.Quotes	= Quotes.Make(q);

					// Set up the position
					Position		= PositionLoad(trip);
					if (funds!=0)
						Position.Broker.Funds = funds;

					// Do the backtest
					Global.Quotes.Enable();
					trip	= Position.Trip;
					funds = Position.Broker.Funds;						// Remember the resultant broker funds
					totalTrips += Position.Trips.Count;

                    foreach( Trip t in Position.Trips )
						if (t.Gain<0)
							losses++;
						else
							wins++;

					Position.Dispose();
					Position = null;

					if (funds<=0)
						break;
				}
				string detail="";
				foreach( MultiValue mv in StrategyValues.Values )
					detail += "\""+mv.Value+"\",";
				DateTime endTime	= DateTime.Now;
				TimeSpan span		= DateTime.Now-startTime;
				startTime			= endTime;
				detail = string.Format("{0},{1}\"{2:0,0}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",
					++seq,
					detail,
					span.TotalMilliseconds,
					wins,
					losses,
					totalTrips,
					funds
					);
				csv.WriteLine(detail);
				csv.Flush();
			} while( MultiValue.Next(StrategyValues));
			csv.WriteLine("\n\"Broker="+Global.Config("Broker")+"\"");
			csv.WriteLine("\"QuoteSource="+Global.Config("QuoteSource")+"\"");
			csv.Close();
		}
	}
	public class CampaignRealtime : Campaign {
		public						CampaignRealtime(string QuotesDescriptor):base(QuotesDescriptor){
			Global.Quotes	= Quotes.Make(QuotesDescriptor);
			Position		= PositionLoad(null);
			Global.Quotes.Enable();
		}
	}
}
