using NLog;
using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace LinuxWithin.GainWatch
{
	/// <summary>
	/// Abstract base class for all the Strategy objects
	/// </summary>
	public abstract class Stobj{
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 
		public	ArrayList			Children=null;
		private string				_description;
		public	string				Description{
			get{ return _description;	}
			set{ _description = value;	}
		}
		public virtual void				FinishMaking(){}
		protected string				GetAttribute(XmlNode node, string name){
			Match			m;
			Regex			rIndirect	= new Regex( @"^=(.*)$");
			XmlAttribute	a			= node.Attributes[name];
			if (a!=null){
				if ((m=rIndirect.Match(a.Value)).Success)
					return ((MultiValue)Global.Campaign.StrategyValues[TheStrategy.Name+"."+m.Groups[1].ToString()]).Value;
				else
					return a.Value;
			}else
				return null;
		}
		protected static string			GetStaticAttribute(XmlNode node, string name){
			XmlAttribute	a			= node.Attributes[name];
			if (a!=null)
				return a.Value;
			else
				return null;
		}
		private string				_name;
		public	string				Name{
			get{ return _name;	}
			set{ _name = value;	}
		}
		/// <summary>
		/// Gets the Type for this object
		/// </summary>
		public string				GetElementName{
			get{
				Type t = this.GetType();
				MemberInfo myMemberInfo = t.GetMethod("ElementName");
				return (string)t.InvokeMember(
					"ElementName",
					BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty,
					null,null,null
					);
			}
		}
		public	Stobj				Parent;
		private static int			_seq;
		protected					Stobj(Stobj parent, XmlNode node){
			Parent = parent;
			if ( (Name=GetAttribute(node, "Name"))==null){
				Name=GetElementName+"-"+(++_seq).ToString();
				Unnamed = true;
			}
			Description = GetAttribute(node, "Description");
		}
		protected					Stobj(){}
		public	Plan				ThePlan{
			get{
				Stobj test = this;
				while( test != null && !(test is Plan))
					test = test.Parent;
				return (Plan)test;
			}
		}
		public	Rule				TheRule{
			get{
				Stobj test = this;
				while( test != null && !(test is Rule))
					test = test.Parent;
				return (Rule)test;
			}
		}
		public	Strategy			TheStrategy{
			get{
				Stobj test = this;
				while( test != null && !(test is Strategy))
					test = test.Parent;
				return (Strategy)test;
			}
		}
		public virtual string		ToStringLine(){
			if (Unnamed)
				return GetElementName;
			else
				return GetElementName+" "+Name;
		}
		public override string		ToString() {
			string s = ToStringLine()+"\n";
			if (Children!=null)
				foreach(Stobj o in Children)
					s += o.ToString();
			return s;
		}
		public bool					Unnamed = false;
	}
}
