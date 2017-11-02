using System;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// A trip keeps track of one instance of a position, that is the price I got in and out at and the quantity
	/// </summary>
	public class Trip{
		/// <summary>
		/// The possible Types for this trip, long or short
		/// </summary>
		public enum				Types {Short=-1, Both=0, Long=1};

		public	Types			Type;
		public	object			BrokerInfo;			// Broker info specific to this tripy
		public	double			Cost=0;
		public	double			Gain{get{return ((PriceOut-PriceIn)*Quantity*((int)Type))-Cost;}}
		public	double			PriceIn=0;
		public	double			PriceOut=0;
		public	double			PriceOutLimit=0;
		public	int				Quantity=0;
		public	DateTime		TimeIn;
		public	DateTime		TimeOut;
		public Trip(){}
		public	string			Report(bool isHtml){
			string format;
			string td;
			string tdx;
			string tr;
			string trx;
			if (isHtml){
				td	= "<td>";
				tdx	= "</td>";
				tr	= "<tr>";
				trx	= "</tr>";
			} else {
				td	= "";
				tdx	= " ";
				tr	= "";
				trx	= "";
			}
			format = tr;
			format += td+"{0}"+tdx;
			format += td+"{1,5}"+tdx;
			format += td+"{2,11:c}"+tdx;
			format += td+"{3,11:c}"+tdx;
			format += td+"{4,11:c}"+tdx;
			format += td+"{5:hh:mm:ss}"+tdx;
			format += td+"{6:hh:mm:ss}"+tdx;
			format += trx;
			return String.Format(format,
				Type==Types.Long?"Long ":"Short",
				Quantity,
				PriceIn,
				PriceOut,
				Gain,
				TimeIn,
				TimeOut
			);
		}
	}
}
