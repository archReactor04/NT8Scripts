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
	public class AR_Blt_Crossovers : ArchReactorAlgoBase
	{
		private NinjaTrader.NinjaScript.Indicators.BltTriggerLines BltTriggerLines1;
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Description									= @"Blt Trigger Crossovers";
				Name										= "AR_Blt_Crossovers";
				StrategyName = "AR_Blt_Crossovers";
				
				#region BltTriggerLines
				TriggerMA = BltMAType.LinReg;
				TriggerMAPeriod = 80;
				AverageMA = BltMAType.EMA;
				AverageMAPeriod = 20;
				#endregion
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			base.OnBarUpdate();
		}
		
		protected override bool validateEntryLong() {
            if (CrossAbove(BltTriggerLines1.Trigger, BltTriggerLines1.Average, 1)) {
				return true;
			}
			return false;
        }
		
		protected override bool validateEntryShort() {
            if (CrossBelow(BltTriggerLines1.Trigger, BltTriggerLines1.Average, 1)) {
				return true;
			}
			return false;
        }
		
		protected override bool validateExitLong() {
			if (CrossBelow(BltTriggerLines1.Trigger, BltTriggerLines1.Average, 1)) {
				return true;
			}
			return false;
		}
		
		protected override bool validateExitShort() {
			if (CrossAbove(BltTriggerLines1.Trigger, BltTriggerLines1.Average, 1)) {
				return true;
			}
			return false;
		}
		
		#region Strategy Management
		protected override void initializeIndicators() {
           	BltTriggerLines1 = BltTriggerLines(TriggerMA, TriggerMAPeriod, AverageMA, AverageMAPeriod, ColorStyle.RegionColors, true, 5, Brushes.Cyan, Brushes.Teal, true, 40, Brushes.Green, Brushes.Red, false, null, Brushes.Green, Brushes.Red, Brushes.Green, Brushes.Red);
			AddChartIndicator(BltTriggerLines1);
		}
		
		#endregion
		
		#region Properties

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TriggerMA", GroupName = "1 Strategy Params - BltTrigger", Order = 1)]
		public BltMAType TriggerMA
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
        [Display(Name = "TriggerMAPeriod", Order = 2, GroupName = "1 Strategy Params - BltTrigger")]
		public int TriggerMAPeriod
        { 
			get; set;
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AverageMA", GroupName = "1 Strategy Params - BltTrigger", Order = 3)]
		public BltMAType AverageMA
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
        [Display(Name = "AverageMAPeriod", Order = 4, GroupName = "1 Strategy Params - BltTrigger")]
		public int AverageMAPeriod
        { 
			get; set;
		}
		
		#endregion
	}
}
