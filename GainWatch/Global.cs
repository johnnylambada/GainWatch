using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Singleton for global variables
	/// </summary>
	public class Global{
		private	SortedList				oConfig			= new SortedList();
		private	SortedList				strategyValues	= new SortedList();
		private	string					configFile		= null;
		private	static readonly Global	Data			= new Global();
		private Campaign				campaign		= null;
		private	bool					csv				= false;
		private	bool					enableUI		= true;
		private	string					outputDir		= string.Format(@"C:\GainStar\Results\{0:yyyyMMddHHmmss}\",DateTime.Now);
		private	ArrayList				positions		= new ArrayList();
		private	Quotes					quotes			= null;

		/// <summary>
		/// Creates the global space.
		/// </summary>
		private							Global(){}
		public static string			Config(string name){
			return (string)Data.oConfig[name];
		}
		public static Campaign			Campaign{
			get{return Data.campaign;}
			set{Data.campaign=value;}
		}
		public string					ConfigFile{
			get{return configFile;}
		}
		public static bool				Csv{
			get{return Data.csv;}
			set{Data.csv = value;}
		}
		public static bool				EnableUI{
			get{return Data.enableUI;}
			set{Data.enableUI = value;}
		}
		public static string			OutputDir{
			get{return Data.outputDir;}
		}
		public static ArrayList			Positions{
			get{return Data.positions;}
		}
		public static Quotes			Quotes{
			get{return Data.quotes;}
			set{Data.quotes = value;}
		}
		public static SortedList		StrategyValues{
			get{return Data.strategyValues;}
		}

		public static void				Configure( string fileName ){
			Directory.CreateDirectory(Data.outputDir);

			Data.configFile				= fileName;
			Match			m;
			String			line;
			StreamReader	sr			= new StreamReader(fileName);

			Regex			rComment			= new Regex( @"^\s*#");
			Regex			rNameValue			= new Regex( @"^\s*([A-Za-z0-9_]*)\s*=\s*([^#]*)");	// No dot
			Regex			rStrategyNameValue	= new Regex( @"^\s*([A-Za-z0-9_\.]*)\s*=\s*([^#]*)");	// Has a dot in the name
			while ((line=sr.ReadLine())!=null) {
				if			((m=rComment.Match(line)).Success){
				} else if	((m=rNameValue.Match(line)).Success){
					string n = m.Groups[1].ToString().Trim();
					string v = m.Groups[2].ToString().Trim();
					Data.oConfig.Add(n,v);
				} else if	((m=rStrategyNameValue.Match(line)).Success){
					string n = m.Groups[1].ToString().Trim();
					string v = m.Groups[2].ToString().Trim();
					Data.strategyValues.Add(n,v);
				}
			}
		}
	}
}