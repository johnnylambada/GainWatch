using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace LinuxWithin.GainWatch{

	/// <summary>
	/// File quote source - used for testing strategies against historical data sets stored in a file.
	/// </summary>
	public class QuotesTradeStationData: Quotes, IBacktest{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public static List<string>	Backtest(string[] args){
			if (args.Length<2)
				throw new Exception("QuotesZip: There must be at least two arguments");
			DirectoryInfo di = new DirectoryInfo(args[1]);
            FileInfo[] fileInfos = di.GetFiles(args[2]);
            List<string> files = new List<string>(fileInfos.Length);
            foreach (FileInfo f in fileInfos)
				files.Add(string.Format("{0},{1}",args[0],f.FullName));
            return files;
		}
		public override void Enable() {
			base.Enable ();
			long lines = 0;
			while( Stream!=null){
				lines++;
				Update( (Symbol)Symbols[(string)symbolNames[0]] );
			}
			if (log.IsDebugEnabled) log.Debug(String.Format("Finished reading file. {0} lines read",lines));
		}
		private						QuotesTradeStationData(){}
		public						QuotesTradeStationData(string[] args):base(){
			if (args.Length<2)
				throw new Exception("QuotesTradeStationData: There must be at least two arguments");
			string filename = args[1];
			Stream = new StreamReader(filename);
			Stream.ReadLine();	// Toss the first line away, it might be bogus
			FileInfo fi = new FileInfo(filename);
			symbolNames = new ArrayList();
			symbolNames.Add(fi.Name.Split('-')[0].ToUpper());
			Date = DateParse((filename.Split('-')[1]).Split('.')[0]);
			if (log.IsInfoEnabled)
				log.Info("Quote Source: "+filename);
		}
		private StreamReader		Stream;
		public override ArrayList	SymbolNames(){
			return symbolNames;
		}
        public new static Types Type { get { return Types.Backtest; } }
        public override void Update(Symbol symbol)
        {
			string line = Stream.ReadLine();
			if (line!=null){
				string[] parts = ((string)line).Split(',');
				Tick t			= new Tick();
				t.Time			= DateTime.Parse(parts[0]);
				t.Time			= t.Time.AddHours(double.Parse(parts[1].Substring(0,2)));
				t.Time			= t.Time.AddMinutes(double.Parse(parts[1].Substring(2,2)));
				t.Last			= double.Parse(parts[5]);
				symbol.Update(t);
			} else
				Stream = null;
		}
	}
}
