using System;
using System.Drawing;
using System.Windows.Forms;

namespace LinuxWithin.GainWatch {
	/// <summary>
	/// Summary description for UI.
	/// </summary>
	public class UI : IQuoteConsumer {
		private NotifyIconNumeric			Icon			= null;
		private	DateTime					NextTime		= new DateTime(0);
		private Position					Position		= null;
		private GainWatchPositionsWindow	StatusWindow	= new GainWatchPositionsWindow();

		private void			Exit_Click(Object sender, System.EventArgs e){
			Position.Report(Global.OutputDir);
			Icon.Icon.Visible = false;
			Application.Exit();
		}
		private void			GenerateReport_Click(Object sender, System.EventArgs e){
			Position.Report(Global.OutputDir);
		}
		private void			HideWindow_Click(Object sender, System.EventArgs e) { StatusWindow.Visible = false; }
		private void			ShowWindow_Click(Object sender, System.EventArgs e) { StatusWindow.Visible = true; }
		private void			setupIcon(){
			Icon = new NotifyIconNumeric(Color.White, Color.Black);
			Icon.Icon.Visible = false;
			Icon.Icon.ContextMenu = new ContextMenu();
			Icon.Icon.ContextMenu.MenuItems.Add(new MenuItem("&Show Status Window",new System.EventHandler(ShowWindow_Click)));
			Icon.Icon.ContextMenu.MenuItems.Add(new MenuItem("&Hide Status Window",new System.EventHandler(HideWindow_Click)));
			Icon.Icon.ContextMenu.MenuItems.Add(new MenuItem("Generate &Report",new System.EventHandler(GenerateReport_Click)));
			Icon.Icon.ContextMenu.MenuItems.Add(new MenuItem("E&xit",new System.EventHandler(Exit_Click)));
			Position = (Position)Global.Positions[0];
			Global.Quotes.Register(this, Position.Symbol.Name);
			Icon.Icon.Visible		= true;
			RefreshIcon();
			StatusWindow.Visible	= true;
			RefreshStatus();
		}
		public void QuoteUpdated(string name) {
			DateTime now = DateTime.Now;
			if (now >= NextTime){
				RefreshIcon();
				RefreshStatus();
				NextTime = now.AddMilliseconds(1000);
			}
		}
		public void				RefreshIcon(){
			if (Position.Symbol.Tick != null){
				string status = Position.Symbol.Name + " @ " + Position.Symbol.Tick.Last.ToString("##0.00");
				if (Position.Symbol.Tick.Volume>0)
					status += "\n"+Position.Symbol.Tick.Volume.ToString("###,###,###,### Shares");
				Icon.Value		= Position.Symbol.Tick.Last;
				Icon.Icon.Text	= status;
			}
		}
		public void				RefreshStatus(){
			if (StatusWindow.Visible)
				StatusWindow.BackgroundImage = Position.Bitmap(StatusWindow.ClientSize);
		}
		public UI(){
			if (Global.EnableUI){
				setupIcon();
			}
		}
	}
}
