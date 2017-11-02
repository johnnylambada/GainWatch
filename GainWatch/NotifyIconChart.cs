// This code was written by MP
// (c) by Maciej Pirog 2002

using System;
using System.Windows.Forms;
using System.Drawing;

namespace System.Windows.Forms
{
	/// <summary>
	/// The NotifyIconChart class will draw a chart in the system tray.
	/// </summary>
	public class NotifyIconChart : System.ComponentModel.Component
	{
		private static readonly log4net.ILog log=log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		// Properties:

		// Size of the icon: 16x16
		private const int size = 16;
		/// <summary>
		///  Stores a reference to the System.Windows.Forms.NotifyIcon object.
		/// </summary>
		public NotifyIcon NotifyIconObject
		{
			get{return ni;}
			set{ni = value; CreateIcon();}
		}
		private NotifyIcon ni;
		/// <summary>
		/// The backround color. Use Color.Transparent if you don't want NotifyIconChart to draw background.
		/// </summary>
		public Color BackgroundColor
		{
			get{return backgroundColor;}
			set{backgroundColor = value; CreateIcon();}
		}
		private Color backgroundColor = Color.Transparent;
		/// <summary>
		/// The chart elements' frame color. Use Color.Transparent if you don't want NotifyIconChart to draw frames.
		/// </summary>
		public Color FrameColor
		{
			get{return frameColor;}
			set{frameColor = value; CreateIcon();}
		}
		private Color frameColor = Color.Transparent;
		/// <summary>
		/// This color will be used to draw the first bar or the pie.
		/// </summary>
		public Color Color1
		{
			get{return color1;}
			set{color1 = value; CreateIcon();}
		}
		private Color color1 = Color.Red;
		/// <summary>
		/// This color will be used to draw the second bar.
		/// </summary>
		public Color Color2
		{
			get{return color2;}
			set{color2 = value; CreateIcon();}
		}
		private Color color2 = Color.Blue;
		
		/// <summary>
		/// Chart types
		/// </summary>
		public enum ChartTypeEnum
		{
			singleBar,
			twoBars,
			pie
		}
		/// <summary>
		/// The type of the chart.
		/// </summary>
		public ChartTypeEnum ChartType
		{
			get{return chartType;}
			set{chartType = value; CreateIcon();}
		}
		private ChartTypeEnum chartType = ChartTypeEnum.singleBar;
		/// <summary>
		/// The value of the first bar or the pie (0-100).
		/// </summary>
		public int Value1
		{
			get{return value1;}
			set{value1 = value; CreateIcon();}
		}
		/// <summary>
		/// The value of the second bar or the pie (0-100).
		/// </summary>
		public int Value2
		{
			get{return value2;}
			set{value2 = value; CreateIcon();}
		}
		private int value1 = 0;
		private int value2 = 0;
		
		// Constructor:

		/// <summary>
		/// Constructor. Don't forget to set Systems.Windows.Forms.NotifyIcon object (NotifyIconObject).
		/// </summary>
		public NotifyIconChart()
		{
			bitmap = new Bitmap(size,size);
			g = Graphics.FromImage(bitmap);
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			CreateIcon();
		}

		// System.Drawing objects that will be used for drawing:
		private Graphics g;
		private Bitmap bitmap;
		private Icon icon;

		private void CreateIcon()
		{
			// Draw different charts
			if(chartType == ChartTypeEnum.singleBar)
				DrawBars(value1);
			else if(chartType == ChartTypeEnum.twoBars)
				DrawBars(value1, value2);
			else if(chartType == ChartTypeEnum.pie)
				DrawPie(value1);
		}
		// Draw one bar
		private void DrawBars(int bar1)
		{
			// Draw background
			DrawBG(BGType.rect);
			// Set GDI+ objects
			Pen p = new Pen(frameColor, 1);
			SolidBrush b = new SolidBrush(color1);
			// Do some math
			int width = (size * bar1) / 100;
			// Draw bar
			g.FillRectangle(b, 3, size - width, 10, width-1);
			g.DrawRectangle(p, 3, size - width, 10, width-1);
			// Create and display icon
			ShowIcon();
		}
		// Draw two bars
		private void DrawBars(int bar1, int bar2)
		{
			// Draw background
			DrawBG(BGType.rect);
			// Set GDI+ objects
			Pen p = new Pen(frameColor, 1);
			SolidBrush b = new SolidBrush(color1);
			// Do some math for the first bar
			int width1 = (size * bar1) / 100;
			// Draw the first bar
			g.FillRectangle(b, 1, size - width1, 5, width1-1);
			g.DrawRectangle(p, 1, size - width1, 5, width1-1);
			// Set GDI+ brush for the second bar
			b = new SolidBrush(Color2);
			// Do some math for the second bar
			int width2 = (size * bar2) / 100;
			// Draw the second bar
			g.FillRectangle(b, 9, size - width2, 5, width2-1);
			g.DrawRectangle(p, 9, size - width2, 5, width2-1);
			// Create and display icon
			ShowIcon();
		}
		// Draw pie chart
		private void DrawPie(int pie1)
		{
			// Draw background
			DrawBG(BGType.pie);
			// Set GDI+ objects
			Pen p = new Pen(frameColor, 1);
			SolidBrush b = new SolidBrush(color1);
			// Draw pie
			g.FillPie(b,0,0,size-1,size-1,270,(int)(pie1 * 3.6));
			g.DrawPie(p,0,0,size-1,size-1,270,(int)(pie1 * 3.6));
			// Create and display icon
			ShowIcon();
		}

		private enum BGType /* background type */
		{
			rect,
			pie
		}
		private void DrawBG(BGType t)
		{
			// Clear Graphics object
			g.Clear(Color.Transparent);
			SolidBrush b = new SolidBrush(backgroundColor);
			if(t == BGType.rect)
				g.FillRectangle(b,0,0,size,size);
			else if(t == BGType.pie)
				g.FillEllipse(b,0,0,size,size);
		}
		private void ShowIcon()
		{
			// Convert bitmap to icon
			icon = Icon.FromHandle(bitmap.GetHicon());
			// if none NotifyIcon object was selected, do not draw icon.
			if(ni != null)
				ni.Icon = icon;
		}
	}
}
