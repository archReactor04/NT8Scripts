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
	public class NadarayaWatsonEnvelopeWithATRNonRepaint : Indicator
	{
		private int size;
		private Series<double> yhat1;
		private Series<double> yhat2;
		
		private DashStyleHelper dash0Style = DashStyleHelper.Solid;
        private PlotStyle plot0Style = PlotStyle.Line;
		
		private Brush upColor = Brushes.Lime;
        private Brush downColor = Brushes.Red;
		double y1 = 0.0;
		double y2 = 0.0;
		private ADX adx;
		private Series<double> trend;
		
		private Series<double> signal; // for buy = 1 and sell = -1
		private Series<double> midBandSignalChange;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "NadarayaWatsonEnvelopeWithATRNonRepaint";
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
				H					= 8;
				R					= 8;
				X_0					= 25;
				ShowMiddle					= true;
				SmoothColors					= false;
				Lag					= 2;
				ATR_Length					= 32;
				Multiplier					= 2.7;
				ADX_Filter					= true; // Adding RSI to filter out buy and sell signals;
				ADX_Period					= 14;
				ADX_Min						= 25;
				AddPlot(Brushes.AntiqueWhite, "MiddleBand");
				AddPlot(Brushes.Chartreuse, "UpperBand");
				AddPlot(Brushes.Crimson, "LowerBand");
				AddPlot(new Stroke(Brushes.Green, 8), PlotStyle.TriangleUp, "UpSignal");
				AddPlot(new Stroke(Brushes.Red, 8), PlotStyle.TriangleDown, "DnSignal");
				AddPlot(new Stroke(Brushes.Red, 3), PlotStyle.Dot, "MidBandSignal");
				
			}
			else if (State == State.Configure)
			{
				yhat1 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				yhat2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				signal = new Series<double>(this, MaximumBarsLookBack.Infinite);
				midBandSignalChange = new Series<double>(this, MaximumBarsLookBack.Infinite);
				adx = ADX(Input, ADX_Period);
				trend = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			
			if (CurrentBar < X_0) {
				LowerBand[0] = Input[0];
				UpperBand[0] = Input[0];
				MiddleBand[0] = Input[0];
				size = 0;
				yhat1[0] = 0.0;
				yhat2[0] = 0.0;
				return;
			}
			
			size = Close.Count;
			
			yhat1[0] = kernel_regression(1, H);

			yhat2[0] = kernel_regression(1, H-Lag);
			
			bool wasBearish = yhat1[2] > yhat1[1];
			bool wasBullish = yhat1[2] < yhat1[1];
			bool isBearish = yhat1[1] > yhat1[0];
			bool isBullish = yhat1[1] < yhat1[0];
			bool isBearishChange = isBearish && wasBullish;
			bool isBullishChange = isBullish && wasBearish;
			
			bool isBullishCross = CrossAbove(yhat2, yhat1, 1);
			bool isBearishCross = CrossBelow(yhat2, yhat1, 1);
			bool isBullishSmooth = yhat2[0] > yhat1[0];
			bool isBearishSmooth = yhat2[0] < yhat1[0];
			
			Brush colorByCross = isBullishSmooth ? upColor : downColor;
			Brush colorByRate = isBullish ? upColor : downColor;
			
			
			Brush plotColor = SmoothColors ? colorByCross : colorByRate;
			
			MiddleBand[0] = yhat1[0];
			
			PlotBrushes[0][0] = plotColor;
			PlotBrushes[5][0] = plotColor;
			if (SmoothColors) {
				if (isBearishCross) {
					MidBandSignal[0] = High[0];
					midBandSignalChange[0] = -1;
				} else if (isBullishCross) {
					MidBandSignal[0] = Low[0];
					midBandSignalChange[0] = 1;
				}
			} else {
				if (isBearishChange) {
					midBandSignalChange[0] = -1;
					MidBandSignal[0] = High[0];
				} else if (isBullishChange) {
					MidBandSignal[0] = Low[0];
					midBandSignalChange[0] = 1;
				}
			}
			
			if (isBullish) trend[0] = 1;
			if (isBearish) trend[0] = -1;
			
			UpperBand[0] = yhat1[0] + Multiplier*ATR(ATR_Length)[0];
			LowerBand[0] = yhat1[0] - Multiplier*ATR(ATR_Length)[0];
			signal[0] = 0;
			//if (Low[1] < LowerBand[1] && Close[1] < Open[1] && Close[0] > Open[0]) {
			if (Close[0] > Open[0] && Low[0] < LowerBand[0] && High[0] > LowerBand[0] && signal[1] != 1) {
				if (ADX_Filter) {
					if (adx[0] > ADX_Min) {
						UpSignal[0] = Low[0] - 2 * TickSize;
						signal[0] = 1;
					}
				} else {
					UpSignal[0] = Low[0] - 2 * TickSize;
					signal[0] = 1;
				}
			} 
			//else if (High[1] > UpperBand[1] && Close[1] > Open[1] && Close[0] < Open[0]) {
			else if (Close[0] < Open[0] && High[0] > UpperBand[0] && Low[0] < UpperBand[0] && signal[1] != -1) {
				if (ADX_Filter) {
					if (adx[0] > ADX_Min) {
						DnSignal[0] = High[0] + 2 * TickSize;
						signal[0] = -1;
					}
				} else {
					DnSignal[0] = High[0] + 2 * TickSize;
					signal[0] = -1;
				}
			}
		}
		
		private double kernel_regression(int _size, double _h) {
			double _currentWeight = 0.0;
			double _cumulativeWeight = 0.0;
			for (int i = 0 ; i < (_size + X_0); i++) {
				double y = Close[i];
				double w = Math.Pow(1 + (Math.Pow(i, 2) / ((Math.Pow(_h,2)*2*R))), -R);
				_currentWeight += y*w;
				_cumulativeWeight += w;
			}
			if (_cumulativeWeight != 0.0)
		    {
		        return _currentWeight / _cumulativeWeight;
		    }
		    else
		    {
		        return 0.0; // Handle division by zero or very small cumulativeWeight
		    }
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(3, double.MaxValue)]
		[Display(Name="H", Description="Lookback Window", Order=1, GroupName="Parameters")]
		public double H
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="R", Description="Relative Weighting", Order=2, GroupName="Parameters")]
		public double R
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="X_0", Description="Start Regression at Bar", Order=3, GroupName="Parameters")]
		public int X_0
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowMiddle", Description="Show middle band", Order=4, GroupName="Parameters")]
		public bool ShowMiddle
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SmoothColors", Description="Smooth Colors", Order=5, GroupName="Parameters")]
		public bool SmoothColors
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Lag", Order=6, GroupName="Parameters")]
		public int Lag
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ATR_Length", Description="ATR Length", Order=7, GroupName="Parameters")]
		public int ATR_Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Multiplier", Description="Multiplier", Order=8, GroupName="Parameters")]
		public double Multiplier
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ADX Filtering", Description="Enable/Disable Buy Sell Signal Filtering with ADX", Order=9, GroupName="Buy Sell Signal Filtering with ADX")]
		public bool ADX_Filter
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADX Period", Description="ADX Period", Order=10, GroupName="Buy Sell Signal Filtering with ADX")]
		public int ADX_Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADX Minimum", Description="ADX Minimum value to filter", Order=10, GroupName="Buy Sell Signal Filtering with RSI")]
		public int ADX_Min
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MiddleBand
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperBand
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerBand
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpSignal
		{
			get { return Values[3]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DnSignal
		{
			get { return Values[4]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MidBandSignal
		{
			get { return Values[5]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Trend
		{
			get { return trend; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal
		{
			get { return signal; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MidBandSignalChange
		{
			get { return midBandSignalChange; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private NadarayaWatsonEnvelopeWithATRNonRepaint[] cacheNadarayaWatsonEnvelopeWithATRNonRepaint;
		public NadarayaWatsonEnvelopeWithATRNonRepaint NadarayaWatsonEnvelopeWithATRNonRepaint(double h, double r, int x_0, bool showMiddle, bool smoothColors, int lag, int aTR_Length, double multiplier, bool aDX_Filter, int aDX_Period, int aDX_Min)
		{
			return NadarayaWatsonEnvelopeWithATRNonRepaint(Input, h, r, x_0, showMiddle, smoothColors, lag, aTR_Length, multiplier, aDX_Filter, aDX_Period, aDX_Min);
		}

		public NadarayaWatsonEnvelopeWithATRNonRepaint NadarayaWatsonEnvelopeWithATRNonRepaint(ISeries<double> input, double h, double r, int x_0, bool showMiddle, bool smoothColors, int lag, int aTR_Length, double multiplier, bool aDX_Filter, int aDX_Period, int aDX_Min)
		{
			if (cacheNadarayaWatsonEnvelopeWithATRNonRepaint != null)
				for (int idx = 0; idx < cacheNadarayaWatsonEnvelopeWithATRNonRepaint.Length; idx++)
					if (cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx] != null && cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx].H == h && cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx].R == r && cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx].X_0 == x_0 && cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx].ShowMiddle == showMiddle && cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx].SmoothColors == smoothColors && cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx].Lag == lag && cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx].ATR_Length == aTR_Length && cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx].Multiplier == multiplier && cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx].ADX_Filter == aDX_Filter && cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx].ADX_Period == aDX_Period && cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx].ADX_Min == aDX_Min && cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx].EqualsInput(input))
						return cacheNadarayaWatsonEnvelopeWithATRNonRepaint[idx];
			return CacheIndicator<NadarayaWatsonEnvelopeWithATRNonRepaint>(new NadarayaWatsonEnvelopeWithATRNonRepaint(){ H = h, R = r, X_0 = x_0, ShowMiddle = showMiddle, SmoothColors = smoothColors, Lag = lag, ATR_Length = aTR_Length, Multiplier = multiplier, ADX_Filter = aDX_Filter, ADX_Period = aDX_Period, ADX_Min = aDX_Min }, input, ref cacheNadarayaWatsonEnvelopeWithATRNonRepaint);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.NadarayaWatsonEnvelopeWithATRNonRepaint NadarayaWatsonEnvelopeWithATRNonRepaint(double h, double r, int x_0, bool showMiddle, bool smoothColors, int lag, int aTR_Length, double multiplier, bool aDX_Filter, int aDX_Period, int aDX_Min)
		{
			return indicator.NadarayaWatsonEnvelopeWithATRNonRepaint(Input, h, r, x_0, showMiddle, smoothColors, lag, aTR_Length, multiplier, aDX_Filter, aDX_Period, aDX_Min);
		}

		public Indicators.NadarayaWatsonEnvelopeWithATRNonRepaint NadarayaWatsonEnvelopeWithATRNonRepaint(ISeries<double> input , double h, double r, int x_0, bool showMiddle, bool smoothColors, int lag, int aTR_Length, double multiplier, bool aDX_Filter, int aDX_Period, int aDX_Min)
		{
			return indicator.NadarayaWatsonEnvelopeWithATRNonRepaint(input, h, r, x_0, showMiddle, smoothColors, lag, aTR_Length, multiplier, aDX_Filter, aDX_Period, aDX_Min);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.NadarayaWatsonEnvelopeWithATRNonRepaint NadarayaWatsonEnvelopeWithATRNonRepaint(double h, double r, int x_0, bool showMiddle, bool smoothColors, int lag, int aTR_Length, double multiplier, bool aDX_Filter, int aDX_Period, int aDX_Min)
		{
			return indicator.NadarayaWatsonEnvelopeWithATRNonRepaint(Input, h, r, x_0, showMiddle, smoothColors, lag, aTR_Length, multiplier, aDX_Filter, aDX_Period, aDX_Min);
		}

		public Indicators.NadarayaWatsonEnvelopeWithATRNonRepaint NadarayaWatsonEnvelopeWithATRNonRepaint(ISeries<double> input , double h, double r, int x_0, bool showMiddle, bool smoothColors, int lag, int aTR_Length, double multiplier, bool aDX_Filter, int aDX_Period, int aDX_Min)
		{
			return indicator.NadarayaWatsonEnvelopeWithATRNonRepaint(input, h, r, x_0, showMiddle, smoothColors, lag, aTR_Length, multiplier, aDX_Filter, aDX_Period, aDX_Min);
		}
	}
}

#endregion
