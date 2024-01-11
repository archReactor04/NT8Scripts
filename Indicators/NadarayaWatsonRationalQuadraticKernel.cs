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
/**
This is a convertion of Trading View Nadaraya-Watson: Rational Quadration Kernel Regression from the user @jdehorty 
https://www.tradingview.com/script/AWNvbPRM-Nadaraya-Watson-Rational-Quadratic-Kernel-Non-Repainting/
**/
namespace NinjaTrader.NinjaScript.Indicators
{
	public class NadarayaWatsonRationalQuadraticKernel : Indicator
	{
		private int size;
		private Series<double> yhat1;
		private Series<double> yhat2;
		
		private DashStyleHelper dash0Style = DashStyleHelper.Solid;
        private PlotStyle plot0Style = PlotStyle.Line;
		
		private Brush upColor = Brushes.Lime;
        private Brush downColor = Brushes.Red;
		
		private Series<int> signalChange;
		private Series<double> trend;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "NadarayaWatsonRationalQuadraticKernel";
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
				Lag					= 2;
				SmoothColors					= false;
				AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Line, "NWKernelLine");
				AddPlot(new Stroke(Brushes.White, 8), PlotStyle.Dot, "Signal");
			}
			else if (State == State.Configure)
			{
				yhat1 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				yhat2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				signalChange = new Series<int>(this, MaximumBarsLookBack.Infinite);
				trend = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			
			if (CurrentBar < X_0) {
				NWKernelLine[0] = Input[0];
				size = 0;
				yhat1[0] = 0.0;
				yhat2[0] = 0.0;
				Signal[0] = 0.0;
				signalChange[0] = 0;
				return;
			}
			
			size = Close.Count;
			signalChange[0] = 0;
			
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
			
			NWKernelLine[0] = yhat1[0];
			
			PlotBrushes[0][0] = plotColor;
			
			if (SmoothColors) {
				if (isBearishCross) {
					Signal[0] = High[0];
					signalChange[0] = -1;
				} else if (isBullishCross) {
					Signal[0] = Low[0];
					signalChange[0] = 1;
				}
			} else {
				if (isBearishChange) {
					Signal[0] = High[0];
					signalChange[0] = -1;
				} else if (isBullishChange) {
					Signal[0] = Low[0];
					signalChange[0] = 1;
				}
			}
			
			if (isBullish) trend[0] = 1;
			if (isBearish) trend[0] = -1;
			
			PlotBrushes[1][0] = plotColor;
			
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
		[Range(1, int.MaxValue)]
		[Display(Name="Lag", Description="Lag", Order=4, GroupName="Parameters")]
		public int Lag
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SmoothColors", Description="Smooth Colors", Order=5, GroupName="Parameters")]
		public bool SmoothColors
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> NWKernelLine
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<int> SignalChange
		{
			get { return signalChange; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Trend
		{
			get { return trend; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private NadarayaWatsonRationalQuadraticKernel[] cacheNadarayaWatsonRationalQuadraticKernel;
		public NadarayaWatsonRationalQuadraticKernel NadarayaWatsonRationalQuadraticKernel(double h, double r, int x_0, int lag, bool smoothColors)
		{
			return NadarayaWatsonRationalQuadraticKernel(Input, h, r, x_0, lag, smoothColors);
		}

		public NadarayaWatsonRationalQuadraticKernel NadarayaWatsonRationalQuadraticKernel(ISeries<double> input, double h, double r, int x_0, int lag, bool smoothColors)
		{
			if (cacheNadarayaWatsonRationalQuadraticKernel != null)
				for (int idx = 0; idx < cacheNadarayaWatsonRationalQuadraticKernel.Length; idx++)
					if (cacheNadarayaWatsonRationalQuadraticKernel[idx] != null && cacheNadarayaWatsonRationalQuadraticKernel[idx].H == h && cacheNadarayaWatsonRationalQuadraticKernel[idx].R == r && cacheNadarayaWatsonRationalQuadraticKernel[idx].X_0 == x_0 && cacheNadarayaWatsonRationalQuadraticKernel[idx].Lag == lag && cacheNadarayaWatsonRationalQuadraticKernel[idx].SmoothColors == smoothColors && cacheNadarayaWatsonRationalQuadraticKernel[idx].EqualsInput(input))
						return cacheNadarayaWatsonRationalQuadraticKernel[idx];
			return CacheIndicator<NadarayaWatsonRationalQuadraticKernel>(new NadarayaWatsonRationalQuadraticKernel(){ H = h, R = r, X_0 = x_0, Lag = lag, SmoothColors = smoothColors }, input, ref cacheNadarayaWatsonRationalQuadraticKernel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.NadarayaWatsonRationalQuadraticKernel NadarayaWatsonRationalQuadraticKernel(double h, double r, int x_0, int lag, bool smoothColors)
		{
			return indicator.NadarayaWatsonRationalQuadraticKernel(Input, h, r, x_0, lag, smoothColors);
		}

		public Indicators.NadarayaWatsonRationalQuadraticKernel NadarayaWatsonRationalQuadraticKernel(ISeries<double> input , double h, double r, int x_0, int lag, bool smoothColors)
		{
			return indicator.NadarayaWatsonRationalQuadraticKernel(input, h, r, x_0, lag, smoothColors);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.NadarayaWatsonRationalQuadraticKernel NadarayaWatsonRationalQuadraticKernel(double h, double r, int x_0, int lag, bool smoothColors)
		{
			return indicator.NadarayaWatsonRationalQuadraticKernel(Input, h, r, x_0, lag, smoothColors);
		}

		public Indicators.NadarayaWatsonRationalQuadraticKernel NadarayaWatsonRationalQuadraticKernel(ISeries<double> input , double h, double r, int x_0, int lag, bool smoothColors)
		{
			return indicator.NadarayaWatsonRationalQuadraticKernel(input, h, r, x_0, lag, smoothColors);
		}
	}
}

#endregion
