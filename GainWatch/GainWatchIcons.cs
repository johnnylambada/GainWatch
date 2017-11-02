using System;
using System.Drawing;
using System.Windows.Forms;

namespace LinuxWithin.GainWatch {
	public class GainWatchIcons : IQuoteConsumer {
		private static readonly log4net.ILog log=log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private NotifyIconNumeric	_icon		= null;
		private Symbol				_symbol		= null;
		private	DateTime			nextTime	= new DateTime(0);

		/// <summary>
		/// The user clicked the configure menu option
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void			Configure_Click(Object sender, System.EventArgs e) {
			new LinuxWithin.GainWatch.Configure(this).Show();
		}

		/// <summary>
		/// The user clicked the exit menu option
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void			Exit_Click(Object sender, System.EventArgs e) {
			_icon.Icon.Visible = false;
			Application.Exit();
		}
		
		public String			Equity{
			get{ return _symbol.Name; }
			set{
				if (_symbol!=null)
					Global.Data.Quotes.UnRegister(this,_symbol);
				_symbol = Global.Data.Quotes.Register(this, value);
				if (_icon!=null)
					_icon.Icon.Text = value;
			}
		}
		/// <summary>
		/// The constructor creates and sets up the icon, it's menus and the timer
		/// </summary>
		public					GainWatchIcons() {
			try {
				Equity = (string) Global.Data.Quotes.SymbolNames()[0];		// The first one in the list
			} catch (Exception e){
				MessageBox.Show("Unable to start application:\n - "+e.Message);
				throw e;
			}

			_icon = new NotifyIconNumeric(Color.White, Color.Black);
			_icon.Icon.ContextMenu = new ContextMenu();
			_icon.Icon.ContextMenu.MenuItems.Add(0,	new MenuItem("Configure",new System.EventHandler(Configure_Click)));
			_icon.Icon.ContextMenu.MenuItems.Add(1,	new MenuItem("Exit",new System.EventHandler(Exit_Click)));
			_icon.Icon.Visible = true;

		}
		public void				Refresh(){
			string status = Equity + " @ " + _symbol.Price.ToString("##0.00");
			if (_symbol.Volume>0)
				status += "\n"+_symbol.Volume.ToString("###,###,###,### Shares");
			_icon.Value		= _symbol.Price;
			_icon.Icon.Text	= status;
		}
		/// <summary>
		/// Update the icon's value and bubble help. Don't update more than 1x/second
		/// </summary>
		public void				QuoteUpdated(string name){
			DateTime now = DateTime.Now;
			if (now >= nextTime){
				Refresh();
				nextTime = now.AddMilliseconds(100);
			}
		}
	}
}
