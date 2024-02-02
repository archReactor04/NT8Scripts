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
	public class AR_gbrm_CL_BB_MACD : ArchReactorAlgoBase
	{
		private Bollinger Bollinger1;
		private MACD	MACD1;
		protected override void OnStateChange()
		{
			base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = @"This is a strategy by user gbrm to be used in 5 min CL.";
                Name = "AR_gbrm_CL_BB_MACD";
				StrategyName = "AR_gbrm_CL_BB_MACD";
				Credits = "Strategy provided by discord user gbrm";
            }
		}

		protected override void OnBarUpdate()
		{
			base.OnBarUpdate();
		}
		
		#region Strategy Management
		protected override void initializeIndicators() {
			Bollinger1 = Bollinger(2, 20);
			AddChartIndicator(Bollinger1);
			
			MACD1 = MACD(12, 26, 9);
			AddChartIndicator(MACD1);
		
		}
		
		protected override bool validateEntryLong() {
			if (Open[0] < Bollinger1.Middle[0] 
				&& Close[0] > Bollinger1.Middle[0]
				&& MACD1.Diff[0] > 0){
					return true;
				}
			return false;
		}
		
		protected override bool validateEntryShort() {
			if (Open[0] > Bollinger1.Middle[0] 
				&& Close[0] < Bollinger1.Middle[0]
				&& MACD1.Diff[0] < 0){
					return true;
				}
			return false;
		}
		
		#endregion
	}
}
	
