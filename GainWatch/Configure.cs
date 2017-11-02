using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using LinuxWithin.GainWatch;

namespace LinuxWithin.GainWatch
{
	/// <summary>
	/// Summary description for Configure.
	/// </summary>
	public class Configure : System.Windows.Forms.Form
	{
		private static readonly log4net.ILog log=log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private bool dontLoad = false;
		private System.Windows.Forms.ListBox list;
		private bool selectable = false;
		GainWatchIcons gw;

		public Configure(GainWatchIcons g){
			gw=g;
            
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Fill the prompts
			list.Items.Clear();
			ArrayList lines = null;
			try {
				lines = Global.Data.Quotes.SymbolNames();
			} catch (Exception) {}
			if (lines!=null && lines.Count>=1){
				for( int i=0; i<lines.Count; i++){
					list.Items.Add(lines[i]);
					if (gw.Equity==(string)lines[i])
						list.SelectedIndex = i-1;
				}
			} else {
				MessageBox.Show("You must choose some active stocks in quote tracker.");
				dontLoad = true;
			}

			// Size and positoin the form
			this.Height = list.ItemHeight*list.Items.Count;
			this.Height += SystemInformation.BorderSize.Height*4;
			this.Location = new Point(Control.MousePosition.X-this.Width, Control.MousePosition.Y-this.Height);
			selectable = true;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ){
			if( disposing ){
				if(components != null){
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
			this.list = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// list
			// 
			this.list.Dock = System.Windows.Forms.DockStyle.Fill;
			this.list.Location = new System.Drawing.Point(0, 0);
			this.list.Name = "list";
			this.list.Size = new System.Drawing.Size(48, 69);
			this.list.TabIndex = 1;
			this.list.KeyDown += new System.Windows.Forms.KeyEventHandler(this.list_KeyDown);
			this.list.SelectedValueChanged += new System.EventHandler(this.control_SelectedValueChanged);
			// 
			// Configure
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(48, 72);
			this.ControlBox = false;
			this.Controls.Add(this.list);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Configure";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Configure";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.Configure_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void Configure_Load(object sender, System.EventArgs e) {
			if (dontLoad)
				this.Close();
		}

		private void control_SelectedValueChanged(object sender, System.EventArgs e) {
			if (selectable){
				gw.Equity = list.SelectedItem.ToString();
log.Debug("THE FOLLOWING LINE SHOULD PROBABLY GO AWAY");
				gw.QuoteUpdated(gw.Equity);
				this.Hide();
			}
		}

		private void list_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
			if (e.KeyCode == Keys.Escape)
				this.Close();
		}
	}
}
