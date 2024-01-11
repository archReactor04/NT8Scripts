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
	public class NadarayaWatsonEnvelopeWithKernelRegNR : Indicator
	{
		private int size;
		private Series<double> yhat_close;
		private Series<double> yhat_high;
		private Series<double> yhat_low;
		private Series<double> yhat;
		
		private Series<double> atr;
		
		private DashStyleHelper dash0Style = DashStyleHelper.Solid;
        private PlotStyle plot0Style = PlotStyle.Line;
		
		private Brush upColor = Brushes.Lime;
        private Brush downColor = Brushes.Red;
		
		private Series<int> signalChange;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "NadarayaWatsonEnvelopeWithKernelRegNR";
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
				Alpha					= 8;
				X_0					= 25;
				ATR_Length 			= 60;
				Near_Factor			= 1.5;
				Far_Factor			= 8.0;
				AddPlot(new Stroke(Brushes.PaleGreen, 2), PlotStyle.Line, "UpperFar");
				AddPlot(new Stroke(Brushes.LawnGreen, 2), PlotStyle.Line, "UpperAvg");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "UpperNear");
				AddPlot(new Stroke(Brushes.IndianRed, 2), PlotStyle.Line, "LowerFar");
				AddPlot(new Stroke(Brushes.Crimson, 2), PlotStyle.Line, "LowerAvg");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Line, "LowerNear");
				AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Line, "NWEstimate");
			}
			else if (State == State.Configure)
			{
				yhat_close = new Series<double>(this, MaximumBarsLookBack.Infinite);
				yhat_high = new Series<double>(this, MaximumBarsLookBack.Infinite);
				yhat_low = new Series<double>(this, MaximumBarsLookBack.Infinite);
				yhat = new Series<double>(this, MaximumBarsLookBack.Infinite);
				atr = new Series<double>(this, MaximumBarsLookBack.Infinite);
				signalChange = new Series<int>(this, MaximumBarsLookBack.Infinite);
			}
		}

		protected override void OnBarUpdate()
		{
			
			if (CurrentBar < X_0) {
				NWEstimate[0] = Input[0];
				size = 0;
				yhat_close[0] = 0.0;
				yhat_high[0] = 0.0;
				yhat_low[0] = 0.0;
				yhat[0] = 0.0;
				signalChange[0] = 0;
				atr[0] = 0.0;
				return;
			}
			// Envelope Calculations
			yhat_close[0] = kernel_regression(Close, H, Alpha, X_0);
			yhat_high[0] = kernel_regression(High, H, Alpha, X_0);
			yhat_low[0] = kernel_regression(Low, H, Alpha, X_0);
			yhat[0] = yhat_close[0];
			double ktr = kernel_atr(ATR_Length, yhat_high, yhat_low, yhat_close);
			double[] bounds = getBounds(ktr, Near_Factor, Far_Factor, yhat_close);
			
			UpperNear[0] = bounds[0];
			UpperFar[0] = bounds[1];
			UpperAvg[0] = bounds[2];
			LowerNear[0] = bounds[3];
			LowerFar[0] = bounds[4];
			LowerAvg[0] = bounds[5];
			NWEstimate[0] = yhat_close[0];
			
			if (yhat[0] > yhat[1]) {
				PlotBrushes[6][0] = Brushes.Green;
			} else {
				PlotBrushes[6][0] = Brushes.Red;
			}
			
			Draw.Region(this, "UpNearAvg", 0, CurrentBar, UpperNear, UpperAvg, Brushes.DarkGreen,Brushes.Green, 60,0);
			Draw.Region(this, "UpAvgFar", 0, CurrentBar, UpperAvg, UpperFar, Brushes.LawnGreen,Brushes.PaleGreen, 60,0);
			Draw.Region(this, "LowNearAvg", 0, CurrentBar, LowerNear, LowerAvg, Brushes.DarkRed,Brushes.Red, 60,0);
			Draw.Region(this, "LowAvgFar", 0, CurrentBar, LowerAvg, LowerFar, Brushes.Crimson,Brushes.IndianRed, 60,0);


			
		}
		
		private double[] getBounds(double _atr, double _nearFactor, double _farFactor, Series<double> _yhat) {
			double _upper_far = _yhat[0] + _farFactor*_atr;
			double _upper_near = _yhat[0] + _nearFactor*_atr;
			double _lower_near = _yhat[0] - _nearFactor*_atr;
			double _lower_far = _yhat[0] - _farFactor*_atr;
			double _upper_avg = (_upper_far + _upper_near) / 2;
			double _lower_avg = (_lower_far + _lower_near) / 2;
			return new double[] {_upper_near, _upper_far, _upper_avg, _lower_near, _lower_far, _lower_avg};
		
		}
		
		private double kernel_atr(int length, ISeries<double> _high, ISeries<double> _low, ISeries<double> _close) {
			double trueRange = 0.0;
			double smoothingFactor = 2.0 / (length + 1);
			if (_high[1] == 0.0) {
				trueRange = _high[0]-_low[0];
			} else {
				trueRange = Math.Max(Math.Max(_high[0] - _low[0], Math.Abs(_high[0] - _close[1])), Math.Abs(_low[0] - _close[1]));
			}
			
			//if (atr.Count < 1) {
			//	atr[0] = trueRange;
			//} else {
				atr[0] = (1 - smoothingFactor) * atr[1] + smoothingFactor * trueRange;
			//}
			return atr[0];
			
		}
		
		private double kernel_regression(ISeries<double> _src, double _h, double _r, int startAtBar) {
			double _currentWeight = 0.0;
			double _cumulativeWeight = 0.0;
			int _size = 1;
			for (int i = 0 ; i < (_size + startAtBar); i++) {
				Print("size "+_size);
				double y = _src[i];
				double w = Math.Pow(1 + (Math.Pow(i, 2) / ((Math.Pow(_h,2)*2*_r))), -_r);
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
		[Range(1, double.MaxValue)]
		[Display(Name="Lookback Window (H)", Description="Lookback Window", Order=1, GroupName="Parameters")]
		public double H
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Relative Weighting (Alpha)", Description="Relative Weighting", Order=2, GroupName="Parameters")]
		public double Alpha
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Start Regression at Bar (X_0)", Description="Start Regression at Bar", Order=3, GroupName="Parameters")]
		public int X_0
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ATR Length", Description="Length Of Atr", Order=4, GroupName="Parameters")]
		public int ATR_Length
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Near Factor", Description="Near ATR Factor", Order=5, GroupName="Parameters")]
		public double Near_Factor
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Far Factor", Description="Far ATR Factor", Order=6, GroupName="Parameters")]
		public double Far_Factor
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperFar
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperAvg
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperNear
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerFar
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerAvg
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerNear
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> NWEstimate
		{
			get { return Values[6]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private NadarayaWatsonEnvelopeWithKernelRegNR[] cacheNadarayaWatsonEnvelopeWithKernelRegNR;
		public NadarayaWatsonEnvelopeWithKernelRegNR NadarayaWatsonEnvelopeWithKernelRegNR(double h, double alpha, int x_0, int aTR_Length, double near_Factor, double far_Factor)
		{
			return NadarayaWatsonEnvelopeWithKernelRegNR(Input, h, alpha, x_0, aTR_Length, near_Factor, far_Factor);
		}

		public NadarayaWatsonEnvelopeWithKernelRegNR NadarayaWatsonEnvelopeWithKernelRegNR(ISeries<double> input, double h, double alpha, int x_0, int aTR_Length, double near_Factor, double far_Factor)
		{
			if (cacheNadarayaWatsonEnvelopeWithKernelRegNR != null)
				for (int idx = 0; idx < cacheNadarayaWatsonEnvelopeWithKernelRegNR.Length; idx++)
					if (cacheNadarayaWatsonEnvelopeWithKernelRegNR[idx] != null && cacheNadarayaWatsonEnvelopeWithKernelRegNR[idx].H == h && cacheNadarayaWatsonEnvelopeWithKernelRegNR[idx].Alpha == alpha && cacheNadarayaWatsonEnvelopeWithKernelRegNR[idx].X_0 == x_0 && cacheNadarayaWatsonEnvelopeWithKernelRegNR[idx].ATR_Length == aTR_Length && cacheNadarayaWatsonEnvelopeWithKernelRegNR[idx].Near_Factor == near_Factor && cacheNadarayaWatsonEnvelopeWithKernelRegNR[idx].Far_Factor == far_Factor && cacheNadarayaWatsonEnvelopeWithKernelRegNR[idx].EqualsInput(input))
						return cacheNadarayaWatsonEnvelopeWithKernelRegNR[idx];
			return CacheIndicator<NadarayaWatsonEnvelopeWithKernelRegNR>(new NadarayaWatsonEnvelopeWithKernelRegNR(){ H = h, Alpha = alpha, X_0 = x_0, ATR_Length = aTR_Length, Near_Factor = near_Factor, Far_Factor = far_Factor }, input, ref cacheNadarayaWatsonEnvelopeWithKernelRegNR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.NadarayaWatsonEnvelopeWithKernelRegNR NadarayaWatsonEnvelopeWithKernelRegNR(double h, double alpha, int x_0, int aTR_Length, double near_Factor, double far_Factor)
		{
			return indicator.NadarayaWatsonEnvelopeWithKernelRegNR(Input, h, alpha, x_0, aTR_Length, near_Factor, far_Factor);
		}

		public Indicators.NadarayaWatsonEnvelopeWithKernelRegNR NadarayaWatsonEnvelopeWithKernelRegNR(ISeries<double> input , double h, double alpha, int x_0, int aTR_Length, double near_Factor, double far_Factor)
		{
			return indicator.NadarayaWatsonEnvelopeWithKernelRegNR(input, h, alpha, x_0, aTR_Length, near_Factor, far_Factor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.NadarayaWatsonEnvelopeWithKernelRegNR NadarayaWatsonEnvelopeWithKernelRegNR(double h, double alpha, int x_0, int aTR_Length, double near_Factor, double far_Factor)
		{
			return indicator.NadarayaWatsonEnvelopeWithKernelRegNR(Input, h, alpha, x_0, aTR_Length, near_Factor, far_Factor);
		}

		public Indicators.NadarayaWatsonEnvelopeWithKernelRegNR NadarayaWatsonEnvelopeWithKernelRegNR(ISeries<double> input , double h, double alpha, int x_0, int aTR_Length, double near_Factor, double far_Factor)
		{
			return indicator.NadarayaWatsonEnvelopeWithKernelRegNR(input, h, alpha, x_0, aTR_Length, near_Factor, far_Factor);
		}
	}
}

#endregion
