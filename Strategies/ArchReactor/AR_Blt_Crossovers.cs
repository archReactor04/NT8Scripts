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
		private NinjaTrader.NinjaScript.Indicators.BltTriggerLines Fast_BltTriggerLines;
		private NinjaTrader.NinjaScript.Indicators.BltTriggerLines Slow_BltTriggerLines;
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Description									= @"Blt Trigger Crossovers";
				Name										= "AR_Blt_Crossovers";
				StrategyName = "AR_Blt_Crossovers";
				StrategyVersion = "2.0";
				
				#region BltTriggerLines
				Fast_TriggerMA = BltMAType.DEMA;
				Fast_TriggerMAPeriod = 10;
				Fast_AverageMA = BltMAType.EMA;
				Fast_AverageMAPeriod = 35;
				
				Filter_SlowBlt = true;
				Slow_TriggerMA = BltMAType.EMA;
				Slow_TriggerMAPeriod = 20;
				Slow_AverageMA = BltMAType.EMA;
				Slow_AverageMAPeriod = 15;
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
			
			if (Filter_SlowBlt == true) {
	            if (CrossAbove(Slow_BltTriggerLines.Trigger, Slow_BltTriggerLines.Average, 1) && Fast_BltTriggerLines.Trigger[0] > Fast_BltTriggerLines.Average[0]) {
					return true;
				} 
			} else {
				if (CrossAbove(Fast_BltTriggerLines.Trigger, Fast_BltTriggerLines.Average, 1)) {
					return true;
				} 
			}
			return false;
        }
		
		protected override bool validateEntryShort() {
			if (Filter_SlowBlt == true) {
	            if (CrossBelow(Slow_BltTriggerLines.Trigger, Slow_BltTriggerLines.Average, 1)  && Fast_BltTriggerLines.Trigger[0] < Fast_BltTriggerLines.Average[0]) {
					return true;
				}
			} else {
				if (CrossBelow(Fast_BltTriggerLines.Trigger, Fast_BltTriggerLines.Average, 1)) {
					return true;
				}
			}
			return false;
        }
		
		protected override bool validateExitLong() {
			if (Filter_SlowBlt == false && CrossBelow(Fast_BltTriggerLines.Trigger, Fast_BltTriggerLines.Average, 1)) {
				return true;
			}
			return false;
		}
		
		protected override bool validateExitShort() {
			if (Filter_SlowBlt == false && CrossAbove(Fast_BltTriggerLines.Trigger, Fast_BltTriggerLines.Average, 1)) {
				return true;
			}
			return false;
		}
		
		
		#region Strategy Management
		protected override void initializeIndicators() {
           	Fast_BltTriggerLines = BltTriggerLines(Fast_TriggerMA, Fast_TriggerMAPeriod, Fast_AverageMA, Fast_AverageMAPeriod, ColorStyle.RegionColors, false, 5, Brushes.Cyan, Brushes.Teal, true, 40, Brushes.Green, Brushes.Red, false, null, Brushes.Green, Brushes.Red, Brushes.Green, Brushes.Red);
			AddChartIndicator(Fast_BltTriggerLines);
			
			if (Filter_SlowBlt == true) {
				Slow_BltTriggerLines = BltTriggerLines(Slow_TriggerMA, Slow_TriggerMAPeriod, Slow_AverageMA, Slow_AverageMAPeriod, ColorStyle.RegionColors, true, 5, Brushes.Blue, Brushes.DeepPink, true, 40, Brushes.Blue, Brushes.DeepPink, false, null, Brushes.Blue, Brushes.DeepPink, Brushes.Blue, Brushes.DeepPink);
				AddChartIndicator(Slow_BltTriggerLines);
			}
		}
		
		#endregion
		
		#region Properties

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast_TriggerMA", GroupName = "1 Strategy Params - Fast BltTrigger", Order = 1)]
		public BltMAType Fast_TriggerMA
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
        [Display(Name = "Fast_TriggerMAPeriod", Order = 2, GroupName = "1 Strategy Params - Fast BltTrigger")]
		public int Fast_TriggerMAPeriod
        { 
			get; set;
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast_AverageMA", GroupName = "1 Strategy Params - Fast BltTrigger", Order = 3)]
		public BltMAType Fast_AverageMA
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
        [Display(Name = "Fast_AverageMAPeriod", Order = 4, GroupName = "1 Strategy Params - Fast BltTrigger")]
		public int Fast_AverageMAPeriod
        { 
			get; set;
		}
		
		[NinjaScriptProperty]
        [Display(Name = "Filter_SlowBlt", Order = 1, GroupName = "1 Strategy Params - Slow BltTrigger")]
		public bool Filter_SlowBlt
        { get; set; }
		
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow_TriggerMA", GroupName = "1 Strategy Params - Slow BltTrigger", Order = 2)]
		public BltMAType Slow_TriggerMA
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
        [Display(Name = "Slow_TriggerMAPeriod", Order = 3, GroupName = "1 Strategy Params - Slow BltTrigger")]
		public int Slow_TriggerMAPeriod
        { 
			get; set;
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow_AverageMA", GroupName = "1 Strategy Params - Slow BltTrigger", Order = 4)]
		public BltMAType Slow_AverageMA
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
        [Display(Name = "Slow_AverageMAPeriod", Order = 5, GroupName = "1 Strategy Params - Slow BltTrigger")]
		public int Slow_AverageMAPeriod
        { 
			get; set;
		}
		
		
		#endregion
	}
}
