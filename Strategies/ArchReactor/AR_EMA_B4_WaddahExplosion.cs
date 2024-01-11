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
using BltTriggerLines.Common;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.ArchReactor
{
	public class AR_EMA_B4_WaddahExplosion : ArchReactorAlgoBase
	{
		private NinjaTrader.NinjaScript.Indicators.B4v3 B4v31;
		private EMA EMA1;
		private EMA EMA2;
		private NinjaTrader.NinjaScript.Indicators.WaddahAttarExplosionCalc WaddahAttarExplosionCalc1;
		private NinjaTrader.NinjaScript.Indicators.BltTriggerLines BltTriggerLines1;
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "AR_EMA_B4_WaddahExplosion";
				StrategyName								= "AR_EMA_B4_WaddahExplosion";
				Calculate									= Calculate.OnPriceChange;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			base.OnBarUpdate();
		}
		
		#region Strategy Management
		protected override void initializeIndicators() {
       		B4v31 = B4v3(true, true, true, true, 5, 12, 26, 15, 1, 11, 23.6, true, 30);
			WaddahAttarExplosionCalc1 = WaddahAttarExplosionCalc(150, 20, true, 7, 40, true, 7, 20, 2, 20);
			EMA1 = EMA(34);
			EMA1.Plots[0].Brush = Brushes.Violet;
			EMA1.Plots[0].Width = 4;
			EMA2 = EMA(200);
			EMA2.Plots[0].Brush = Brushes.White;
			EMA2.Plots[0].Width = 4;
			BltTriggerLines1 = BltTriggerLines(BltMAType.ZLEMA, 80, BltMAType.DEMA, 20, ColorStyle.RegionColors, false, 0, null, null, true, 40, Brushes.Chartreuse, Brushes.Red, false, null, Brushes.Green, Brushes.Red, Brushes.Green, Brushes.Red); 
			AddChartIndicator(B4v31);
			AddChartIndicator(WaddahAttarExplosionCalc1);
			AddChartIndicator(EMA1);
			AddChartIndicator(EMA2);
			AddChartIndicator(BltTriggerLines1);
		}
		
		protected override bool validateEntryLong() {
            if (B4v31.BuySellOutput[0] == 1 
				&& WaddahAttarExplosionCalc1.TrendUp[0] > WaddahAttarExplosionCalc1.ExplosionLine[0] 
				&&  WaddahAttarExplosionCalc1.TrendUp[0] > 20
				&& Close[0] > EMA1[0]
				&& Close[0] > EMA2[0]
				&& BltTriggerLines1.UpTrend[0] == true) {
					return true;
			}
			return false;
        }
		
		protected override bool validateEntryShort() {
            if (B4v31.BuySellOutput[0] == -1 
				&& WaddahAttarExplosionCalc1.TrendDown[0] > WaddahAttarExplosionCalc1.ExplosionLine[0] 
				&&  WaddahAttarExplosionCalc1.TrendDown[0] > 20
				&& Close[0] < EMA1[0]
				&& Close[0] < EMA2[0]
				&& BltTriggerLines1.UpTrend[0] == false) {
					return true;
			}
			return false;
        }
		
		protected override bool validateExitLong() {
			if (Close[0] < EMA1[0]) {
				return true;
			}
			return false;
		}
		
		protected override bool validateExitShort() {
			if (Close[0] > EMA1[0]) {
				return true;
			}
			return false;
		}
		#endregion
	}
}
