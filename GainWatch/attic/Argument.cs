using System;
using System.Xml;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// Argument
	/// </summary>
	public abstract class Argument : Stobj{
		private static readonly log4net.ILog log=log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public					Argument( Stobj parent, XmlNode node ):base(parent,node){
			Prompt =		GetAttribute( node, "Prompt");
			ValueString =	GetAttribute( node, "Value");
		}
		public static Argument	Make(Stobj parent, XmlNode node){
			Argument a;
			if		(node.Name==ArgumentPercent.ElementName)	a = new ArgumentPercent(parent,node);
			else if (node.Name==ArgumentTime.ElementName)		a = new ArgumentTime(parent,node);
			else if (node.Name==ArgumentPrice.ElementName)		a = new ArgumentPrice(parent,node);
			else	throw new Exception(node.Name+" found where ARGUMENT expected");
			return a;
		}
		private String				_prompt = null;
		public	String				Prompt{
			get{ return _prompt;	}
			set{ _prompt = value;	}
		}
		private String				_valueString = null;
		public	String				ValueString{
			get{ return _valueString;	}
			set{ _valueString = value;	}
		}
		public override string		ToStringLine(){return "     "+base.ToStringLine();}
	}
}
