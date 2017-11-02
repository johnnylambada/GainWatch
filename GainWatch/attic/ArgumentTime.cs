using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Time argument
	/// </summary>
	public class ArgumentTime : Argument{
		private static readonly log4net.ILog log=log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public						ArgumentTime( Stobj parent, XmlNode node ):base(parent,node){
			Value = DateTime.Parse(ValueString);
		}
		public static string			ElementName {get {return "ARGTIME";}}
		public	DateTime				Value = DateTime.MinValue;
		public override string			ToStringLine(){return base.ToStringLine()+" "+Value.ToString();}
	}
}
