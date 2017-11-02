using NLog;
using System;
using System.Windows.Forms;
using System.Xml;

namespace LinuxWithin.GainWatch {
	/// <summary>
	/// The FlipType action flips the type, so if it was long it will be short and visa versa.
	/// </summary>
	public class ActionFlipType : Action{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ActionFlipType(Stobj parent, XmlNode node):base(parent,node){}
		public static string			ElementName {get {return "FlipType";}}
		public	override bool			Fire(){
			Position p = TheStrategy.Position;
			p.Type = ( (Trip.Types) ( ((int)p.Type) * -1));
			return true;
		}
	}
	public class ActionSetType : Action{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public							ActionSetType(Stobj parent, XmlNode node):base(parent,node){
			string typeName = GetAttribute(node,"To");
			if (typeName==Trip.Types.Both.ToString()){
				Type = Trip.Types.Both;
			} else if (typeName==Trip.Types.Long.ToString()){
				Type = Trip.Types.Long;
			} else if (typeName==Trip.Types.Short.ToString()){
				Type = Trip.Types.Short;
			} else {
				throw new Exception("ActionSetType: "+typeName+" is not a valid type");
			}
		}
		public static string			ElementName {get {return "SetType";}}
		public	override bool			Fire(){
			TheStrategy.Position.Type = Type;
			return true;
		}
		private Trip.Types				Type;
	}
}
