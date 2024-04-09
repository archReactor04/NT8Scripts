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
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaADXVMAPlus amaADXVMAPlus1;
		private NinjaTrader.NinjaScript.Indicators.TradeSaber.ReversalTS ReversalTS1;
		private bool tradeDone = false;
		private double previousSwingHigh; 
		private double previousSwingLow;
		private CommonEnums.OrderState previousOrder = CommonEnums.OrderState.BOTH;
		protected override void OnStateChange()
		{
			base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = @"Swing Breakout with ADXVMA filter, from Arch Reactor Algo";
                Name = "AR_Swing_Breakout_ADXVMA";
				StrategyName = "AR_Swing_Breakout_ADXVMA";
				StrategyVersion = "2.0";
				Strength = 5;
				Period = 8;
				ADX_Period = 8;
				VMA_Period = 8;
				Use_ADXVMAPlus = true;
				previousSwingLow = 0;
				previousSwingHigh = 0;
				EnableDistanceFromADXLine = false;
				DistanceFromADXLineInPoints = 50;
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
			amaADXVMAPlus1 = amaADXVMAPlus(false, Period, ADX_Period, VMA_Period);
			ReversalTS1= ReversalTS(0, 0, 0, false, null, null, false, null, false);
			AddChartIndicator(Swing1);
			if (Use_ADXVMAPlus == true) {
				AddChartIndicator(amaADXVMAPlus1);
			} else {
				AddChartIndicator(amaADXVMA1);
			}
		}
		
		protected override bool validateEntryLong() {
			if (Use_ADXVMAPlus == true) {
				if (tradeDone == false 
					&& Swing1.SwingHigh[0] < Close[1] 
					&& amaADXVMAPlus1.Trend[0] == 1
					&& previousSwingHigh != Swing1.SwingHigh[0]
					&& distanceFromADXLineFilter(amaADXVMAPlus1.ADXVMA[0], false)){
					tradeDone = true;
					previousSwingHigh = Swing1.SwingHigh[0];
					previousOrder = CommonEnums.OrderState.LONGS;
					return true;
				}
			} else {
				if (tradeDone == false 
					&& Swing1.SwingHigh[0] < Close[1] 
					&& amaADXVMA1.Trend[0] == 1
					&& previousSwingHigh != Swing1.SwingHigh[0]
					&& distanceFromADXLineFilter(amaADXVMA1.ADXVMA[0], false)){
					tradeDone = true;
					previousSwingHigh = Swing1.SwingHigh[0];
					previousOrder = CommonEnums.OrderState.LONGS;
					return true;
				}
			}
			return false;
		}
		
		protected override bool validateEntryShort() {
			if (Use_ADXVMAPlus == true) {
				if (tradeDone == false 
					&& Swing1.SwingLow[0] > Close[1] 
					&& amaADXVMAPlus1.Trend[0] == -1
					&& previousSwingLow != Swing1.SwingLow[0]
					&& distanceFromADXLineFilter(amaADXVMAPlus1.ADXVMA[0], true)){
					tradeDone = true;
					previousSwingLow = Swing1.SwingLow[0];
					previousOrder = CommonEnums.OrderState.SHORTS;
					return true;
				}
			} else {
				if (tradeDone == false 
					&& Swing1.SwingLow[0] > Close[1] 
					&& amaADXVMA1.Trend[0] == -1
					&& previousSwingLow != Swing1.SwingLow[0]
					&& distanceFromADXLineFilter(amaADXVMA1.ADXVMA[0], true)){
					tradeDone = true;
					previousSwingLow = Swing1.SwingLow[0];
					previousOrder = CommonEnums.OrderState.SHORTS;
					return true;
				}
			}
			return false;
		}
		
		private bool distanceFromADXLineFilter(double adxValue, bool isShort) {
			double distance = 0;
			if (isShort == true) {
				distance = adxValue - Close[0];
			} else {
				distance = Close[0] - adxValue;
			}
			if (EnableDistanceFromADXLine == true) {
				if (DistanceFromADXLineInPoints <= distance) {
					return true;
				} else {
					return false;
				}
			}
			return true;
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
		
		[NinjaScriptProperty]
		[Display(Name="Use_ADXVMAPlus", Order=2, GroupName="1.1 Strategy Params - ADXVMA")]
		public bool Use_ADXVMAPlus
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADX_Period", Order=3, GroupName="1.1 Strategy Params - ADXVMA")]
		public int ADX_Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="VMA_Period", Order=4, GroupName="1.1 Strategy Params - ADXVMA")]
		public int VMA_Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="EnableDistanceFromADXLine", Order=5, GroupName="1.1 Strategy Params - ADXVMA")]
		public bool EnableDistanceFromADXLine
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="DistanceFromADXLineInPoints", Order=6, GroupName="1.1 Strategy Params - ADXVMA")]
		public int DistanceFromADXLineInPoints
		{ get; set; }
		
		#endregion
		
	}
}