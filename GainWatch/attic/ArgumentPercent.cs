using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Percentage argument
	/// </summary>
	public class ArgumentPercent : Argument{
		private static readonly log4net.ILog log=log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public	static string		ElementName {get {return "ARGPERCENT";}}
		public						ArgumentPercent( Stobj parent, XmlNode node ):base(parent,node){
			Value = double.Parse(ValueString, System.Globalization.NumberStyles.Float)/100;
		}
		public	double					Value = 0;
		public	override string			ToStringLine(){
			return base.ToStringLine()+" "+(100*Value).ToString()+"%";
		}
	}
}
