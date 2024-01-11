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
This is an NT8 Conversion of Range Filter from guikroth https://www.tradingview.com/script/J8GzFGfD-Range-Filter-Buy-and-Sell-5min-guikroth-version/
Credit goes to the user @tvenn
**/
namespace NinjaTrader.NinjaScript.Indicators
{
	public class RangeFilterSimple : Indicator
	{
		private Series<double> x;
		private Series<double> xCalc;
		private Series<double> avrng;
		private Series<double> rngfilt;
		private Series<double> upward;
		private Series<double> downward;
		private Series<double> condIni;
		private Series<double> Buy;
		private Series<double> Sell;
		private Series<double> trend;
		private SimpleFont	font;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "RangeFilterSimple";
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
				Period					= 100;
				AreaOpacity				= 30;
				Multiplier					= 3;
				AddPlot(Brushes.Green, "HBand");
				AddPlot(Brushes.Red, "LBand");
				AddPlot(new Stroke(Brushes.White, 5), PlotStyle.Line, "RangeFilter");
				
				font = new Gui.Tools.SimpleFont("Arial", 12);
			}
			else if (State == State.Configure)
			{
				x = new Series<double>(this, MaximumBarsLookBack.Infinite);
				xCalc = new Series<double>(this, MaximumBarsLookBack.Infinite);
				avrng = new Series<double>(this, MaximumBarsLookBack.Infinite);
				rngfilt = new Series<double>(this, MaximumBarsLookBack.Infinite);
				upward = new Series<double>(this, MaximumBarsLookBack.Infinite);
				downward = new Series<double>(this, MaximumBarsLookBack.Infinite);
				condIni = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Buy = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Sell = new Series<double>(this, MaximumBarsLookBack.Infinite);
				trend = new Series<double>(this, MaximumBarsLookBack.Infinite);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 2) {
				x[0] = 0;
				xCalc[0] = 0;
				avrng[0] = 0;
				rngfilt[0] = 0;
				upward[0] = 0;
				downward[0] = 0;
				condIni[0] = 0;
				return;
			}
			
			x[0] = Close[0];
			double smrng = smoothrng(Period, Multiplier);
			rngfilter(smrng);
			upward[0] = rngfilt[0] > rngfilt[1] ? upward[1] + 1 : rngfilt[0] < rngfilt[1] ? 0 : upward[1];
			downward[0] = rngfilt[0] < rngfilt[1] ? downward[1] + 1 : rngfilt[0] > rngfilt[1] ? 0 : downward[1];
			HBand[0] = rngfilt[0] + smrng;
			LBand[0] = rngfilt[0] - smrng;
			bool longCond = false;
			bool shortCond = false;
			longCond = Close[0] > rngfilt[0] && Close[0] > Close[1] && upward[0] > 0 || Close[0] > rngfilt[0] && Close[0] < Close[1] && upward[0] > 0;
			
			shortCond = Close[0] < rngfilt[0] && Close[0] < Close[1] && downward[0] > 0 || Close[0] < rngfilt[0] && Close[0] > Close[1] && downward[0] > 0;
			
			condIni[0] = longCond ? 1 : shortCond ? -1 : condIni[1];
			bool longCondition = longCond && condIni[1] == -1;
			bool shortCondition = shortCond && condIni[1] == 1;
			
			if (longCondition) {
				Buy[0] = 1;
				//Draw.ArrowUp(this, "Buy-"+CurrentBar, true, 1 , Low[1] - TickSize, Brushes.Green);
				Draw.Text(this, "UpArrow-" + Convert.ToString(CurrentBars[0]), "▲", 0, (Low[0] + (-11 * TickSize)), Brushes.Green);
				//Draw.Text(this, "BuyText-"+Convert.ToString(CurrentBars[0]), true, "Buy",1, Low[1] - 15*TickSize, 0, Brushes.White, font, TextAlignment.Center, Brushes.Green, Brushes.DarkGreen, 100);
			}
			
			if (shortCondition) {
				Sell[0] = 1;
				//Draw.ArrowDown(this, "DownArrow-"+Convert.ToString(CurrentBars[0]), true, 1 , High[1] + TickSize, Brushes.Red);
				Draw.Text(this, "DownArrow-" + Convert.ToString(CurrentBars[0]), "▼", 0, (High[0] + (11 * TickSize)), Brushes.Red);
				//Draw.Text(this, "SellText-"+Convert.ToString(CurrentBars[0]), true, "Sell",1, High[1] + 15*TickSize, 0, Brushes.White, font, TextAlignment.Center, Brushes.Red, Brushes.DarkRed, 100);
			}
			
			RangeFilter[0] = rngfilt[0];
			
			if (upward[0] > 0) {
				trend[0] = 1;
			} else if (downward[0] > 0) {
				trend[0] = -1;
			} else {
				trend[0] = 0;
			}
			
			
			
			if (AreaOpacity > 0) {
				Draw.Region(this, "HBandArea"+CurrentBar, 1, 0, rngfilt, HBand, null, Brushes.Green, AreaOpacity);
			
				Draw.Region(this, "LBandArea"+CurrentBar, 1, 0, rngfilt, LBand, null, Brushes.Red, AreaOpacity);
			}
			
			//neutral zone
			
			if (RangeFilter[0] == RangeFilter[1]) {
				PlotBrushes[2][0] = Brushes.Gray;
				return;
			}
			
			PlotBrushes[2][0] = upward[0] > 0 ? Brushes.Green : downward[0] > 0 ? Brushes.Red : Brushes.White;
		}
		
		private double smoothrng(int t, double m) {
			int wper = t * 2 - 1;
			xCalc[0] = Math.Abs(x[0] - x[1]);
			avrng[0] = EMA(xCalc, t)[0];
			double smoothrng = EMA(avrng, wper)[0] * m;
			return smoothrng;
		}
		
		private void rngfilter(double r) {
			rngfilt[0] = x[0];
			if (Double.IsNaN(rngfilt[1])) {
				rngfilt[1] = 0.0;
			}
			
			rngfilt[0] = (x[0] > rngfilt[1]) ? ((x[0] - r < rngfilt[1]) ? rngfilt[1] : x[0] - r) : x[0] + r > rngfilt[1] ? rngfilt[1] : x[0] + r;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Description="Sampling Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="Multiplier", Description="Multiplier", Order=2, GroupName="Parameters")]
		public double Multiplier
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Area Opacity", Description="Area Opacity", Order=3, GroupName="Parameters")]
		public int AreaOpacity
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuySignal
		{
			get { return Buy; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellSignal
		{
			get { return Sell; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HBand
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LBand
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RangeFilter
		{
			get { return Values[2]; }
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
		private RangeFilterSimple[] cacheRangeFilterSimple;
		public RangeFilterSimple RangeFilterSimple(int period, double multiplier, int areaOpacity)
		{
			return RangeFilterSimple(Input, period, multiplier, areaOpacity);
		}

		public RangeFilterSimple RangeFilterSimple(ISeries<double> input, int period, double multiplier, int areaOpacity)
		{
			if (cacheRangeFilterSimple != null)
				for (int idx = 0; idx < cacheRangeFilterSimple.Length; idx++)
					if (cacheRangeFilterSimple[idx] != null && cacheRangeFilterSimple[idx].Period == period && cacheRangeFilterSimple[idx].Multiplier == multiplier && cacheRangeFilterSimple[idx].AreaOpacity == areaOpacity && cacheRangeFilterSimple[idx].EqualsInput(input))
						return cacheRangeFilterSimple[idx];
			return CacheIndicator<RangeFilterSimple>(new RangeFilterSimple(){ Period = period, Multiplier = multiplier, AreaOpacity = areaOpacity }, input, ref cacheRangeFilterSimple);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RangeFilterSimple RangeFilterSimple(int period, double multiplier, int areaOpacity)
		{
			return indicator.RangeFilterSimple(Input, period, multiplier, areaOpacity);
		}

		public Indicators.RangeFilterSimple RangeFilterSimple(ISeries<double> input , int period, double multiplier, int areaOpacity)
		{
			return indicator.RangeFilterSimple(input, period, multiplier, areaOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RangeFilterSimple RangeFilterSimple(int period, double multiplier, int areaOpacity)
		{
			return indicator.RangeFilterSimple(Input, period, multiplier, areaOpacity);
		}

		public Indicators.RangeFilterSimple RangeFilterSimple(ISeries<double> input , int period, double multiplier, int areaOpacity)
		{
			return indicator.RangeFilterSimple(input, period, multiplier, areaOpacity);
		}
	}
}

#endregion
