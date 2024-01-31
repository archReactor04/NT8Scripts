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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.ArchReactor
{
	public class AR_Swing_Breakout : ArchReactorAlgoBase
	{
		private Swing Swing1;
		private SMA SMA1;
		private SMA SMA2;
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaADXVMA amaADXVMA1;
		private NinjaTrader.NinjaScript.Indicators.TradeSaber.ReversalTS ReversalTS1;
		private NinjaTrader.NinjaScript.Indicators.RangeFilterSimple RangeFilter1;
		private NinjaTrader.NinjaScript.Indicators.RipsterEMAClouds RipsterEMAClouds1;
		private NinjaTrader.NinjaScript.Indicators.MovingAverageRibbon MARibbon;
		private bool tradeDone = false;
		private double previousSwingHigh; 
		private double previousSwingLow;
		protected override void OnStateChange()
		{
			base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = @"Swing Breakout with ADXVMA filter, from Arch Reactor Algo";
                Name = "AR_Swing_Breakout_ADXVMA";
				StrategyName = "AR_Swing_Breakout_ADXVMA";
				Strength = 5;
				Period = 8;
				previousSwingLow = 0;
				previousSwingHigh = 0;
            }
		}

		protected override void OnBarUpdate()
		{ 
			base.OnBarUpdate();
			
			if (ReversalTS1.CurrentReversalBar[0] == 1 || ReversalTS1.CurrentReversalBar[0] == -1 || IsStratEnabled == false) {
				tradeDone = false;
			}
			
			if (Position.MarketPosition == MarketPosition.Long) {
				previousSwingHigh = Swing1.SwingHigh[0];
			}
			
			if (Position.MarketPosition == MarketPosition.Short) {
				previousSwingLow = Swing1.SwingLow[0];
			}
			
		}
		
		#region Strategy Management
		protected override void initializeIndicators() {
			Swing1 = Swing(Strength);
			amaADXVMA1 = amaADXVMA(Period);
			ReversalTS1= ReversalTS(0, 0, 0, false, null, null, false, null, false);
			//RangeFilter1 = RangeFilterSimple(100, 2, 30);
			AddChartIndicator(Swing1);
			AddChartIndicator(amaADXVMA1);
			//RipsterEMAClouds1 = RipsterEMAClouds(8, 9, 5, 12, 34, 50, 72, 89, 180, 200, 0, 3);
			//AddChartIndicator(RipsterEMAClouds1);
			//AddChartIndicator(RangeFilter1);
		}
		
		protected override bool validateEntryLong() {
			if (tradeDone == false 
				&& Swing1.SwingHigh[0] < Close[1] 
				&& amaADXVMA1.Trend[0] == 1
				&& previousSwingHigh != Swing1.SwingHigh[0]){
				//&& RangeFilter1.Trend[0] == 1) {
				tradeDone = true;
				previousSwingHigh = Swing1.SwingHigh[0];
				return true;
			}
			return false;
		}
		
		protected override bool validateEntryShort() {
			if (tradeDone == false 
				&& Swing1.SwingLow[0] > Close[1] 
				&& amaADXVMA1.Trend[0] == -1
				&& previousSwingLow != Swing1.SwingLow[0]){
				//&& RangeFilter1.Trend[0] == -1) {
				tradeDone = true;
				previousSwingLow = Swing1.SwingLow[0];
				return true;
			}
			return false;
		}
		
		private bool validateRipsterCloud(bool isShort) {
			/*if (Enable_EMA_Trend_Filter == false) {
				return true;
			}*/
			
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
		
		private int validateMovingAverageRibbon() {
			if(MARibbon.MovingAverage1[0] < MARibbon.MovingAverage2[0] 
				&& MARibbon.MovingAverage2[0] < MARibbon.MovingAverage3[0]
				&& MARibbon.MovingAverage3[0] < MARibbon.MovingAverage4[0]
				&& MARibbon.MovingAverage4[0] < MARibbon.MovingAverage5[0]
				&& MARibbon.MovingAverage5[0] < MARibbon.MovingAverage6[0]
				&& MARibbon.MovingAverage6[0] < MARibbon.MovingAverage7[0]
				&& MARibbon.MovingAverage7[0] < MARibbon.MovingAverage8[0]) {
					return -1;
				}
				
				if(MARibbon.MovingAverage1[0] > MARibbon.MovingAverage2[0] 
				&& MARibbon.MovingAverage2[0] > MARibbon.MovingAverage3[0]
				&& MARibbon.MovingAverage3[0] > MARibbon.MovingAverage4[0]
				&& MARibbon.MovingAverage4[0] > MARibbon.MovingAverage5[0]
				&& MARibbon.MovingAverage5[0] > MARibbon.MovingAverage6[0]
				&& MARibbon.MovingAverage6[0] > MARibbon.MovingAverage7[0]
				&& MARibbon.MovingAverage7[0] > MARibbon.MovingAverage8[0]) {
					return 1;
				}
				return 0;
		}
		
		#endregion
		
		#region Properties

        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Strength", Order=1, GroupName="1.1 Strategy Params - Swing")]
		public int Strength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="1.1 Strategy Params - ADXVMA")]
		public int Period
		{ get; set; }
		
		#endregion
		
	}
}
