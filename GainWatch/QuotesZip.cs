using NLog;
using System;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;

namespace LinuxWithin.GainWatch{

	/// <summary>
	/// File quote source - used for testing strategies against historical data sets stored in a file.
	/// </summary>
    public class QuotesZip : Quotes, IBacktest
    {
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public static List<string> Backtest(string[] args){
			if (args.Length<3)
				throw new Exception("QuotesZip: There must be at least three arguments");
			string	filename	= args[1];
			Regex	zipname		= new Regex(args[2]);
			ZipFile f	= new ZipFile(filename);
            List<string> files = new List<string>();
			foreach( ZipEntry e in f ){
				if (zipname.Match(e.Name).Success)
					files.Add(string.Format("{0},{1},{2}",args[0],args[1],e.Name));
			}
			return files;
		}
		public override void Enable() {
			base.Enable();
			long lines = 0;
			while( Stream!=null){
				lines++;
				Update( (Symbol)Symbols[(string)symbolNames[0]] );
			}
			if (log.IsDebugEnabled) log.Debug(String.Format("Finished reading file. {0} lines read",lines));
		}
		private enum				Parsed{
			Time,		Volume,
			Bid,		Ask,			Last,
			BidSize,	AskSize,		LastVolume
		}
		private						QuotesZip(){}
		public						QuotesZip( string[] args):base(){
			if (args.Length<3)
				throw new Exception("QuotesZip: There must be at least three arguments");
			string filename	= args[1];
			string zipname	= args[2];
			ZipFile f	= new ZipFile(filename);
			ZipEntry e	= f.GetEntry(zipname);

			if (e != null)
				Stream = new StreamReader(f.GetInputStream(e));
			else
				throw new Exception("Unable to open zip entry");
			symbolNames = new ArrayList();
			symbolNames.Add(zipname.Split('-')[1].ToUpper());
			Date = DateParse(zipname.Split('-')[0]);
			if (log.IsInfoEnabled)
				log.Info("Quote Source: "+filename+" ("+zipname+")");
		}
		private StreamReader		Stream;
		public override ArrayList	SymbolNames(){
			return symbolNames;
		}
        public new static Types Type { get { return Types.Backtest; } }
        public override void Update(Symbol symbol)
        {
			string line;
			line = Stream.ReadLine();
			if (line!=null){
				string[] parts = ((string)line).Split(',');
				// First update the symbol
				double d;
				if (double.TryParse(parts[(int)Parsed.Bid], NumberStyles.Any, null, out d)==true)
					symbol.Bid = d;
				if (double.TryParse(parts[(int)Parsed.Ask], NumberStyles.Any, null, out d)==true)
					symbol.Ask = d;

				// Now Create a new Tick if necessary
				Tick t = null;
				if (double.TryParse(parts[(int)Parsed.Volume], NumberStyles.Any, null, out d)==true){
					if (t==null)	t = Tick.Clone(symbol.Tick);
					t.Volume = (System.Int64) d;
				}
				if (double.TryParse(parts[(int)Parsed.Last], NumberStyles.Any, null, out d)==true){
					if (t==null)	t = Tick.Clone(symbol.Tick);
					t.Last = d;
				}
				if (t!=null && parts[(int)Parsed.Time]!=""){
					try{
						t.Time = DateTime.Parse(parts[(int)Parsed.Time]);
					} catch (Exception){}
				}
				if (t!=null && parts[(int)Parsed.LastVolume]!="")
					t.LastSize = 100*System.Int64.Parse(parts[(int)Parsed.LastVolume]);
				if (t!=null)
					symbol.Update(t);
			} else
				Stream = null;
		}
	}
}
