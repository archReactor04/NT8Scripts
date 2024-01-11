#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class RipsterEMAClouds : Indicator
	{
		private Series<double> ema1Trend;
		private Series<double> ema2Trend;
		private Series<double> ema3Trend;
		private Series<double> ema4Trend;
		private Series<double> ema5Trend;
		
		private Brush ema1Color;
		private Brush ema2Color;
		private Brush ema3Color;
		private Brush ema4Color;
		private Brush ema5Color;
		private Brush labelTextColor;
		private Brush borderColor;
		private int[] emaLengthShort;
		private int[] emaLengthLong;
		private double[] htf_ma_long;
		private double[] htf_ma_short;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "RipsterEMAClouds";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				ShortEMA1Length					= 8;
				LongEMA1Length					= 9;
				ShortEMA2Length					= 5;
				LongEMA2Length					= 12;
				ShortEMA3Length					= 34;
				LongEMA3Length					= 50;
				ShortEMA4Length					= 72;
				LongEMA4Length					= 89;
				ShortEMA5Length					= 180;
				LongEMA5Length					= 200;
				Offset					= 0;
				NoOfMACloud = 3;
				ema1Color	=Brushes.Gold;
				ema2Color	=Brushes.Gold;
				ema3Color	=Brushes.Gold;
				ema4Color	=Brushes.Gold;
				ema5Color	=Brushes.Gold;
				labelTextColor		= Brushes.White;
				borderColor			= Brushes.White;
				
				//for (int i = 1; i <= NoOfMACloud; i++) {}
					AddPlot(Brushes.Orange, "EMA1Short");
					AddPlot(Brushes.Orange, "EMA1Long");
		
				//if (NoOfMACloud <= 2) {
					AddPlot(Brushes.White, "EMA2Short");
					AddPlot(Brushes.White, "EMA2Long");
				//}
				//if (NoOfMACloud <= 3) {
					AddPlot(Brushes.Blue, "EMA3Short");
					AddPlot(Brushes.Blue, "EMA3Long");
			//	}
				
			//	if (NoOfMACloud <= 4) {
					AddPlot(Brushes.Red, "EMA4Short");
					AddPlot(Brushes.Red, "EMA4Long");
			//	}
				
			//	if (NoOfMACloud <= 5) {
					AddPlot(Brushes.Pink, "EMA5Short");
					AddPlot(Brushes.Pink, "EMA5Long");
			//	}
			
			}
			else if (State == State.Configure)
			{
				Print("In Configure");
				ema1Trend = new Series<double>(this);
				ema2Trend = new Series<double>(this);
				ema3Trend = new Series<double>(this);
				ema4Trend = new Series<double>(this);
				ema5Trend = new Series<double>(this);
				emaLengthLong = new int[5];
				emaLengthShort = new int[5];
				emaLengthLong[0] = LongEMA1Length;
				emaLengthShort[0] = ShortEMA1Length;
				emaLengthLong[1] = LongEMA2Length;
				emaLengthShort[1] = ShortEMA2Length;
				emaLengthLong[2] = LongEMA3Length;
				emaLengthShort[2] = ShortEMA3Length;
				emaLengthLong[3] = LongEMA4Length;
				emaLengthShort[3] = ShortEMA4Length;
				emaLengthLong[4] = LongEMA5Length;
				emaLengthShort[4] = ShortEMA5Length;
				htf_ma_long = new double[5];
				htf_ma_short = new double[5];
			} else if (State == State.DataLoaded) {
				Print("In Dataloaded");
				for (int i = 1; i<=NoOfMACloud; i++) {
					htf_ma_long[i-1] = f_ma(emaLengthLong[i-1]);
					htf_ma_short[i-1] = f_ma(emaLengthShort[i-1]);
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1) {
				return;
			}
			
			Print("On Bar Update");
			
			for (int i = 1; i<=NoOfMACloud; i++) {
				htf_ma_long[i-1] = f_ma(emaLengthLong[i-1]);
				htf_ma_short[i-1] = f_ma(emaLengthShort[i-1]);
				
				Brush cloudcolour1 = null;
			
				if (i == 1) {
					EMA1Short[0] = htf_ma_short[0];
					EMA1Long[0] = htf_ma_long[0];
					if (EMA1Short[0] >= EMA1Long[0]) {
						cloudcolour1 = Brushes.DarkGreen;
						ema1Trend[0] = 1;
					} else {
						cloudcolour1 = Brushes.DarkRed;
						ema1Trend[0] = -1;
					}
					
					Brush mashortcolor1 = EMA1Short[0] >= EMA1Short[1] ? Brushes.Olive : Brushes.Maroon;
					Brush malongcolor1 = EMA1Long[0] >= EMA1Long[1] ? Brushes.Green : Brushes.Red;
					
					PlotBrushes[0][0] = mashortcolor1;
					PlotBrushes[1][0] = malongcolor1;
					Draw.Region(this, "cloud1"+CurrentBar.ToString(),Time[1], Time[0], EMA1Short, EMA1Long, Brushes.Transparent, cloudcolour1, 45);
					drawText("ema1", ema1Trend[0]);			
						
				}
				
				Brush cloudcolour2 = null;
				if (i == 2) {
					EMA2Short[0] = htf_ma_short[1];
					EMA2Long[0] = htf_ma_long[1];
					if (EMA2Short[0] >= EMA2Long[0]) {
						cloudcolour2 = Brushes.Chartreuse;
						ema2Trend[0] = 1;
					} else {
						cloudcolour2 = Brushes.Salmon;
						ema2Trend[0] = -1;
					}
					Brush mashortcolor2 = EMA2Short[0] >= EMA2Short[1] ? Brushes.Olive : Brushes.Maroon;
					Brush malongcolor2 = EMA2Long[0] >= EMA2Long[1] ? Brushes.Green : Brushes.Red;
					
					PlotBrushes[2][0] = mashortcolor2;
					PlotBrushes[3][0] = malongcolor2;
					Draw.Region(this, "cloud2"+CurrentBar.ToString(),Time[1], Time[0], EMA2Short, EMA2Long, Brushes.Transparent, cloudcolour2, 65);
					drawText( "\n" + "ema2", ema2Trend[0]);	
					
				}
				
				Brush cloudcolour3 = null;
				if (i == 3) {
					EMA3Short[0] = htf_ma_short[2];
					EMA3Long[0] = htf_ma_long[2];
					if (EMA3Short[0] >= EMA3Long[0]) {
						cloudcolour3 = Brushes.Blue;
						ema3Trend[0] = 1;
					} else {
						cloudcolour3 = Brushes.Yellow;
						ema3Trend[0] = -1;
					}
					Brush mashortcolor3 = EMA3Short[0] >= EMA3Short[1] ? Brushes.Olive : Brushes.Maroon;
					Brush malongcolor3 = EMA3Long[0] >= EMA3Long[1] ? Brushes.Green : Brushes.Red;
					
					PlotBrushes[4][0] = mashortcolor3;
					PlotBrushes[5][0] = malongcolor3;
					Draw.Region(this, "cloud3"+CurrentBar.ToString(), Time[1], Time[0], EMA3Short, EMA3Long, Brushes.Transparent, cloudcolour3, 70);
					drawText( "\n\n" + "ema3", ema3Trend[0]);	
					
				}
				
				Brush cloudcolour4 = null;
				if (i == 4) {
					EMA4Short[0] = htf_ma_short[3];
					EMA4Long[0] = htf_ma_long[3];
					if (EMA4Short[0] >= EMA4Long[0]) {
						cloudcolour4 = Brushes.White;
						ema4Trend[0] = 1;
					} else {
						cloudcolour4 = Brushes.Khaki;
						ema4Trend[0] = -1;
					}
					Brush mashortcolor4 = EMA4Short[0] >= EMA4Short[1] ? Brushes.Olive : Brushes.Maroon;
					Brush malongcolor4 = EMA4Long[0] >= EMA4Long[1] ? Brushes.Green : Brushes.Red;
					
					PlotBrushes[6][0] = mashortcolor4;
					PlotBrushes[7][0] = malongcolor4;
					Draw.Region(this, "cloud4"+CurrentBar.ToString(), Time[1], Time[0], EMA4Short, EMA4Long, Brushes.Transparent, cloudcolour4, 65);
					drawText( "\n\n\n"+"ema4", ema4Trend[0]);	
				
				}
				
				Brush cloudcolour5 = null;
				if (i == 5) {
					EMA5Short[0] = htf_ma_short[4]; 
					EMA5Long [0] = htf_ma_long[4];
					if (EMA5Short[0] >= EMA5Long[0]) {
						cloudcolour5 = Brushes.Cyan;
						ema5Trend[0] = 1;
					} else {
						cloudcolour5 = Brushes.Teal;
						ema5Trend[0] = -1;
					}
					Brush mashortcolor5 = EMA5Short[0] >= EMA5Short[1] ? Brushes.Olive : Brushes.Maroon;
					Brush malongcolor5 = EMA5Long[0] >= EMA5Long[1] ? Brushes.Green : Brushes.Red;
					PlotBrushes[8][0] = mashortcolor5;
					PlotBrushes[9][0] = malongcolor5;
					Draw.Region(this, "cloud5"+CurrentBar.ToString(), Time[1], Time[0], EMA5Short, EMA5Long, Brushes.Transparent, cloudcolour5, 65);
					drawText("\n\n\n\n"+ "ema5", ema5Trend[0]);
				}
			}
		/*	double htf_ma1 = f_ma(ShortEMA1Length);
			double htf_ma2 = f_ma(LongEMA1Length);
			double htf_ma3 = f_ma(ShortEMA2Length);
			double htf_ma4 = f_ma(LongEMA2Length);
			double htf_ma5 = f_ma(ShortEMA3Length);
			double htf_ma6 = f_ma(LongEMA3Length);
			double htf_ma7 = f_ma(ShortEMA4Length);
			double htf_ma8 = f_ma(LongEMA4Length);
			double htf_ma9 = f_ma(ShortEMA5Length);
			double htf_ma10 = f_ma(LongEMA5Length);*/

			//Brush mashortcolor1 = EMA1Short[0] >= EMA1Short[1] ? Brushes.Olive : Brushes.Maroon;
			//Brush mashortcolor2 = EMA2Short[0] >= EMA2Short[1] ? Brushes.Olive : Brushes.Maroon;
			//Brush mashortcolor3 = EMA3Short[0] >= EMA3Short[1] ? Brushes.Olive : Brushes.Maroon;
			//Brush mashortcolor4 = EMA4Short[0] >= EMA4Short[1] ? Brushes.Olive : Brushes.Maroon;
			//Brush mashortcolor5 = EMA5Short[0] >= EMA5Short[1] ? Brushes.Olive : Brushes.Maroon;
			
			
			//Brush malongcolor1 = EMA1Long[0] >= EMA1Long[1] ? Brushes.Green : Brushes.Red;
			//Brush malongcolor2 = EMA2Long[0] >= EMA2Long[1] ? Brushes.Green : Brushes.Red;
			//Brush malongcolor3 = EMA3Long[0] >= EMA3Long[1] ? Brushes.Green : Brushes.Red;
			//Brush malongcolor4 = EMA4Long[0] >= EMA4Long[1] ? Brushes.Green : Brushes.Red;
			//Brush malongcolor5 = EMA5Long[0] >= EMA5Long[1] ? Brushes.Green : Brushes.Red;
			
			/*if (ShowCloud1 == false) {
				PlotBrushes[0][0] = Brushes.Transparent;
				PlotBrushes[1][0] = Brushes.Transparent;
			} else {
				PlotBrushes[0][0] = mashortcolor1;
				PlotBrushes[1][0] = malongcolor1;
				Draw.Region(this, "cloud1"+CurrentBar.ToString(),Time[1], Time[0], EMA1Short, EMA1Long, Brushes.Transparent, cloudcolour1, 45);
				drawText("ema1", ema1Trend[0]);			
			}*/
			
			/*if (ShowCloud2 == false) {
				PlotBrushes[2][0] = Brushes.Transparent;
				PlotBrushes[3][0] = Brushes.Transparent;
			} else {
				PlotBrushes[2][0] = mashortcolor2;
				PlotBrushes[3][0] = malongcolor2;
				Draw.Region(this, "cloud2"+CurrentBar.ToString(),Time[1], Time[0], EMA2Short, EMA2Long, Brushes.Transparent, cloudcolour2, 65);
				drawText( "\n" + "ema2", ema2Trend[0]);	
			}*/
			
		/*	if (ShowCloud3 == false) {
				PlotBrushes[4][0] = Brushes.Transparent;
				PlotBrushes[5][0] = Brushes.Transparent;
			} else {
				PlotBrushes[4][0] = mashortcolor3;
				PlotBrushes[5][0] = malongcolor3;
				Draw.Region(this, "cloud3"+CurrentBar.ToString(), Time[1], Time[0], EMA3Short, EMA3Long, Brushes.Transparent, cloudcolour3, 70);
				drawText( "\n\n" + "ema3", ema3Trend[0]);	
			}*/
			
			/*if (ShowCloud4 == false) {
				PlotBrushes[6][0] = Brushes.Transparent;
				PlotBrushes[7][0] = Brushes.Transparent;
			} else {
				PlotBrushes[6][0] = mashortcolor4;
				PlotBrushes[7][0] = malongcolor4;
				Draw.Region(this, "cloud4"+CurrentBar.ToString(), Time[1], Time[0], EMA4Short, EMA4Long, Brushes.Transparent, cloudcolour4, 65);
				drawText( "\n\n\n"+"ema4", ema4Trend[0]);	
			}*/
			
			/*if (ShowCloud5 == false) {
				PlotBrushes[8][0] = Brushes.Transparent;
				PlotBrushes[9][0] = Brushes.Transparent;
			} else {
				PlotBrushes[8][0] = mashortcolor5;
				PlotBrushes[9][0] = malongcolor5;
				Draw.Region(this, "cloud5"+CurrentBar.ToString(), Time[1], Time[0], EMA5Short, EMA5Long, Brushes.Transparent, cloudcolour5, 65);
				drawText("\n\n\n\n"+ "ema5", ema5Trend[0]);	
			}*/
		}
		
		private void drawText(string name, double trend) {
			SimpleFont font = new SimpleFont("Arial", 15) {Bold = true };
			Brush color;
			string arrow;
			if (trend == 1) {
				color = Brushes.Green;
				arrow = "▲";
			}
			else{
				color = Brushes.Red;
				arrow = "▼";
			}

			Draw.TextFixed (this, name, name + " " + arrow, TextPosition.TopRight,color, font, Brushes.Transparent, Brushes.Transparent,100);
		}
		
		/*protected override void OnRender(ChartControl chartControl, ChartScale chartScale) {
			SimpleFont font = new SimpleFont("Arial", 15) {Bold = true };
			if (ShowCloud1 == true) {
				if (ema1Trend[0] == 1) {
					ema1Color	= Brushes.Green;
				} else {
					ema1Color	= Brushes.Red;
				}
				DrawTextBox(RenderTarget, ChartPanel, "ema1",  font, labelTextColor.ToDxBrush(RenderTarget), ChartPanel.X, ChartPanel.Y+50, ema1Color.ToDxBrush(RenderTarget), borderColor.ToDxBrush(RenderTarget));
			}
			
			if (ShowCloud2 == true) {
				if (ema2Trend[0] == 1) {
					ema2Color	= Brushes.Green;
				} else {
					ema2Color	= Brushes.Red;
				}
				DrawTextBox(RenderTarget, ChartPanel, "ema2",  font, labelTextColor.ToDxBrush(RenderTarget), ChartPanel.X, ChartPanel.Y+75, ema2Color.ToDxBrush(RenderTarget), borderColor.ToDxBrush(RenderTarget));
			}
			
			if (ShowCloud3 == true) {
				if (ema3Trend[0] == 1) {
					ema3Color	= Brushes.Green;
				} else {
					ema3Color	= Brushes.Red;
				}
				DrawTextBox(RenderTarget, ChartPanel, "ema3",  font, labelTextColor.ToDxBrush(RenderTarget), ChartPanel.X, ChartPanel.Y+100, ema3Color.ToDxBrush(RenderTarget), borderColor.ToDxBrush(RenderTarget));
			}
			
			if (ShowCloud4 == true) {
				if (ema4Trend[0] == 1) {
					ema4Color	= Brushes.Green;
				} else {
					ema4Color	= Brushes.Red;
				}
				DrawTextBox(RenderTarget, ChartPanel, "ema4",  font, labelTextColor.ToDxBrush(RenderTarget), ChartPanel.X, ChartPanel.Y+125, ema4Color.ToDxBrush(RenderTarget), borderColor.ToDxBrush(RenderTarget));
			}
			
			if (ShowCloud5 == true) {
				if (ema5Trend[0] == 1) {
					ema5Color	= Brushes.Green;
				} else {
					ema5Color	= Brushes.Red;
				}
				DrawTextBox(RenderTarget, ChartPanel, "ema5",  font, labelTextColor.ToDxBrush(RenderTarget), ChartPanel.X, ChartPanel.Y+150, ema5Color.ToDxBrush(RenderTarget), borderColor.ToDxBrush(RenderTarget));
			}
		}
		
		public void DrawTextBox(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, string text, SimpleFont font, SharpDX.Direct2D1.Brush brush, double pointX, double pointY, SharpDX.Direct2D1.Brush areaBrush, SharpDX.Direct2D1.Brush borderBrush)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout =
			new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
				text, textFormat, 1000,
				textFormat.FontSize);
			SharpDX.Vector2 TextPlotPoint = new System.Windows.Point(pointX, pointY).ToVector2();
			
			float newW = textLayout.Metrics.Width; 
            float newH = textLayout.Metrics.Height;
			
            SharpDX.RectangleF PLBoundRect = new SharpDX.RectangleF((float)pointX - 2, (float)pointY - 2, newW + 4, newH + 4);
            renderTarget.FillRectangle(PLBoundRect, areaBrush);
			renderTarget.DrawRectangle(PLBoundRect, borderBrush);
			
			renderTarget.DrawTextLayout(TextPlotPoint, textLayout, brush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			textLayout.Dispose();
			textFormat.Dispose();
		}*/
		
		private double f_ma(int malength) {
			double result = 0;
			return EMA(Close, malength)[0];
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ShortEMA1Length", Description="Short EMA1 Length", Order=1, GroupName="Parameters")]
		public int ShortEMA1Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LongEMA1Length", Description="Long EMA1 Length", Order=2, GroupName="Parameters")]
		public int LongEMA1Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ShortEMA2Length", Description="Short EMA2 Length", Order=3, GroupName="Parameters")]
		public int ShortEMA2Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LongEMA2Length", Description="Long EMA2 Length", Order=4, GroupName="Parameters")]
		public int LongEMA2Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ShortEMA3Length", Description="Short EMA3 Length", Order=5, GroupName="Parameters")]
		public int ShortEMA3Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LongEMA3Length", Description="Long EMA3 Length", Order=6, GroupName="Parameters")]
		public int LongEMA3Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ShortEMA4Length", Description="Short EMA4 Length", Order=7, GroupName="Parameters")]
		public int ShortEMA4Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LongEMA4Length", Description="Long EMA4 Length", Order=8, GroupName="Parameters")]
		public int LongEMA4Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ShortEMA5Length", Description="Short EMA5 Length", Order=9, GroupName="Parameters")]
		public int ShortEMA5Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LongEMA5Length", Description="Long EMA5 Length", Order=10, GroupName="Parameters")]
		public int LongEMA5Length
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Offset", Description="Offset", Order=11, GroupName="Parameters")]
		public int Offset
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, 5)]
		[Display(Name="NoOfMACloud", Order=13, GroupName="Parameters")]
		public int NoOfMACloud
		{ get; set; }

		/*[NinjaScriptProperty]
		[Display(Name="ShowCloud1", Order=14, GroupName="Parameters")]
		public bool ShowCloud1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowCloud2", Order=15, GroupName="Parameters")]
		public bool ShowCloud2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowCloud3", Order=16, GroupName="Parameters")]
		public bool ShowCloud3
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowCloud4", Order=17, GroupName="Parameters")]
		public bool ShowCloud4
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowCloud5", Order=18, GroupName="Parameters")]
		public bool ShowCloud5
		{ get; set; }*/
		
		// the WPF exposed to the UI which the user defines
		[XmlIgnore]
		public System.Windows.Media.Brush LabelTextBrush { get; set; }
		 
		[Browsable(false)]
		
		public string LabelTextBrushSerialize
		{
		  get { return Serialize.BrushToString(LabelTextBrush); }
		  set { LabelTextBrush = Serialize.StringToBrush(value); }
		}
		
		// the WPF exposed to the UI which the user defines
		[XmlIgnore]
		public System.Windows.Media.Brush AreaBrush { get; set; }
		 
		[Browsable(false)]
		
		public string AreaBrushSerialize
		{
		  get { return Serialize.BrushToString(AreaBrush); }
		  set { AreaBrush = Serialize.StringToBrush(value); }
		}
		
		// the WPF exposed to the UI which the user defines
		[XmlIgnore]
		public System.Windows.Media.Brush BorderBrush { get; set; }
		 
		[Browsable(false)]
		
		public string BorderBrushSerialize
		{
		  get { return Serialize.BrushToString(BorderBrush); }
		  set { BorderBrush = Serialize.StringToBrush(value); }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA1Short
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA1Long
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA2Short
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA2Long
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA3Short
		{
			get { return Values[4]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA3Long
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA4Short
		{
			get { return Values[6]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA4Long
		{
			get { return Values[7]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA5Short
		{
			get { return Values[8]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA5Long
		{
			get { return Values[9]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA1Trend
		{
			get { return ema1Trend; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA2Trend
		{
			get { return ema2Trend; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA3Trend
		{
			get { return ema3Trend; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA4Trend
		{
			get { return ema4Trend; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA5Trend
		{
			get { return ema5Trend; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RipsterEMAClouds[] cacheRipsterEMAClouds;
		public RipsterEMAClouds RipsterEMAClouds(int shortEMA1Length, int longEMA1Length, int shortEMA2Length, int longEMA2Length, int shortEMA3Length, int longEMA3Length, int shortEMA4Length, int longEMA4Length, int shortEMA5Length, int longEMA5Length, int offset, int noOfMACloud)
		{
			return RipsterEMAClouds(Input, shortEMA1Length, longEMA1Length, shortEMA2Length, longEMA2Length, shortEMA3Length, longEMA3Length, shortEMA4Length, longEMA4Length, shortEMA5Length, longEMA5Length, offset, noOfMACloud);
		}

		public RipsterEMAClouds RipsterEMAClouds(ISeries<double> input, int shortEMA1Length, int longEMA1Length, int shortEMA2Length, int longEMA2Length, int shortEMA3Length, int longEMA3Length, int shortEMA4Length, int longEMA4Length, int shortEMA5Length, int longEMA5Length, int offset, int noOfMACloud)
		{
			if (cacheRipsterEMAClouds != null)
				for (int idx = 0; idx < cacheRipsterEMAClouds.Length; idx++)
					if (cacheRipsterEMAClouds[idx] != null && cacheRipsterEMAClouds[idx].ShortEMA1Length == shortEMA1Length && cacheRipsterEMAClouds[idx].LongEMA1Length == longEMA1Length && cacheRipsterEMAClouds[idx].ShortEMA2Length == shortEMA2Length && cacheRipsterEMAClouds[idx].LongEMA2Length == longEMA2Length && cacheRipsterEMAClouds[idx].ShortEMA3Length == shortEMA3Length && cacheRipsterEMAClouds[idx].LongEMA3Length == longEMA3Length && cacheRipsterEMAClouds[idx].ShortEMA4Length == shortEMA4Length && cacheRipsterEMAClouds[idx].LongEMA4Length == longEMA4Length && cacheRipsterEMAClouds[idx].ShortEMA5Length == shortEMA5Length && cacheRipsterEMAClouds[idx].LongEMA5Length == longEMA5Length && cacheRipsterEMAClouds[idx].Offset == offset && cacheRipsterEMAClouds[idx].NoOfMACloud == noOfMACloud && cacheRipsterEMAClouds[idx].EqualsInput(input))
						return cacheRipsterEMAClouds[idx];
			return CacheIndicator<RipsterEMAClouds>(new RipsterEMAClouds(){ ShortEMA1Length = shortEMA1Length, LongEMA1Length = longEMA1Length, ShortEMA2Length = shortEMA2Length, LongEMA2Length = longEMA2Length, ShortEMA3Length = shortEMA3Length, LongEMA3Length = longEMA3Length, ShortEMA4Length = shortEMA4Length, LongEMA4Length = longEMA4Length, ShortEMA5Length = shortEMA5Length, LongEMA5Length = longEMA5Length, Offset = offset, NoOfMACloud = noOfMACloud }, input, ref cacheRipsterEMAClouds);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RipsterEMAClouds RipsterEMAClouds(int shortEMA1Length, int longEMA1Length, int shortEMA2Length, int longEMA2Length, int shortEMA3Length, int longEMA3Length, int shortEMA4Length, int longEMA4Length, int shortEMA5Length, int longEMA5Length, int offset, int noOfMACloud)
		{
			return indicator.RipsterEMAClouds(Input, shortEMA1Length, longEMA1Length, shortEMA2Length, longEMA2Length, shortEMA3Length, longEMA3Length, shortEMA4Length, longEMA4Length, shortEMA5Length, longEMA5Length, offset, noOfMACloud);
		}

		public Indicators.RipsterEMAClouds RipsterEMAClouds(ISeries<double> input , int shortEMA1Length, int longEMA1Length, int shortEMA2Length, int longEMA2Length, int shortEMA3Length, int longEMA3Length, int shortEMA4Length, int longEMA4Length, int shortEMA5Length, int longEMA5Length, int offset, int noOfMACloud)
		{
			return indicator.RipsterEMAClouds(input, shortEMA1Length, longEMA1Length, shortEMA2Length, longEMA2Length, shortEMA3Length, longEMA3Length, shortEMA4Length, longEMA4Length, shortEMA5Length, longEMA5Length, offset, noOfMACloud);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RipsterEMAClouds RipsterEMAClouds(int shortEMA1Length, int longEMA1Length, int shortEMA2Length, int longEMA2Length, int shortEMA3Length, int longEMA3Length, int shortEMA4Length, int longEMA4Length, int shortEMA5Length, int longEMA5Length, int offset, int noOfMACloud)
		{
			return indicator.RipsterEMAClouds(Input, shortEMA1Length, longEMA1Length, shortEMA2Length, longEMA2Length, shortEMA3Length, longEMA3Length, shortEMA4Length, longEMA4Length, shortEMA5Length, longEMA5Length, offset, noOfMACloud);
		}

		public Indicators.RipsterEMAClouds RipsterEMAClouds(ISeries<double> input , int shortEMA1Length, int longEMA1Length, int shortEMA2Length, int longEMA2Length, int shortEMA3Length, int longEMA3Length, int shortEMA4Length, int longEMA4Length, int shortEMA5Length, int longEMA5Length, int offset, int noOfMACloud)
		{
			return indicator.RipsterEMAClouds(input, shortEMA1Length, longEMA1Length, shortEMA2Length, longEMA2Length, shortEMA3Length, longEMA3Length, shortEMA4Length, longEMA4Length, shortEMA5Length, longEMA5Length, offset, noOfMACloud);
		}
	}
}

#endregion
