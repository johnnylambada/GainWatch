using NLog;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace System.Windows.Forms {
	/// <summary>
	/// A wrapper for the NotifyIcon class.  The Value of which is diaplayed on the task bar Icon
	/// </summary>
	public class NotifyIconNumeric : System.ComponentModel.Component {
		private static Logger log = NLog.LogManager.GetCurrentClassLogger(); 

		// External Win32 functions
		[DllImport("user32.dll")]
		static extern bool		DestroyIcon(IntPtr hIcon);

		// Private data
		private Bitmap			_bitmap = null;
		private Font			_font = null;
        private Graphics		_graphics = null;
		private NotifyIcon		_icon;    
		private const int		_size = 16;
		private double			_value = 0;

		/// <summary>
		/// The background color of the icon
		/// </summary>
		public	Color			BackgroundColor = Color.White;

		/// <summary>
		/// The forground color of the icon
		/// </summary>
		public	Color			ForgroundColor = Color.Black;

		/// <summary>
		/// The constructor creates and sets up the icon
		/// </summary>
		/// <param name="Background">The background color for the icon</param>
		/// <param name="Forground">The forground color for the icon</param>
		public NotifyIconNumeric( Color Background, Color Forground) {
			init();
			BackgroundColor = Background;
			ForgroundColor = Forground;
		}

		/// <summary>
		/// The constructor creates and sets up the icon.
		/// The background color defaults to white, and the forground color defaults to black.
		/// </summary>
		public NotifyIconNumeric(){
			init();
		}

		/// <summary>
		/// Initialize the internal objects
		/// </summary>
		private void init(){
			_bitmap		= new Bitmap(_size,_size);
			_font		= new Font("Courier New", 7);
			_graphics	= Graphics.FromImage(_bitmap);
			_icon		= new NotifyIcon();
		}

		/// <summary>
		/// Gets the NotifyIcon object
		/// </summary>
		public NotifyIcon Icon {
			get { return _icon; }
		}

		/// <summary>
		/// Setting the value of the object changes the number displayed in the task tray.
		/// It can't display a number greater than 999.99.  It can only display two decimal places.
		/// </summary>
		public double Value {
			get{ return _value; }
			set {
				_value = value;
				if (!Icon.Visible)
					return;
				IntPtr hIcon = IntPtr.Zero;
				string[] snum = value.ToString("##0.00").Split('.');
				string s = snum[0];
				_graphics.FillRectangle(new SolidBrush(BackgroundColor),_graphics.ClipBounds);	// Erase what was there

				switch (s.Length){
					case 1: s="  "+s; break;
					case 2: s=" "+s; break;
					case 3: break;
					default: s = s.Substring(s.Length-3); break;
				}
				int offset = -2;
				for( int i=0; i<3; i++){
					string c = s.Substring(i,1);
					_graphics.DrawString(c, _font, new SolidBrush(ForgroundColor),offset, -1);
					offset += 5;
				}
				s = "."+snum[1]+"000";
				offset = -2;
				for( int i=0; i<3; i++){
					string c = s.Substring(i,1);
					_graphics.DrawString(c, _font, new SolidBrush(ForgroundColor),offset, 6);
					offset += 5;
				}

				hIcon = _bitmap.GetHicon();
				if (hIcon != IntPtr.Zero){
					Icon.Icon = System.Drawing.Icon.FromHandle(hIcon);
					DestroyIcon(hIcon);
				}

				GC.Collect();
			}
		}
	}
}
