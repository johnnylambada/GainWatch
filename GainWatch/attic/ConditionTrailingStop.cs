using System;
using System.Collections;
using System.Xml;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Trailing Stop condition
	/// </summary>
	public class ConditionTrailingStop : Condition{
		public						ConditionTrailingStop( Stobj parent, XmlNode node ):base(parent,node){
		}
		public static string			Type {get {return "TRAILINGSTOP";}}
		public override bool			Test(){return false;}
	}
}
