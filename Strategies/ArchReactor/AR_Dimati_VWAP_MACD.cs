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
	public class AR_Dimati_VWAP_MACD : ArchReactorAlgoBase
	{
		private NinjaTrader.NinjaScript.Indicators.Prop_Trader_Tools.FreeVWAP Vwap;
		private MACD	MACD1;
		protected override void OnStateChange()
		{
			base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = @"This is a strategy by user Dimati to be used in 5 min CL.";
                Name = "AR_Dimati_VWAP_MACD";
				StrategyName = "AR_Dimati_VWAP_MACD";
				Credits = "Strategy provided by discord user Dimati";
				
				AnchorHour = 14;
				AnchorMinute = 0;
				
				Fast = 12;
				Slow = 26;
				Smooth = 9;
				MacdDiff = 0.005;
            }
		}

		protected override void OnBarUpdate()
		{
			base.OnBarUpdate();
		}
		
		#region Strategy Management
		protected override void initializeIndicators() {
			Vwap = FreeVWAP("VWAP", false, false, false, 1,2,3, Brushes.Blue, 20, Brushes.Blue, 20, Brushes.Blue, 20, false, false, false, true, AnchorHour, AnchorMinute, false);
			AddChartIndicator(Vwap);
			
			MACD1 = MACD(Fast, Slow, Smooth);
			AddChartIndicator(MACD1);
		
		}
		
		protected override bool validateEntryLong() {
			if (Open[0] < Vwap.Vwap[0]
				&& Close[0] > Vwap.Vwap[0]
				&& MACD1.Diff[0] > MacdDiff){
					return true;
				}
			return false;
		}
		
		protected override bool validateEntryShort() {
			if (Open[0] > Vwap.Vwap[0]
				&& Close[0] < Vwap.Vwap[0]
				&& MACD1.Diff[0] < MacdDiff){
					return true;
				}
			return false;
		}
		
		#endregion
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="AnchorHour", Order=1, GroupName="1.1 Strategy Params - Vwap")]
		public int AnchorHour
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="AnchorMinute", Order=2, GroupName="1.1 Strategy Params - Vwap")]
		public int AnchorMinute
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Fast", Order=1, GroupName="1.1 Strategy Params - MACD")]
		public int Fast
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Slow", Order=2, GroupName="1.1 Strategy Params - MACD")]
		public int Slow
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Smooth", Order=3, GroupName="1.1 Strategy Params - MACD")]
		public int Smooth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="MacdDiff", Order=4, GroupName="1.1 Strategy Params - MACD")]
		public double MacdDiff
		{ get; set; }
		#endregion
	}
}
