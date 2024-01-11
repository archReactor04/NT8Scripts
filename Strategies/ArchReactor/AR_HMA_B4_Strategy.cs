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
using BltTriggerLines.Common;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.ArchReactor {
    public class AR_HMA_B4_Strategy : ArchReactorAlgoBase {
        
        #region Variables
			#region Strategy variables
		//private NinjaTrader.NinjaScript.Indicators.B4Signals.B4Indicator B4Indicator1;
		private NinjaTrader.NinjaScript.Indicators.BobC.HMAWaveSmoothFull HMAWaveSmoothFull1;
		private NinjaTrader.NinjaScript.Indicators.ADX ADX1;
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaADXVMA amaADXVMA1;
		private NinjaTrader.NinjaScript.Indicators.TradeSaber.ReversalTS ReversalTS1;
		private NinjaTrader.NinjaScript.Indicators.RipsterEMAClouds RipsterEMAClouds1;
		private NinjaTrader.NinjaScript.Indicators.BltTriggerLines BltTriggerLines1;
		private NinjaTrader.NinjaScript.Indicators.BltTriggerLines BltTriggerLines2;
		private NinjaTrader.NinjaScript.Indicators.B4v3 B4v31;
			#endregion
		private int currentReversalBarIdx = 1;
		
		private bool isProfitTargetHit = false;
		private double profitTargetPrice;
		#endregion

        #region OnStateChange
        protected override void OnStateChange()
		{
            base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = @"HMA Wave smooth indicator with B4 Signals, from Arch Reactor Algo";
                Name = "AR_HMA_B4_Strategy";
				StrategyName = "AR_HMS_B4";

                #region Indicator Variable Initialization
					#region HMA Wave
				HMAWave_Period = 64;
				HMAWave_HMABack = 1;
				HMAWave_Smooth = 1;
					#endregion
				
					#region ADX
				ADXFilter = true;
				ADX_Period	= 5;
				ADX_Min		= 40;
					#endregion
					
					#region ADXVMA
				ADXVMAFilter = false;
				ADXVMA_Period = 5;
					#endregion
				
					#region B4 LookBack
				EnableB4_Lookback = false;
				B4_Lookback	= 4;
					#endregion
					
					#region EMA Trend
				Enable_EMA_Trend_Filter = true;
					#endregion
				#endregion
            }
        }
        #endregion

		#region onBarUpdate
		protected override void OnBarUpdate()
		{ 
			if (EnableB4_Lookback == true) {
				if (ReversalTS1.CurrentReversalBar[0] == 1 || ReversalTS1.CurrentReversalBar[0] == -1) {
					currentReversalBarIdx = CurrentBar;
				}
			}
            base.OnBarUpdate();
        }
        #endregion

        #region Strategy Management
		protected override void initializeIndicators() {
           // B4Indicator1 = B4Indicator(Close, false, true, false, true, true, true, true, @"1.0.0");
			B4v31 = B4v3(true, true, true, true, 5, 12, 26, 15, 1, 11, 23.6, true, 30);
            HMAWaveSmoothFull1 = HMAWaveSmoothFull(Close, HMAWave_Period, 11, Brushes.DodgerBlue, Brushes.Khaki, 3, @"", 0, true, 11, HMAWave_HMABack, HMAWave_Smooth);
			AddChartIndicator(B4v31);
			AddChartIndicator(HMAWaveSmoothFull1);
			if (ADXFilter == true) {
				ADX1 = ADX(ADX_Period);		
				AddChartIndicator(ADX1);
			}
			
			if (ADXVMAFilter == true) {
				amaADXVMA1 = amaADXVMA(ADXVMA_Period);
				AddChartIndicator(amaADXVMA1);
			}
			
			if (EnableB4_Lookback == true) {
				ReversalTS1 = ReversalTS(0, 0, 0, true, Brushes.Violet, Brushes.Yellow, false, Brushes.Transparent, false);
				AddChartIndicator(ReversalTS1);
			}
			
			if (Enable_EMA_Trend_Filter == true) {
				RipsterEMAClouds1 = RipsterEMAClouds(8, 9, 5, 12, 34, 50, 72, 89, 180, 200, 0, 3);
				AddChartIndicator(RipsterEMAClouds1);
			}
			
			/*BltTriggerLines1 = BltTriggerLines(BltMAType.WMA, 21, BltMAType.EMA,13,ColorStyle.RegionColors, false, 0, null, null, false, 0, null, null, false, null, Brushes.Green, Brushes.Red, Brushes.Green, Brushes.Red); 
			AddChartIndicator(BltTriggerLines1);
			
			BltTriggerLines2 = BltTriggerLines(BltMAType.EMA, 63, BltMAType.EMA,21,ColorStyle.RegionColors, false, 0, null, null, false, 0, null, null, false, null, Brushes.Green, Brushes.Red, Brushes.Green, Brushes.Red); 
			AddChartIndicator(BltTriggerLines2);*/
		}
		
        protected override bool validateEntryLong() {
            if ((B4v31.BuySellOutput[0] == 1)
					&& (HMAWaveSmoothFull1.Trend[0] == 1)
					&& validateFiltersLong() == true
					&& validateB4SignalLookBack() == true
					&& validateRipsterCloud(false) == true) {
						return true;
			}
			return false;
        }
		
		private bool validateRipsterCloud(bool isShort) {
			if (Enable_EMA_Trend_Filter == false) {
				return true;
			}
			
			if (isShort == false) {
				if (RipsterEMAClouds1.EMA1Trend[0] == 1 && RipsterEMAClouds1.EMA2Trend[0] == 1 && RipsterEMAClouds1.EMA3Trend[0] == 1) {
					return true;
				}
			} else {
				if (RipsterEMAClouds1.EMA1Trend[0] == -1 && RipsterEMAClouds1.EMA2Trend[0] == -1 && RipsterEMAClouds1.EMA3Trend[0] == -1) {
					return true;
				}
			}
			return false;
		}

        protected override bool validateEntryShort() {
            if ((B4v31.BuySellOutput[0] == -1)
					&& (HMAWaveSmoothFull1.Trend[0] == -1)
						&& validateFiltersShort() == true
						&& validateB4SignalLookBack() == true
						&& validateRipsterCloud(true) == true) {
						return true;
			}
			return false;
        }
		
		private bool validateB4SignalLookBack() {
			if (EnableB4_Lookback == true) {
				return (CurrentBar - currentReversalBarIdx) <= B4_Lookback;
			} else
				return true;
		}
		
		
		private bool validateFiltersLong() {
			bool isADXValid = true;
			if (ADXFilter == true) {
				if (ADX1[0] >= ADX_Min)
					isADXValid = true;
				else 
					isADXValid = false;
			}
			
			bool isADXVMAValid = true;
			if (ADXVMAFilter == true) {
				if(amaADXVMA1.Trend[0] == 1.0)
					isADXVMAValid = true;
				else 
					isADXVMAValid = false;
			}
			
			return isADXValid == true && isADXVMAValid == true;
		}
		
		private bool validateFiltersShort() {
			bool isADXValid = true;
			if (ADXFilter == true) {
				if (ADX1[0] >= ADX_Min)
					isADXValid = true;
				else 
					isADXValid = false;
			}
			
			bool isADXVMAValid = true;
			if (ADXVMAFilter == true) {
				if(amaADXVMA1.Trend[0] == -1.0)
					isADXVMAValid = true;
				else 
					isADXVMAValid = false;
			}
			
			return isADXValid == true && isADXVMAValid == true;
		}

        protected override bool validateExitLong() {
            if (HMAWaveSmoothFull1 != null && HMAWaveSmoothFull1.Trend[0] == -1) {
				return true;
			}
			return false;
        }

        protected override bool validateExitShort() {
            if (HMAWaveSmoothFull1 != null && HMAWaveSmoothFull1.Trend[0] == 1) {
						return true;
			}
			return false;
        }
        #endregion

        #region Properties

        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="HMAWave_Period", Order=1, GroupName="1.1 Strategy Params - HMA Wave")]
		public int HMAWave_Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="HMAWave_HMABack", Order=2, GroupName="1.1 Strategy Params - HMA Wave")]
		public int HMAWave_HMABack
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="HMAWave_Smooth", Order=3, GroupName="1.1 Strategy Params - HMA Wave")]
		public int HMAWave_Smooth
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "ADXFilter", Order = 1, GroupName = "1.2 Strategy Params - ADX")]
		public bool ADXFilter
        { 
			get; set;
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADX_Period", Order=2, GroupName="1.2 Strategy Params - ADX")]
		public int ADX_Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADX_Min", Order=3, GroupName="1.2 Strategy Params - ADX")]
		public int ADX_Min
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "ADXVMAFilter", Order = 1, GroupName = "1.3 Strategy Params - ADXVMA")]
		public bool ADXVMAFilter
        { 
			get; set;
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADXVMA_Period", Order=2, GroupName="1.3 Strategy Params - ADXVMA")]
		public int ADXVMA_Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="EnableB4_Lookback", Description="How far from the reversal candle , should the signal appear and be valid to trade.", Order=1, GroupName="1.4 Strategy Params - B4 Signals")]
		public bool EnableB4_Lookback
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="B4_Lookback", Order=2, GroupName="1.4 Strategy Params - B4 Signals")]
		public int B4_Lookback
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Enable_EMA_Trend_Filter", Order=2, GroupName="1.4 Strategy Params - Ripster EMA Trend Filter")]
		public bool Enable_EMA_Trend_Filter
		{ get; set; }
        #endregion

    }
}