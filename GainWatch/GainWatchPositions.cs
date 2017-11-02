using System;
using System.Drawing;
using System.IO;

namespace LinuxWithin.GainWatch {
	public class GainWatchPositions {
		private static readonly log4net.ILog log=log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private double						Funds			= double.MinValue;
		private Position					Position		= null;

		private void			setupPosition(){
			if (Position!=null)
				Global.Positions.Remove(Position);
			Position = new Position(
				Broker.Make(Global.Config("Broker")),
				Global.Config("PositionType")=="Short"?Trip.Types.Short:Trip.Types.Long,
				Global.Config("PositionSymbol")
				);
			Global.Positions.Add(Position);

			// Use the funds from the previous run
			if (Funds!=double.MinValue)
				Position.Broker.Funds = Funds;

			if (Global.Config("PositionEnterStrategy")!=null)
				Position.LoadEnterStrategy(Global.Config("PositionEnterStrategy"));

			if (Global.Config("PositionPriceIn")!=null && Global.Config("PositionQuantity")!=null){
				Position.Trip			= new Trip();
				Position.Trip.PriceIn	= double.Parse(Global.Config("PositionPriceIn"));
				Position.Trip.Quantity	= int.Parse(Global.Config("PositionQuantity"));
			}
			
			if (Global.Config("PositionExitStrategy")!=null)
				Position.LoadExitStrategy(Global.Config("PositionExitStrategy"));

			if (log.IsDebugEnabled && Position.EnterStrategy!=null)
				log.Debug("------------Enter Strategy:------------\n"+Position.EnterStrategy.ToString());

			if (log.IsDebugEnabled && Position.ExitStrategy!=null)
				log.Debug("------------Exit Strategy:------------\n"+Position.ExitStrategy.ToString());

			// Polling will start when quotes are enabled
			if (Position.EnterStrategy!=null)
				Position.State = Position.States.WaitEntry;
			else
				Position.State = Position.States.WaitExit;
		}
		/// <summary>
		/// The constructor creates and sets up the position and the timer
		/// TODO: THIS STUFF SHOULD GO IN THE CAMPAIGN.  IT SHOULD JUST HAVE AN AMOUNT OF MONEY IN THE BROKERAGE ACCOUNT TO USE
		/// </summary>
		public					GainWatchPositions() {
			string[] backtestQuoteSources = Quotes.Backtest(Global.Config("QuoteSource"));
			if (backtestQuoteSources!=null){

				string filename = string.Format("{0}index.html",Global.OutputDir);
				StreamWriter html = new StreamWriter(Global.OutputDir+"index.html",false);
				html.WriteLine(@"<html><body>");
				html.WriteLine("<h1>Results</h1>");
				html.WriteLine(@"<table>");

				double total=0;
				double totalTrips = 0;
				bool printHeading=true;
				foreach( string q in backtestQuoteSources){
					Position position = Run(q);
					if (printHeading){
						printHeading=false;
						html.WriteLine(Position.ReportHeading());
					}

					// Create the output for this run
					string line = position.Report(Global.OutputDir);
					if (line!=null){
						total += position.TripsTotal;
						totalTrips += position.Trips.Count;
						html.WriteLine(line);
					}
				}
				html.WriteLine(@"</table>");
				html.WriteLine(
					String.Format(
					"Total gain/(loss): <b><font color=\"{0}\">{1:c}</font></b><br>{2} trips<br>{3:c} Broker Funds",
					total>=0?"green":"red",
					total,
					totalTrips,
					Position.Broker.Funds
					)
					);
				html.WriteLine(@"</body></html>");
				html.Close();
			} else
				Run(Global.Config("QuoteSource"));
			if (!Global.EnableUI)
				Global.Run = false;			// Don't run, just exit
		}
		private Position			Run( string quoteSource ){
			// Set up the quote source
			Global.Quotes	= Quotes.Make(quoteSource);

			// Set up the position
			setupPosition();

			// Run the backtest (if we're backtesting)
            Global.Quotes.Enable();		// If the quote engine is file based, then the file is read in here

			// Remember the funds for next time (when backtesting)
			Funds = Position.Broker.Funds;
			return Position;
		}
	}
}
