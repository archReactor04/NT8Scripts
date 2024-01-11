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
		private bool tradeDone = false;
		private double previousSwingLow;
		private double previousSwingHigh;
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
			amaADXVMA1 = amaADXVMA(Period);
			ReversalTS1= ReversalTS(0, 0, 0, false, null, null, false, null, false);
			AddChartIndicator(Swing1);
			AddChartIndicator(amaADXVMA1);
		}
		
		protected override bool validateEntryLong() {
			if (tradeDone == false 
				&& Swing1.SwingHigh[0] < Close[1] 
				&& amaADXVMA1.Trend[0] == 1
				&& previousSwingHigh != Swing1.SwingHigh[0]) {
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
				&& previousSwingLow != Swing1.SwingLow[0]) {
				tradeDone = true;
				previousSwingLow = Swing1.SwingLow[0];
				return true;
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
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="1.1 Strategy Params - ADXVMA")]
		public int Period
		{ get; set; }
		
		#endregion
		
	}
}
