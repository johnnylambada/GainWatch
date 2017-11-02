using System;
using System.Collections;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// One tick of the symbol
	/// </summary>
	public sealed class Tick{

		#region properties
		public	double			Last;
		public	System.Int64	LastSize;
		public	Hashtable		Points = null;

		public	DateTime		Time;
		public	System.Int64	Volume;
		#endregion

		public					Tick(){}
		public static Tick		Clone( Tick oldTick ){
			if (oldTick==null)
				return new Tick();
			Tick nu		= (Tick) oldTick.MemberwiseClone();
			nu.Points	= null;
			return nu;
		}
	}
}