// @Version: 2.0
// This strategy is developed in collaboration with few members from TraderRob's discord server. Please test this on SIM account before you use it live.

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.ArchReactor {
    public class AR_NadarayaWatson_RSI_Strategy : ArchReactorAlgoBase {
        
        #region Variables
			#region Strategy variables
		//private NinjaTrader.NinjaScript.Indicators.RangeFilterSimple RangeFilterSimple1;
		private NinjaTrader.NinjaScript.Indicators.NadarayaWatsonEnvelopeWithATRNonRepaint NadarayaWatsonEnvelopeWithATRNonRepaint1;
		private NinjaTrader.NinjaScript.Indicators.RSI RSI1;
		private NinjaTrader.NinjaScript.Indicators.ArchReactor.TOSignals TOSignals1;
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaCamarillaPivotsDaily amaCamarillaPivotsDaily1;
			#endregion
		#endregion
		
		#region OnStateChange
        protected override void OnStateChange()
		{	
			base.OnStateChange();
			
            if (State == State.SetDefaults)
            {
                Description = @"Nadaraya Watson Envelope and RSI (version 2.1)";
                Name = "AR_NadarayaWatson_RSI_Strategy_v2.1";
				StrategyName = "AR_NadarayaWatson_RSI version 2.1";
				Calculate									= Calculate.OnBarClose;

                #region Indicator Variable Initialization
					#region RSI
				RSI_Period = 14;
				RSI_Smooth = 3;
				RSI_High = 70;
				RSI_Low = 30;
				#endregion
				
					#region NadarayaWatson RQK
				NW_H = 8;
				NW_R	= 8;
				NW_X0 = 25;
				NW_Lag = 2;
				NW_SmoothColors = false;
				NW_Lag = 2;
				NW_ATR_Length = 32;
				NW_ATR_Mult = 2.7;
				NW_ADX_Filter					= true; // Adding RSI to filter out buy and sell signals;
				NW_ADX_Period					= 14;
				NW_ADX_Min						= 25;
					#endregion
				
					#region TO Signals
				TradeTrampolineSignals		= false;
				Tramp_BollingerLowerThreshold					= 0.0015;
				Tramp_RSILowerThreshold					= 25;
				Tramp_RSIUpperThreshold					= 72;
				Tramp_RSILength					= 14;
				Tramp_BollingerBandsLength					= 20;
				Tramp_BollingerBandMult					= 2;
				Tramp_BollingerBandsOffset					= 0;
				Shark_Apply25_75					= false;
				ConfirmWithSharkSignals		= false;
					#endregion
					
					#region Nadaraya watson RSI
				TradeOnConfirmationCandle 		= false;
				TradeNadarayaWatsonRSI 			= true;
					#endregion
				
					#region Camarilla Pivots
				TradeR3S3Reversal = false;
				TradeR4S4Breakout = false;
					#endregion
				#endregion
            }
        }
        #endregion
		
		#region onBarUpdate
		protected override void OnBarUpdate()
		{ 
            base.OnBarUpdate();
        }
        #endregion

       #region Strategy Management
		protected override void initializeIndicators() {
			NadarayaWatsonEnvelopeWithATRNonRepaint1 = NadarayaWatsonEnvelopeWithATRNonRepaint(Close, NW_H, NW_R, Convert.ToInt32(NW_X0), true, NW_SmoothColors, NW_Lag, NW_ATR_Length, NW_ATR_Mult, NW_ADX_Filter, NW_ADX_Period, NW_ADX_Min);
            RSI1 = RSI(RSI_Period, RSI_Smooth);
			
			AddChartIndicator(NadarayaWatsonEnvelopeWithATRNonRepaint1);
			AddChartIndicator(RSI1);
			
			if (TradeTrampolineSignals == true) {
				TOSignals1 = TOSignals(Tramp_BollingerLowerThreshold, Tramp_RSILowerThreshold, Tramp_RSIUpperThreshold, Tramp_RSILength, Tramp_BollingerBandsLength, Tramp_BollingerBandMult, Tramp_BollingerBandsOffset, Shark_Apply25_75);
				AddChartIndicator(TOSignals1);
			}
			
			if (TradeR3S3Reversal == true || TradeR4S4Breakout == true) {
				amaCamarillaPivotsDaily1 = amaCamarillaPivotsDaily(amaSessionTypeCamD.Daily_Bars, amaCalcModeCamD.Daily_Data);
				amaCamarillaPivotsDaily1.Plots[3].Brush = Brushes.Red;
                amaCamarillaPivotsDaily1.Plots[4].Brush = Brushes.Blue;
				amaCamarillaPivotsDaily1.Plots[5].Brush = Brushes.Red;
                amaCamarillaPivotsDaily1.Plots[6].Brush = Brushes.Blue;
				amaCamarillaPivotsDaily1.Plots[7].Brush = Brushes.Red;
                amaCamarillaPivotsDaily1.Plots[8].Brush = Brushes.Blue;
				amaCamarillaPivotsDaily1.Plots[11].Brush = Brushes.Red;
                amaCamarillaPivotsDaily1.Plots[12].Brush = Brushes.Blue;
				amaCamarillaPivotsDaily1.Plots[13].Brush = Brushes.Red;
                amaCamarillaPivotsDaily1.Plots[14].Brush = Brushes.Blue;
				AddChartIndicator(amaCamarillaPivotsDaily1);
			}
				
		}
		
        protected override bool validateEntryLong() {
			
			if (TradeR3S3Reversal == true) {
				if (validateCamarillaPivotsReversalLong() == true) {
					Print("Entering Long Camarilla Reversal");
					return true;
				}
			}
			
			if (TradeR4S4Breakout == true) {
				if (validateCamarillaPivotsBreakoutLong() == true) {
					Print("Entering Long Camarilla Breakout");
					return true;
				}
			}
			
			if (TradeNadarayaWatsonRSI == true) {
				if (TradeOnConfirmationCandle == true) {
					 if ((NadarayaWatsonEnvelopeWithATRNonRepaint1.Signal[1] == 1 || NadarayaWatsonEnvelopeWithATRNonRepaint1.Signal[1] == -1)
						&& (Close[0] > High[1])
						 && (RSI1.Avg[1] <= RSI_Low)
						  && (IsRising(RSI1.Avg))
							&& (Open[0] < Close[0])) {
							Print("Entering Long Nadaraya Watson - RSI on Confirmation Candle");
							return true;
					}
				} else {
					if ((NadarayaWatsonEnvelopeWithATRNonRepaint1.Signal[0] == 1 || NadarayaWatsonEnvelopeWithATRNonRepaint1.Signal[0] == -1)
						 && (RSI1.Avg[0] <= RSI_Low)
						  && (IsRising(RSI1.Avg))
							&& (Open[0] < Close[0])) {
								Print("Entering Long Nadaraya Watson - RSI");
							return true;
					}
				}
			}
			
			if (TradeTrampolineSignals == true) {
				bool confirmSharkSignal = (ConfirmWithSharkSignals == true) ? TOSignals1.SharkOBOS[1] == -1 : true;
				
				if (TOSignals1.TrampolineBuySell[0] == 1 && confirmSharkSignal == true && Close[0] < NadarayaWatsonEnvelopeWithATRNonRepaint1.LowerBand[0] ) {
					Print("Entering Long Trampoline");
					return true;
				}
			}
			
			return false;
        }

        protected override bool validateEntryShort() {
		//	int barsAgo = MRO(delegate { return RSI1.Default[0] >= RSI_High;}, 1, 10);
			
			if (TradeR3S3Reversal == true) {
				if (validateCamarillaPivotsReversalShort() == true) {
					Print("Entering Short Camarilla Reversal");
					return true;
				}
			}
			
			if (TradeR4S4Breakout == true) {
				if (validateCamarillaPivotsBreakoutShort() == true) {
					Print("Entering Short Camarilla Breakout");
					return true;
				}
			}
			
			if (TradeNadarayaWatsonRSI == true) {
				if (TradeOnConfirmationCandle == true) {
					
		            if ((NadarayaWatsonEnvelopeWithATRNonRepaint1.Signal[1] == -1)
						&& (Close[0] < Low[1])
						 && (RSI1.Default[1] >= RSI_High)
						  && IsFalling(RSI1.Avg)
							&& Open[0] > Close[0]) {
								Print("Entering Short Nadaraya Watson - RSI on Confirmation Candle");
								return true;
					}
				} else {
					if ((NadarayaWatsonEnvelopeWithATRNonRepaint1.Signal[0] == -1)
						 && (RSI1.Default[0] >= RSI_High)
						// && (barsAgo > 1 && barsAgo < 3)
						  && IsFalling(RSI1.Avg)
							&& Open[0] > Close[0]) {
								Print("Entering Short Nadaraya Watson - RSI");
								return true;
					}
				}
			}
			
			if (TradeTrampolineSignals == true) {
				bool confirmSharkSignal = (ConfirmWithSharkSignals == true) ? TOSignals1.SharkOBOS[1] == 1 : true;
				
				if (TOSignals1.TrampolineBuySell[0] == -1 && confirmSharkSignal == true && Close[0] > NadarayaWatsonEnvelopeWithATRNonRepaint1.UpperBand[0]) {
					Print("Entering Short Trampoline");
					return true;
				}
			}
			
			return false;
        }
		
		private bool validateCamarillaPivotsReversalShort() {
			if ((Open[0] > Close[0])
				&& (Open[0] > amaCamarillaPivotsDaily1.R3[0])
				&& (Close[0] < amaCamarillaPivotsDaily1.R3[0])
				&& (NadarayaWatsonEnvelopeWithATRNonRepaint1.Trend[0] == -1))
				//&&  (NadarayaWatsonEnvelopeWithATRNonRepaint1.Tren))
					//&& (Open[0] > Close[0])
				//	&& (Open[0] >= amaCamarillaPivotsDaily1.S3[0]))
				{
					return true;
				}
			return false;
		}
		
		private bool validateCamarillaPivotsReversalLong() {
			if ((Open[0] < Close[0])
				&& (Open[0] < amaCamarillaPivotsDaily1.S3[0])
				&& (Close[0] > amaCamarillaPivotsDaily1.S3[0])
				&& (NadarayaWatsonEnvelopeWithATRNonRepaint1.Trend[0] == 1))
				//&& (Open[0] > Close[0])
				//	&& (Open[0] >= amaCamarillaPivotsDaily1.S3[0]))
				{
					return true;
				}
				return false;
		}
		
		private bool validateCamarillaPivotsBreakoutLong() {
			if ((Open[0] < Close[0])
				&& (Open[0] < amaCamarillaPivotsDaily1.R4[0])
				&& (Close[0] > amaCamarillaPivotsDaily1.R4[0])
				&& (NadarayaWatsonEnvelopeWithATRNonRepaint1.Trend[0] == 1)){
				return true;
			}
			return false;
		}
		
		private bool validateCamarillaPivotsBreakoutShort() {
			if ((Open[0] > Close[0])
				&& (Open[0] > amaCamarillaPivotsDaily1.S4[0])
				&& (Close[0] < amaCamarillaPivotsDaily1.S4[0])
				&& (NadarayaWatsonEnvelopeWithATRNonRepaint1.Trend[0] == -1)){
				return true;
			}
			return false;
		}

		protected override double getCustomStopLossLong() {
			return NadarayaWatsonEnvelopeWithATRNonRepaint1.UpSignal[1] ;
		}
		
		protected override double getCustomStopLossShort() {
			return NadarayaWatsonEnvelopeWithATRNonRepaint1.DnSignal[1] ;
		}
		
		protected override double getCustomProfitTargetLong() {
			return NadarayaWatsonEnvelopeWithATRNonRepaint1.MiddleBand[0] + 20 *TickSize;
		}
		
		protected override double getCustomProfitTargetShort() {
			return NadarayaWatsonEnvelopeWithATRNonRepaint1.MiddleBand[0] - 20 *TickSize;
		}
        #endregion
		
		#region Properties

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="NW_H", Order=1, GroupName="1.1 Strategy Params - Nadaraya Watson RQK")]
		public double NW_H
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="NW_R", Order=2, GroupName="1.1 Strategy Params - Nadaraya Watson RQK")]
		public double NW_R
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="NW_X_0", Order=3, GroupName="1.1 Strategy Params - Nadaraya Watson RQK")]
		public int NW_X0
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="NW_Lag", Order=4, GroupName="1.1 Strategy Params - Nadaraya Watson RQK")]
		public int NW_Lag
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "NW_SmoothColors", Order = 5, GroupName = "1.1 Strategy Params - Nadaraya Watson RQK")]
        public bool NW_SmoothColors
        { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="NW_ATR_Length", Description="ATR Length", Order=6, GroupName="1.1 Strategy Params - Nadaraya Watson RQK")]
		public int NW_ATR_Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="NW_ATR_Mult", Description="Multiplier", Order=7, GroupName="1.1 Strategy Params - Nadaraya Watson RQK")]
		public double NW_ATR_Mult
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="NW_ADX_Filter", Description="Enable/Disable Buy Sell Signal Filtering with ADX", Order=8, GroupName="1.1 Strategy Params - Nadaraya Watson RQK")]
		public bool NW_ADX_Filter
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="NW_ADX_Period", Description="ADX Period", Order=9, GroupName="1.1 Strategy Params - Nadaraya Watson RQK")]
		public int NW_ADX_Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="NW_ADX_Min", Description="ADX Minimum value to filter", Order=10, GroupName="1.1 Strategy Params - Nadaraya Watson RQK")]
		public int NW_ADX_Min
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI_Period", Order=1, GroupName="1.2 Strategy Params - RSI")]
		public int RSI_Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI_Smooth", Order=2, GroupName="1.2 Strategy Params - RSI")]
		public int RSI_Smooth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI_High", Order=3, GroupName="1.2 Strategy Params - RSI")]
		public int RSI_High
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI_Low", Order=4, GroupName="1.2 Strategy Params - RSI")]
		public int RSI_Low
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="TradeNadarayaWatsonRSI", Description="Trade NadarayaWatson RSI", Order=1, GroupName="1.3 Strategy Params - Nadaraya Watson RSI")]
		public bool TradeNadarayaWatsonRSI
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="TradeOnConfirmationCandle", Description="Waits for a confirmation candle to Close to take the trade else takes the trade on Signal Candle Close", Order=2, GroupName="1.3 Strategy Params - Nadaraya Watson RSI")]
		public bool TradeOnConfirmationCandle
		{ get; set; }
		
				
		[NinjaScriptProperty]
		[Display(Name="TradeR3S3Reversal", Description="Trade R3 and S3 Reversal", Order=1, GroupName="1.4 Strategy Params - Camarilla Pivots")]
		public bool TradeR3S3Reversal
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="TradeR4S4Breakout", Description="Trade R3 and S3 Breakout", Order=2, GroupName="1.4 Strategy Params - Camarilla Pivots")]
		public bool TradeR4S4Breakout
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="TradeTrampolineSignals", Description="Trade on TrampolineSignals", Order=1, GroupName="1.5 Strategy Params - TO Signals")]
		public bool TradeTrampolineSignals
		{ get; set; }
		
				
		[NinjaScriptProperty]
		[Display(Name="ConfirmWithSharkSignals", Description="Confirm with shark signals of OB/OS", Order=2, GroupName="1.5 Strategy Params - TO Signals")]
		public bool ConfirmWithSharkSignals
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Tramp_BollingerLowerThreshold", Description="0.003 for daily, 0.0015 for 30 min candles", Order=3, GroupName="1.5 Strategy Params - TO Signals")]
		public double Tramp_BollingerLowerThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tramp_RSILowerThreshold", Description="Normally 25", Order=4, GroupName="1.5 Strategy Params - TO Signals")]
		public int Tramp_RSILowerThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tramp_RSIUpperThreshold", Description="Normally 75", Order=5, GroupName="1.5 Strategy Params - TO Signals")]
		public int Tramp_RSIUpperThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tramp_RSILength", Description="RSI Length", Order=6, GroupName="1.5 Strategy Params - TO Signals")]
		public int Tramp_RSILength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tramp_BollingerBandsLength", Order=7, GroupName="1.5 Strategy Params - TO Signals")]
		public int Tramp_BollingerBandsLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.001, double.MaxValue)]
		[Display(Name="Tramp_BollingerBandMult", Order=8, GroupName="1.5 Strategy Params - TO Signals")]
		public double Tramp_BollingerBandMult
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-500, int.MaxValue)]
		[Display(Name="Tramp_BollingerBandsOffset", Order=9, GroupName="1.5 Strategy Params - TO Signals")]
		public int Tramp_BollingerBandsOffset
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Shark_Apply25_75", Description="Apply the 25/75 Rule - For Shark Signals", Order=10, GroupName="1.5 Strategy Params - TO Signals")]
		public bool Shark_Apply25_75
		{ get; set; }
		
		#endregion
		
		
    }
}