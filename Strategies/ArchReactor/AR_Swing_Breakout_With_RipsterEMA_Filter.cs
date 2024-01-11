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
	public class AR_Swing_Breakout_With_RipsterEMA_Filter : ArchReactorAlgoBase
	{
		private Swing Swing1;
		private SMA SMA1;
		private SMA SMA2;
		private NinjaTrader.NinjaScript.Indicators.TradeSaber.ReversalTS ReversalTS1;
		private NinjaTrader.NinjaScript.Indicators.RipsterEMAClouds RipsterEMAClouds1;
		private bool tradeDone = false;
		protected override void OnStateChange()
		{
			base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = @"Swing Breakout, from Arch Reactor Algo";
                Name = "AR_Swing_Breakout_With_RipsterEMA_Filter";
				StrategyName = "AR_Swing_Breakout_With_RipsterEMA_Filter";
				Strength = 5;
            }
		}

		protected override void OnBarUpdate()
		{
			base.OnBarUpdate();
			
			if (ReversalTS1.CurrentReversalBar[0] == 1 || ReversalTS1.CurrentReversalBar[0] == -1) {
				tradeDone = false;
			}
			
		}
		
		#region Strategy Management
		protected override void initializeIndicators() {
			Swing1 = Swing(Strength);
			ReversalTS1= ReversalTS(0, 0, 0, false, null, null, false, null, false);
			AddChartIndicator(Swing1);
			
			RipsterEMAClouds1 = RipsterEMAClouds(8, 9, 5, 12, 34, 50, 72, 89, 180, 200, 0, 3);
			AddChartIndicator(RipsterEMAClouds1);
		}
		
		protected override bool validateEntryLong() {
			if (tradeDone == false && Swing1.SwingHigh[0] < Close[0] && validateRipsterCloud(false) == true) {
				tradeDone = true;
				return true;
			}
			return false;
		}
		
		protected override bool validateEntryShort() {
			if (tradeDone == false && Swing1.SwingLow[0] > Close[0] && validateRipsterCloud(true) == true) {
				tradeDone = true;
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
		#endregion
		
		#region Properties

        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Strength", Order=1, GroupName="1.1 Strategy Params - Swing")]
		public int Strength
		{ get; set; }
		
		#endregion
		
	}
}
