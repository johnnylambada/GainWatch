using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LinuxWithin.GainWatch
{

    /// <summary>
    /// File quote source - used for testing strategies against historical from IB.
    /// </summary>
    public class QuotesIBLogger : Quotes, IBacktest
    {
        private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
        private StreamReader Stream;
        public override ArrayList SymbolNames()
        {
            return symbolNames;
        }
        public new static Types Type { get { return Types.Backtest; } }
        private Tick tickPrototype = new Tick();
        private Regex sizeRegex = new Regex(@"^. ([-0-9]*)$");
        private Regex priceRegex = new Regex(@"^. ([-0-9.]*)$");
        private Regex timeRegex = new Regex(@"^T ([0-9][0-9])([0-9][0-9])([0-9][0-9])-([0-9][0-9])([0-9][0-9])([0-9][0-9])$");

        #region Backtest
        public static List<string> Backtest(string[] args)
        {
            if (args.Length < 2)
                throw new Exception("QuotesIBLogger: There must be at least two arguments");
            DirectoryInfo di = new DirectoryInfo(args[1]);
            FileInfo[] fileInfos = di.GetFiles(args[2]);
            List<string> files = new List<string>(fileInfos.Length);
            foreach (FileInfo f in fileInfos)
                files.Add(string.Format("{0},{1}", args[0], f.FullName));
            return files;
        }
        #endregion
        #region Enable
        public override void Enable()
        {
            base.Enable();
            while (Stream != null)
            {
                Update((Symbol)Symbols[(string)symbolNames[0]]);
            }
            if (log.IsDebugEnabled)
                log.Debug(String.Format("Finished reading file."));
        }
        #endregion
        #region QuotesIBLogger
        private QuotesIBLogger() { }
        /// <summary>
        /// Constructor for QuotesIBLogger
        /// </summary>
        /// <param name="args">Arguments that create the Quotes machine.</param>
        public QuotesIBLogger(string[] args): base(){
            if (args.Length < 2)
                throw new Exception("QuotesIBLogger: There must be at least two arguments");
            string filename = args[1];
            Stream = new StreamReader(filename);
            FileInfo fi = new FileInfo(filename);
            symbolNames = new ArrayList();
            symbolNames.Add(fi.Name.Split('-')[0].ToUpper());
            Date = DateParse("20"+(fi.Name.Split('-')[1]));
            if (int.Parse((fi.Name.Split('-')[2]).Substring(0, 2)) >= 16)
                Date = Date.AddDays(1);
            if (log.IsInfoEnabled)
                log.Info("Quote Source: " + filename);
        }
        #endregion
        #region Update
        public override void Update(Symbol symbol)
        {
            Match priceMatch, sizeMatch, timeMatch;
            for(bool cont=true;cont;){
                string line = Stream.ReadLine();
                if (line == null)
                {
                    Stream = null;
                    break;
                }
                priceMatch = priceRegex.Match(line);
                sizeMatch = sizeRegex.Match(line);
                timeMatch = timeRegex.Match(line);
                switch (line[0])
                {
                    case '1': symbol.Bid = double.Parse(priceMatch.Groups[1].ToString()); break;
                    case '2': symbol.Ask = double.Parse(priceMatch.Groups[1].ToString()); break;
                    case '4': tickPrototype.Last = double.Parse(priceMatch.Groups[1].ToString()); cont = false;  break;
                    case '5': tickPrototype.LastSize = long.Parse(priceMatch.Groups[1].ToString()); break;
                    case '8': tickPrototype.Volume = long.Parse(priceMatch.Groups[1].ToString()); break;
                    case 'T': 
                        tickPrototype.Time = new DateTime(
                        2000 + int.Parse(timeMatch.Groups[1].ToString()),
                        int.Parse(timeMatch.Groups[2].ToString()),
                        int.Parse(timeMatch.Groups[3].ToString()),
                        int.Parse(timeMatch.Groups[4].ToString()),
                        int.Parse(timeMatch.Groups[5].ToString()),
                        int.Parse(timeMatch.Groups[6].ToString()));
                        break;
                }
            }
            if (log.IsTraceEnabled)
            {
                string s = string.Format("tick:{0:s},{1},{2}", tickPrototype.Time, tickPrototype.Last, tickPrototype.Volume);
                s = s.Replace('T', ' ');
                log.Trace(s);
            }
            symbol.Update(Tick.Clone(tickPrototype));
        }
        #endregion
    }
}
