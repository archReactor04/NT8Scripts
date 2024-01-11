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
namespace NinjaTrader.NinjaScript.Indicators.ArchReactor
{
	public class TwinRangeFilter : Indicator
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
		private Series<double> HBand;
		private Series<double> LBand;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "TwinRangeFilter";
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
				FastPeriod					= 27;
				FastRange					= 1.6;
				SlowPeriod					= 55;
				SlowRange					= 2;
				AddPlot(new Stroke(Brushes.Red, 8), PlotStyle.TriangleDown, "Short");
				AddPlot(new Stroke(Brushes.Green, 8), PlotStyle.TriangleUp, "Long");
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
				HBand = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LBand = new Series<double>(this, MaximumBarsLookBack.Infinite);	
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
			double smrng1 = smoothrng(FastPeriod, FastRange);
			double smrng2 = smoothrng(SlowPeriod, SlowRange);
			double smrng = (smrng1 + smrng2) / 2;
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
			
			if (longCondition == true) {
				Long[0] = Low[0] - 5*TickSize;
			} else if (shortCondition == true) {
				Short[0] = High[0] + 5*TickSize;
			}
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
		[Display(Name="FastPeriod", Description="Fast Period", Order=1, GroupName="Parameters")]
		public int FastPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="FastRange", Description="Fast Range", Order=2, GroupName="Parameters")]
		public double FastRange
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowPeriod", Description="Slow Period", Order=3, GroupName="Parameters")]
		public int SlowPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="SlowRange", Description="Slow Range", Order=4, GroupName="Parameters")]
		public double SlowRange
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Short
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Long
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ArchReactor.TwinRangeFilter[] cacheTwinRangeFilter;
		public ArchReactor.TwinRangeFilter TwinRangeFilter(int fastPeriod, double fastRange, int slowPeriod, double slowRange)
		{
			return TwinRangeFilter(Input, fastPeriod, fastRange, slowPeriod, slowRange);
		}

		public ArchReactor.TwinRangeFilter TwinRangeFilter(ISeries<double> input, int fastPeriod, double fastRange, int slowPeriod, double slowRange)
		{
			if (cacheTwinRangeFilter != null)
				for (int idx = 0; idx < cacheTwinRangeFilter.Length; idx++)
					if (cacheTwinRangeFilter[idx] != null && cacheTwinRangeFilter[idx].FastPeriod == fastPeriod && cacheTwinRangeFilter[idx].FastRange == fastRange && cacheTwinRangeFilter[idx].SlowPeriod == slowPeriod && cacheTwinRangeFilter[idx].SlowRange == slowRange && cacheTwinRangeFilter[idx].EqualsInput(input))
						return cacheTwinRangeFilter[idx];
			return CacheIndicator<ArchReactor.TwinRangeFilter>(new ArchReactor.TwinRangeFilter(){ FastPeriod = fastPeriod, FastRange = fastRange, SlowPeriod = slowPeriod, SlowRange = slowRange }, input, ref cacheTwinRangeFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ArchReactor.TwinRangeFilter TwinRangeFilter(int fastPeriod, double fastRange, int slowPeriod, double slowRange)
		{
			return indicator.TwinRangeFilter(Input, fastPeriod, fastRange, slowPeriod, slowRange);
		}

		public Indicators.ArchReactor.TwinRangeFilter TwinRangeFilter(ISeries<double> input , int fastPeriod, double fastRange, int slowPeriod, double slowRange)
		{
			return indicator.TwinRangeFilter(input, fastPeriod, fastRange, slowPeriod, slowRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ArchReactor.TwinRangeFilter TwinRangeFilter(int fastPeriod, double fastRange, int slowPeriod, double slowRange)
		{
			return indicator.TwinRangeFilter(Input, fastPeriod, fastRange, slowPeriod, slowRange);
		}

		public Indicators.ArchReactor.TwinRangeFilter TwinRangeFilter(ISeries<double> input , int fastPeriod, double fastRange, int slowPeriod, double slowRange)
		{
			return indicator.TwinRangeFilter(input, fastPeriod, fastRange, slowPeriod, slowRange);
		}
	}
}

#endregion
