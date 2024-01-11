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
	public class AR_NQ_ES_EZ : ArchReactorAlgoBase
	{
		private LinReg LinReg1;
		private LinReg LinReg2;
		private NinjaTrader.NinjaScript.Indicators.RipsterEMAClouds RipsterEMAClouds1;
		private NinjaTrader.NinjaScript.Indicators.ADX ADX1;
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaADXVMA amaADXVMA1;
				private bool isProfitTargetHit = false;
		private double profitTargetPrice;
		private bool jumpToProfitSet = false;
		protected override void OnStateChange()
		{
			 base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = @"NQ ES EZ, from Arch Reactor Algo ( added my order management but the strategy logic is from elite futures discord";
                Name = "AR_NQ_ES_EZ";
				StrategyName = "AR_NQ_ES_EZ";

                #region Indicator Variable Initialization
					#region LinReg
				Use_Filter					= false;
				Filter_Period					= 70;
					#endregion
				
					#region ADX
				ADX_Filter = true;
				ADX_Period	= 5;
				ADX_Min		= 40;
						#endregion
				
				#endregion
            } else if (State == State.DataLoaded) {
				
			}
			
			
		}

		protected override void OnBarUpdate()
		{
			base.OnBarUpdate();
		}

		
		#region Strategy Management
		protected override void initializeIndicators() {
           		LinReg1				= LinReg(Close, Convert.ToInt32(Filter_Period));
				LinReg2				= LinReg(Close, Convert.ToInt32(Filter_Period));
				LinReg1.Plots[0].Brush = Brushes.SeaShell;
				AddChartIndicator(LinReg1);
			
			if (ADX_Filter == true) {
				ADX1 = ADX(ADX_Period);		
				AddChartIndicator(ADX1);
			}
		}
		
		private bool validateFiltersShort() {
			bool isADXValid = true;
			if (ADX_Filter == true) {
				if (ADX1[0] >= ADX_Min)
					isADXValid = true;
				else 
					isADXValid = false;
			}
			
			return isADXValid == true;
		}
		
		private bool validateFiltersLong() {
			bool isADXValid = true;
			if (ADX_Filter == true) {
				if (ADX1[0] >= ADX_Min)
					isADXValid = true;
				else 
					isADXValid = false;
			}
			
			return isADXValid == true;
		}
		
        protected override bool validateEntryLong() {
            if ((Open[0] > (Close[0] + (-31 * TickSize)) )
				 && (Close[2] < Open[0])
				 && (Close[1] > Open[1])
				 && (Close[0] > Open[0])
				 // Filter
				 && ((Use_Filter == false)
				 || (LinReg1[0] > LinReg2[1]))
				 && (GetCurrentBid(0) > LinReg2[0])
				&& validateFiltersLong() == true) {
						return true;
			}
			return false;
        }
		
		protected override bool validateEntryShort() {
            if ((Close[1] < (Close[0] + (31 * TickSize)) )
				 && (Close[2] > Open[0])
				 && (Close[1] < Open[1])
				 && (Close[0] < Open[0])
				 // Filter
				 && ((Use_Filter == false)
				 || (LinReg1[0] < LinReg2[1]))
				 && (GetCurrentBid(0) < LinReg2[0])
				&& validateFiltersShort() == true) {
						return true;
			}
			return false;
        }
		
		#endregion
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Use_Filter", Order=3, GroupName="1.1 Strategy Params - LinReg")]
		public bool Use_Filter
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Filter_Period", Order=4, GroupName="1.1 Strategy Params - LinReg")]
		public int Filter_Period
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "ADX_Filter", Order = 1, GroupName = "1.2 Strategy Params - ADX")]
		public bool ADX_Filter
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
        [Display(Name = "ADXVMA_Filter", Order = 1, GroupName = "1.3 Strategy Params - ADXVMA")]
		public bool ADXVMA_Filter
        { 
			get; set;
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADXVMA_Period", Order=2, GroupName="1.3 Strategy Params - ADXVMA")]
		public int ADXVMA_Period
		{ get; set; }
		
		#endregion
	}
}
