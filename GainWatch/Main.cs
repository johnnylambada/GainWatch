//#define CATCHIT

using log4net;
using log4net.Config;
using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

[assembly: log4net.Config.DOMConfigurator(Watch=true)]

namespace LinuxWithin.GainWatch {
	public class MainApp {
		private static readonly log4net.ILog log=log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		[STAThread]
		static void Main(string[] args) {
			string FileName = null;
			string arg;
			int argn = 0;
			// Parse the command line arguments
			while ( args!=null && argn<args.Length ){
				arg = args[argn++];
				if (arg[0] == '-'){
					if (arg.Length > 1){
						switch( arg[1] ){
							case 'c':
								Global.Csv = true;
								break;
							case 'u':
								Global.EnableUI = false;
								break;
							default:
								throw new Exception("Unknown option: "+arg[1]);
						}
					} else {
						throw new Exception("Invalid option '-'");
					}
				} else {
					FileName = arg;
				}
			}

#if CATCHIT
			try {
#endif
			Global.Configure(FileName);
			Campaign campaign = Campaign.Make(Global.Config("QuoteSource"));
			UI ui = new UI();
#if CATCHIT
			} catch( Exception e){
				if (log.IsDebugEnabled)
					log.Error("Error starting application:\n"+e.Message);
				dorun = false;
			}
#endif
			if (Global.EnableUI)
				Application.Run();
		}
	}
}