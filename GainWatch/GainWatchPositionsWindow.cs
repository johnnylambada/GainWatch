using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace LinuxWithin.GainWatch
{
	/// <summary>
	/// Summary description for GainWatchPositionsWindow.
	/// </summary>
	public class GainWatchPositionsWindow : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public GainWatchPositionsWindow()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// GainWatchPositionsWindow
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(112, 112);
			this.ControlBox = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GainWatchPositionsWindow";
			this.Opacity = 0.55;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.TopMost = true;
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GainWatchPositionsWindow_MouseDown);

		}
		#endregion

		/// <summary>
		/// Move the status window when it is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GainWatchPositionsWindow_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			const int WM_SYSCOMMAND = 0x112;
			const int MOUSE_MOVE = 0xF012;
			Message msg = new Message();
			msg.HWnd = this.Handle;
			msg.Msg = WM_SYSCOMMAND;
			msg.WParam = new IntPtr(MOUSE_MOVE);
			this.Capture = false;
			this.WndProc(ref msg);
		}

	}
}
