using System;

namespace LinuxWithin.GainWatch{
	/// <summary>
	/// One tick of the symbol
	/// </summary>
	public sealed class Tick{
		public Tick 			Next;
		public					Tick(){}
		public static Tick		Clone( Tick oldTick ){
			if (oldTick==null)
				return new Tick();
			Tick nu = (Tick) oldTick.MemberwiseClone();
			nu.Next = null;
			return nu;
		}
		public bool				IsComplete(){
			return 	AskSet && AskSizeSet && BidSet && BidSizeSet && LastSet && TimeSet && VolumeSet;
		}

		public bool				TimeSet=false;
		private	DateTime		time;
		public	DateTime		Time{
			get{if (TimeSet) return time; else throw new Exception("This tick has no Time");}
			set{time=value; TimeSet=true;}
		}

		public bool				LastSet=false;
		private	double			last;
		public	double			Last{
			get{
				if (LastSet) 
					return last; 
				else 
					throw new Exception("This tick has no Last");
			}
			set{
				last=value;
				LastSet=true;
			}
		}

		public bool				VolumeSet=false;
		private	System.Int64	volume;
		public	System.Int64	Volume{
			get{if (VolumeSet) return volume; else throw new Exception("This tick has no Volume");}
			set{volume=value; VolumeSet=true;}
		}

		public bool				BidSet=false;
		private	double			bid;
		public	double			Bid{
			get{if (BidSet) return bid; else throw new Exception("This tick has no Bid");}
			set{bid=value; BidSet=true;}
		}

		public bool				BidSizeSet=false;
		private	System.Int64	bidSize;
		public	System.Int64	BidSize{
			get{if (BidSizeSet) return bidSize; else throw new Exception("This tick has no BidSize");}
			set{bidSize=value; BidSizeSet=true;}
		}

		public bool				AskSet=false;
		private	double			ask;
		public	double			Ask{
			get{if (AskSet) return ask; else throw new Exception("This tick has no Ask");}
			set{ask=value; AskSet=true;}
		}

		public bool				AskSizeSet=false;
		private	System.Int64	askSize;
		public	System.Int64	AskSize{
			get{if (AskSizeSet) return askSize; else throw new Exception("This tick has no AskSize");}
			set{askSize=value; AskSizeSet=true;}
		}
	}
}