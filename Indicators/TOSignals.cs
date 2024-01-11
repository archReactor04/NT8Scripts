/* This script is a conversion of Trader Oracle Method on trading view https://www.tradingview.com/script/yE35zW1B-TraderOracle-Method/
I have only inckuded Trampoline and Shark Signals 
*/

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
	public class TOSignals : Indicator
	{
		private Series<double> rsiM;
		private Series<double> bbw;
		private Series<bool> isRed;
		private Series<bool> isGreen;
		private Series<double> upperBB;
		private Series<double> lowerBB;
		private Series<bool> weGoUp;
		private Series<bool> weGoDown;
		private Series<double> rsiChange1;
		private Series<double> rsiChange2;
		private Series<double> trampolineBuySell;
		private Series<double> sharkOBOS;
		
		 private bool isBarConfirmed = false;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "TOSignals";
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
				BollingerLowerThreshold					= 0.0015;
				RSILowerThreshold					= 25;
				RSIUpperThreshold					= 72;
				RSILength					= 14;
				BollingerBandsLength					= 20;
				BollingerBandMult					= 2;
				BollingerBandsOffset					= 0;
				Apply25_75					= false;
				AddPlot(new Stroke(Brushes.White, 10), PlotStyle.Square, "TrampolineSignal");
			}
			else if (State == State.Configure)
			{
				rsiM = new Series<double>(this);
				bbw = new Series<double>(this);
				isRed = new Series<bool>(this);
				isGreen = new Series<bool>(this);
				upperBB = new Series<double>(this);
				lowerBB = new Series<double>(this);
				weGoUp = new Series<bool>(this);
				weGoDown = new Series<bool>(this);
				rsiChange1 = new Series<double>(this);
				rsiChange2 = new Series<double>(this);
				trampolineBuySell = new Series<double>(this);
				sharkOBOS = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 6) {
				return;
			}
			
			isRed[0] = Close[0] < Open[0];
			isGreen[0] = Close[0] > Open[0];
			bool isConfirmed = !Double.IsNaN(Close[0]);
			
			// STANDARD BOLLINGER BANDS
			double basisBB = SMA(Input, BollingerBandsLength)[0];
			double devBB = BollingerBandMult * StdDev(Input, BollingerBandsLength)[0];
			upperBB[0] = basisBB + devBB;
			lowerBB[0] = basisBB - devBB;
			bool downBB = Low[0] < lowerBB[0] || High[0] < lowerBB[0];
			bool upBB = Low[0] > upperBB[0] || High[0] > upperBB[0];
			bbw[0] = (upperBB[0] - lowerBB[0]) / basisBB;
			
			// RSI
			rsiChange1[0] = Math.Max(Input[0] - Input[1], 0);
			double up = WildersMovingAverage(rsiChange1, RSILength)[0];
			rsiChange2[0] = -Math.Min(Input[0] - Input[1], 0);
			double down = WildersMovingAverage(rsiChange2, RSILength)[0];
			rsiM[0] = down == 0 ? 100 : up == 0 ? 0 : 100 - (100 / (1 + up / down));
			
			bool back1 = isRed[1] && rsiM[1] <= RSILowerThreshold && Close[1] < lowerBB[1] && bbw[1] > BollingerLowerThreshold;
			bool back2 = isRed[2] && rsiM[2] <= RSILowerThreshold && Close[2] < lowerBB[2] && bbw[2] > BollingerLowerThreshold;
			bool back3 = isRed[3] && rsiM[3] <= RSILowerThreshold && Close[3] < lowerBB[3] && bbw[3] > BollingerLowerThreshold;
			bool back4 = isRed[4] && rsiM[4] <= RSILowerThreshold && Close[4] < lowerBB[4] && bbw[4] > BollingerLowerThreshold;
			bool back5 = isRed[5] && rsiM[5] <= RSILowerThreshold && Close[5] < lowerBB[5] && bbw[5] > BollingerLowerThreshold;
			
			bool for1 = isGreen[1] && rsiM[1] >= RSIUpperThreshold && Close[1] > upperBB[1] && bbw[1] > BollingerLowerThreshold;
			bool for2 = isGreen[2] && rsiM[2] >= RSIUpperThreshold && Close[2] > upperBB[2] && bbw[2] > BollingerLowerThreshold;
			bool for3 = isGreen[3] && rsiM[3] >= RSIUpperThreshold && Close[3] > upperBB[3] && bbw[3] > BollingerLowerThreshold;
			bool for4 = isGreen[4] && rsiM[4] >= RSIUpperThreshold && Close[4] > upperBB[4] && bbw[4] > BollingerLowerThreshold;
			bool for5 = isGreen[5] && rsiM[5] >= RSIUpperThreshold && Close[5] > upperBB[5] && bbw[5] > BollingerLowerThreshold;
			
			weGoUp[0]= isGreen[0] && (back1 || back2 || back3 || back4 || back5) && (High[0] > High[1]) && isConfirmed;
			bool upThrust = weGoUp[0] && !weGoUp[1] && !weGoUp[2] && !weGoUp[3] && !weGoUp[4];
			weGoDown[0] = isRed[0] && (for1 || for2 || for3 || for4 || for5) && (Low[0] < Low[1]) && isConfirmed;
			bool downThrust = weGoDown[0] && !weGoDown[1] && !weGoDown[2] && !weGoDown[3] && !weGoDown[4];
			TrampolineBuySell[0] = 0;
			SimpleFont font = new SimpleFont("Courier New", 12) {Bold = true };
			if (upThrust) {
				TrampolineSignal[0] = Low[0] - 5*TickSize;
				trampolineBuySell[0] = 1;
				PlotBrushes[0][0] = Brushes.Lime;
				Draw.Text(this, "TrampolineBuy"+CurrentBar, true, "T", 0, Low[0] - 15*TickSize, 0, Brushes.White, font, TextAlignment.Center, Brushes.Green, Brushes.Lime, 20);
			} else if (downThrust) {
				TrampolineSignal[0] = High[0] + 5*TickSize;
				trampolineBuySell[0] = -1;
				PlotBrushes[0][0] = Brushes.Red;
				Draw.Text(this, "TrampolineSell"+CurrentBar, true, "T", 0, High[0] + 15*TickSize, 0, Brushes.White, font, TextAlignment.Center, Brushes.Red, Brushes.Salmon, 20);
			}
			
			
			double ema50 = EMA(Close, 50)[0];
			double ema200 = EMA(Close, 200)[0];
			double ema400 = EMA(Close, 400)[0];
			double ema800 = EMA(Close, 800)[0];
			double wapwap = VWAP(Close)[0];
			
			bool bTouchedLine = (ema50<High[0] && ema50>Low[0]) || (ema200<High[0] && ema200>Low[0]) || (ema400<High[0] && ema400>Low[0]) || (ema800<High[0] && ema800>Low[0]) || (wapwap<High[0] && wapwap>Low[0]);
			
			double basis5 = SMA(rsiM, 30)[0];
			double dev = 2.0 * StdDev(rsiM, 30)[0]; 
			double upper = basis5 + dev;
			double lower = basis5 - dev;
			
			bool bBelow25 = rsiM[0] < 26;
			bool bAbove75 = rsiM[0] > 74;
			if(Apply25_75 == false) {
			    bBelow25 = true;
			    bAbove75 = true;
			}
			
			bool bShowSharkDown = (rsiM[0] > upper && bAbove75) && isConfirmed;
			bool bShowSharkUp = (rsiM[0] < lower && bBelow25) && isConfirmed;
			
			if (bShowSharkDown == true) {
				sharkOBOS[0] = 1;
				Draw.Text(this, "SharkSell"+CurrentBar, true, "🦈", 0, High[0] + 15*TickSize, 0, Brushes.Green, font, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 20);
			} else if (bShowSharkUp == true) 
			{
				sharkOBOS[0] = -1;
				Draw.Text(this, "SharkBuy"+CurrentBar, true, "🦈", 0, Low[0] - 15*TickSize, 0, Brushes.Red, font, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 20);
			} 
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="BollingerLowerThreshold", Description="0.003 for daily, 0.0015 for 30 min candles", Order=1, GroupName="Parameters")]
		public double BollingerLowerThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSILowerThreshold", Description="Normally 25", Order=2, GroupName="1. Trampoline")]
		public int RSILowerThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSIUpperThreshold", Description="Normally 75", Order=3, GroupName="1. Trampoline")]
		public int RSIUpperThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSILength", Description="RSI Length", Order=4, GroupName="1. Trampoline")]
		public int RSILength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BollingerBandsLength", Order=5, GroupName="1. Trampoline")]
		public int BollingerBandsLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.001, double.MaxValue)]
		[Display(Name="BollingerBandMult", Order=6, GroupName="1. Trampoline")]
		public double BollingerBandMult
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-500, int.MaxValue)]
		[Display(Name="BollingerBandsOffset", Order=7, GroupName="1. Trampoline")]
		public int BollingerBandsOffset
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrampolineSignal
		{
			get { return Values[0]; }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Apply25_75", Description="Apply the 25/75 Rule", Order=1, GroupName="2. Shark Signals")]
		public bool Apply25_75
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrampolineBuySell
		{
			get { return trampolineBuySell; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SharkOBOS
		{
			get { return sharkOBOS; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ArchReactor.TOSignals[] cacheTOSignals;
		public ArchReactor.TOSignals TOSignals(double bollingerLowerThreshold, int rSILowerThreshold, int rSIUpperThreshold, int rSILength, int bollingerBandsLength, double bollingerBandMult, int bollingerBandsOffset, bool apply25_75)
		{
			return TOSignals(Input, bollingerLowerThreshold, rSILowerThreshold, rSIUpperThreshold, rSILength, bollingerBandsLength, bollingerBandMult, bollingerBandsOffset, apply25_75);
		}

		public ArchReactor.TOSignals TOSignals(ISeries<double> input, double bollingerLowerThreshold, int rSILowerThreshold, int rSIUpperThreshold, int rSILength, int bollingerBandsLength, double bollingerBandMult, int bollingerBandsOffset, bool apply25_75)
		{
			if (cacheTOSignals != null)
				for (int idx = 0; idx < cacheTOSignals.Length; idx++)
					if (cacheTOSignals[idx] != null && cacheTOSignals[idx].BollingerLowerThreshold == bollingerLowerThreshold && cacheTOSignals[idx].RSILowerThreshold == rSILowerThreshold && cacheTOSignals[idx].RSIUpperThreshold == rSIUpperThreshold && cacheTOSignals[idx].RSILength == rSILength && cacheTOSignals[idx].BollingerBandsLength == bollingerBandsLength && cacheTOSignals[idx].BollingerBandMult == bollingerBandMult && cacheTOSignals[idx].BollingerBandsOffset == bollingerBandsOffset && cacheTOSignals[idx].Apply25_75 == apply25_75 && cacheTOSignals[idx].EqualsInput(input))
						return cacheTOSignals[idx];
			return CacheIndicator<ArchReactor.TOSignals>(new ArchReactor.TOSignals(){ BollingerLowerThreshold = bollingerLowerThreshold, RSILowerThreshold = rSILowerThreshold, RSIUpperThreshold = rSIUpperThreshold, RSILength = rSILength, BollingerBandsLength = bollingerBandsLength, BollingerBandMult = bollingerBandMult, BollingerBandsOffset = bollingerBandsOffset, Apply25_75 = apply25_75 }, input, ref cacheTOSignals);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ArchReactor.TOSignals TOSignals(double bollingerLowerThreshold, int rSILowerThreshold, int rSIUpperThreshold, int rSILength, int bollingerBandsLength, double bollingerBandMult, int bollingerBandsOffset, bool apply25_75)
		{
			return indicator.TOSignals(Input, bollingerLowerThreshold, rSILowerThreshold, rSIUpperThreshold, rSILength, bollingerBandsLength, bollingerBandMult, bollingerBandsOffset, apply25_75);
		}

		public Indicators.ArchReactor.TOSignals TOSignals(ISeries<double> input , double bollingerLowerThreshold, int rSILowerThreshold, int rSIUpperThreshold, int rSILength, int bollingerBandsLength, double bollingerBandMult, int bollingerBandsOffset, bool apply25_75)
		{
			return indicator.TOSignals(input, bollingerLowerThreshold, rSILowerThreshold, rSIUpperThreshold, rSILength, bollingerBandsLength, bollingerBandMult, bollingerBandsOffset, apply25_75);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ArchReactor.TOSignals TOSignals(double bollingerLowerThreshold, int rSILowerThreshold, int rSIUpperThreshold, int rSILength, int bollingerBandsLength, double bollingerBandMult, int bollingerBandsOffset, bool apply25_75)
		{
			return indicator.TOSignals(Input, bollingerLowerThreshold, rSILowerThreshold, rSIUpperThreshold, rSILength, bollingerBandsLength, bollingerBandMult, bollingerBandsOffset, apply25_75);
		}

		public Indicators.ArchReactor.TOSignals TOSignals(ISeries<double> input , double bollingerLowerThreshold, int rSILowerThreshold, int rSIUpperThreshold, int rSILength, int bollingerBandsLength, double bollingerBandMult, int bollingerBandsOffset, bool apply25_75)
		{
			return indicator.TOSignals(input, bollingerLowerThreshold, rSILowerThreshold, rSIUpperThreshold, rSILength, bollingerBandsLength, bollingerBandMult, bollingerBandsOffset, apply25_75);
		}
	}
}

#endregion
