using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinuxWithin.GainWatch
{
    public class Gain
    {
        private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
        public Gain( bool csv, string configFileName)
        {
            log.Info("Gain constructor");
            Global.Csv = csv;
            Global.EnableUI = false;
            Global.Configure(configFileName);
            Campaign campaign = Campaign.Make(Global.Config("QuoteSource"));
        }
    }
}
