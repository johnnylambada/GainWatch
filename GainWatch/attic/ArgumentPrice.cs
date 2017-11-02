using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Price argument
	/// </summary>
	public class ArgumentPrice : Argument{
		private static readonly log4net.ILog log=log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public	static string		ElementName {get {return "ARGPRICE";}}
		public						ArgumentPrice( Stobj parent, XmlNode node ):base(parent,node){
			Value = double.Parse(ValueString, System.Globalization.NumberStyles.Float);
		}
		public	double					Value = 0;
		public	override string			ToStringLine(){
			return base.ToStringLine()+" $"+Value.ToString();
		}
	}
}
