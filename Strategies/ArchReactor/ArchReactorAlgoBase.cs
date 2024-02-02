#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
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
namespace NinjaTrader.NinjaScript.Strategies.ArchReactor {
    abstract public class ArchReactorAlgoBase : Strategy, ICustomTypeDescriptor {

        #region orders
        protected const string entryLongString1 = "L1";
        protected const string entryLongString2 = "L2";
        protected const string entryShortString1 = "S1";
        protected const string entryShortString2 = "S2";
        protected const string stopLongString1 = "StopLong1";
        protected const string stopLongString2 = "StopLong2";
		protected const string stopShortString1 = "StopShort1";
	    protected const string stopShortString2 = "StopShort2";
        protected const string targetLongString1 = "TargetLong1";
        protected const string targetLongString2 = "TargetLong2";
		protected const string targetShortString1 = "TargetShort1";
	    protected const string targetShortString2 = "TargetShort2";
        #endregion
		
		#region ChartTrader variables		
		private System.Windows.Controls.RowDefinition	addedRow, addedRow2, addedRow3;
		private Gui.Chart.ChartTab						chartTab;
		private Gui.Chart.Chart							chartWindow;
		private System.Windows.Controls.Grid			chartTraderGrid, chartTraderButtonsGrid, lowerButtonsGrid, lowerButtonsGrid2, lowerButtonsGrid3;
		private System.Windows.Controls.Button			activateButton1, activateButton2, activateButton3;
		private bool									panelActive;
		private System.Windows.Controls.TabItem			tabItem;
		private System.Windows.Controls.Grid 			myGrid;
		
		private String Button1String;
		private String Button1Name, Button2Name;
		private Brush Button1BackColor;
		private Brush Button1ForeColor;
		
		private String Button2String;
		private Brush Button2BackColor;
		private Brush Button2ForeColor;
		
		private String Button3String;
		private Brush Button3BackColor;
		private Brush Button3ForeColor;
		#endregion
		
		#region Strategy variables
		private double TrailStopLong;
		private double TrailStopShort;

        private bool startTrail;
		protected string StrategyName;
		protected bool IsStratEnabled;
		private CommonEnums.OrderState orderState;
		protected CommonEnums.OrderType orderType;
		private CommonEnums.LimitType limitType;
		private bool showLimitTypeOptions;
		private bool isHistoricalTradeDisplayed;
		
		private double 	previousPrice		= 0;		// previous price used to calculate trailing stop
		private double 	newPrice			= 0;		// Default setting for new price used to calculate trailing stop
		private double	stopPlot			= 0;		// Value used to plot the stop level
		private double	initialBreakEven	= 0; 	
		private bool enableTrail;
		private bool showTrailOptions;
		private bool enableBreakeven;
		private bool showBreakevenOptions;
		private bool enableRunner;
		private bool showRunnerOptions;
		private bool showFixedStopLossOptions;
		private bool showATRStopLossOptions;
		private CommonEnums.StopLossType stopLossType;
		
		private CommonEnums.ProfitTargetType profitTargetType;
		private bool showFixedProfitTargetOptions;
		private bool showATRProfitTargetOptions;
		private double stopLossPriceLong;
		private double profitTargetPriceLong;
		private double stopLossPriceShort;
		private double profitTargetPriceShort;
		
		private CommonEnums.TrailStopType trailStopType;
		private bool showTickTrailOptions;
		private bool showATRTrailOptions;
		private bool showBarTrailOptions;
		
		private NinjaTrader.NinjaScript.Indicators.TradeSaber.ATRTrailBands StopLoss_ATR;
		private NinjaTrader.NinjaScript.Indicators.TradeSaber.ATRTrailBands ProfitTarget_ATR; 
		private NinjaTrader.NinjaScript.Indicators.TradeSaber.ATRTrailBands TrailStop_ATR; 
		private NinjaTrader.NinjaScript.Indicators.TradeSaber.ATRTrailBands Runner_ATR; 
		private double runnerProfitTargetPrice;

		#endregion
		
		private bool isProfitTargetHit = false;
		private double profitTargetPrice;
		private bool jumpToProfitSet = false;
		private double orderPriceLong = 0;
		private double orderPriceShort = 0;
		
		private Order entryOrder;
		private int Session1Count;
		private int Session2Count;
		private int Session3Count;
		private int Session4Count;
		private int SessionNumber;
		private bool isPnlAchieved;
		private bool ManuallyDisabled = false;

        protected override void OnStateChange() {
            switch (State) {
                case State.SetDefaults:
                    Description = @"ArchReactorAlgoBase";
                    Name = "ArchReactorAlgoBase";
					BaseAlgoVersion								= "1.6";
					StrategyVersion								= "1.0";
					Author										= "archReactor";
					Credits										= "archReactor";
					Disclaimer									= "Use this strategy at your own risk. Author take no responsibility of the losses incurred.";
                    Calculate									= Calculate.OnBarClose;
                    EntriesPerDirection							= 2;
                    EntryHandling								= EntryHandling.AllEntries;
                    IsExitOnSessionCloseStrategy				= true;
                    ExitOnSessionCloseSeconds					= 30;
                    IsFillLimitOnTouch							= false;
					MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
                    OrderFillResolution							= OrderFillResolution.Standard;
                    Slippage									= 0;
                    StartBehavior								= StartBehavior.WaitUntilFlat;
                    TimeInForce									= TimeInForce.Gtc;
                    TraceOrders									= false;
                    RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
                    StopTargetHandling							= StopTargetHandling.PerEntryExecution;
                    BarsRequiredToTrade							= 20;
                    // Disable this property for performance gains in Strategy Analyzer optimizations
                    // See the Help Guide for additional information
                    IsInstantiatedOnEachOptimizationIteration	= true;
					
					PositionSize					= 1;
					MaxTradesPerSession				= 4;
					InitialStopLong					= 30;
					InitialStopShort				= 30;
	                TrailStopLong				    = -1;
					TrailStopShort          		= 1;
					TrailProfitTrigger				= 20;
					TrailStepTicks					= 8;
	                ProfitTargetLong				= 40;
					ProfitTargetShort				= 40;
					BreakevenTicks					= 10;
					PlusBreakeven					= 2;
	                startTrail                      = false;
					StrategyName 					= "ArchReactor Strategy";
					TrailByBars						= 1;
					orderState = CommonEnums.OrderState.BOTH;
					IsStratEnabled = true;
					EnableTrail = true;
					EnableBreakeven = true;
					enableTrail = true;
					showTrailOptions = true;
					enableBreakeven = true;
					showBreakevenOptions = true;
					orderType	= CommonEnums.OrderType.MARKET;
					limitType = CommonEnums.LimitType.CLOSE;
					showLimitTypeOptions = false;
					LimitOffset = 0;
					isHistoricalTradeDisplayed = false;
					enableRunner = false;
					showRunnerOptions = false;
					RunnerPositionSize = 1;
					DisplayStrategyPnLOrientation = TextPosition.BottomLeft;
					DisplayHistoricalTradePerformanceOrientation = TextPosition.BottomRight;
					JumpToProfit = true;
					JumpToProfitTickOffset = 4;
					
					trailStopType = CommonEnums.TrailStopType.TickTrail;
					showTickTrailOptions = false;
					showATRTrailOptions = false;
					showBarTrailOptions	= false;
					
					TrailStop_ATR_Period = 14;
					TrailStop_ATR_Mult = 2;
					Runner_Mult = 4;
	
	                Time_1	= true;
                    Time_2 	= false;
                    Time_3 	= false;
                    Time_4	= false;
                    
                    Start_Time_1 = DateTime.Parse("06:30", System.Globalization.CultureInfo.InvariantCulture);
                    Stop_Time_1 = DateTime.Parse("13:00", System.Globalization.CultureInfo.InvariantCulture);
                    Start_Time_2 = DateTime.Parse("1:15", System.Globalization.CultureInfo.InvariantCulture);
                    Stop_Time_2 = DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);
                    Start_Time_3 = DateTime.Parse("09:45", System.Globalization.CultureInfo.InvariantCulture);
                    Stop_Time_3 = DateTime.Parse("11:00", System.Globalization.CultureInfo.InvariantCulture);
                    Start_Time_4 = DateTime.Parse("13:00", System.Globalization.CultureInfo.InvariantCulture);
                    Stop_Time_4 = DateTime.Parse("14:00", System.Globalization.CultureInfo.InvariantCulture);
					
					MaxTarget = 200;
					MaxLoss = -200;
					DisplayStrategyPnL = true;
					DisplayHistoricalTradePerformance = true;
					stopLossType = CommonEnums.StopLossType.Fixed;
					StopLoss_ATR_Period = 14;
					StopLoss_ATR_Mult	= 2;
					showFixedStopLossOptions = false;
					showATRStopLossOptions = false;
					
					profitTargetType = CommonEnums.ProfitTargetType.Fixed;
					ProfitTarget_ATR_Period = 14;
					ProfitTarget_ATR_Mult	= 2;
					showFixedProfitTargetOptions = false;
					showATRProfitTargetOptions = false;
					
					stopLossPriceLong = InitialStopLong;
					stopLossPriceShort = InitialStopShort;
					profitTargetPriceLong = ProfitTargetLong;
					profitTargetPriceShort = ProfitTargetShort;
					
					Session1Count = 0;
					Session2Count = 0;
					Session3Count = 0;
					Session4Count = 0;
					SessionNumber = 0;
					isPnlAchieved = false;
					
					#region ChartTrader Button variables
				
					//Row 1
					Button1String								= "Strategy Enabled";
					Button1Name									= "StrategyButtonEnabled";
					Button1BackColor							= Brushes.Aquamarine;
					Button1ForeColor							= Brushes.Black;
					
					//Row 2
					Button2String								= "Longs & Shorts";
					Button2Name									= "StrategyButtonBoth";
					Button2BackColor							= Brushes.Yellow;
					Button2ForeColor							= Brushes.Black;
	
					Button3String								= "Flatten";
					Button3BackColor							= Brushes.Plum;
					Button3ForeColor							= Brushes.Black;
					#endregion
                    break;
                case State.Configure:
					initializeIndicators();
					
					if (stopLossType == CommonEnums.StopLossType.ATR) {
						StopLoss_ATR = ATRTrailBands(StopLoss_ATR_Period, StopLoss_ATR_Mult);
					}
					
					if (profitTargetType == CommonEnums.ProfitTargetType.ATR) {
						ProfitTarget_ATR = ATRTrailBands(ProfitTarget_ATR_Period, ProfitTarget_ATR_Mult);
						Runner_ATR = ATRTrailBands(ProfitTarget_ATR_Period, Runner_Mult);
					}
					
					if (trailStopType == CommonEnums.TrailStopType.ATRTrail) {
						TrailStop_ATR = ATRTrailBands(TrailStop_ATR_Period, TrailStop_ATR_Mult);
					}
					
                    break;
                case State.DataLoaded:
                    break;
                case State.Historical:
                    #region Chart Trader Buttons Load
				
					if (ChartControl != null)
					{	
						if (UserControlCollection.Contains(myGrid))
								return;
							
						ChartControl.Dispatcher.InvokeAsync(() =>
						{
							CreateWPFControls();
						});
					}
				
                    break;
					#endregion
                case State.Terminated:
                    #region Chart Trader Terminate
				
					if (ChartControl != null)
					{
						ChartControl.Dispatcher.InvokeAsync(() =>
						{
							DisposeWPFControls();
						});
					}
				
                    break;     
					#endregion
						
            }
        }
		
		public override string DisplayName
		{
        	get { return StrategyName; }
		}

 		#region ChartTrader UI
			#region Button Click Events
		
			#region Button 1
		
		protected void Button1Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			if (button == activateButton1 && button.Name == "StrategyButtonEnabled" && button.Content == "Strategy Enabled")
			{
				button.Content = "Strategy Disabled";
				button.Name = "StrategyButtonDisabled";
				button.Background = Brushes.Gray;
				button.BorderBrush = Brushes.Black;
				IsStratEnabled = false;
				ManuallyDisabled = true;
				return;
			}
			
			if (button == activateButton1 && button.Name == "StrategyButtonDisabled" && button.Content == "Strategy Disabled")
			{
				button.Content = "Strategy Enabled";
				button.Name = "StrategyButtonEnabled";
				button.Background = Brushes.Aquamarine;
				button.BorderBrush = Brushes.Black;
				IsStratEnabled = true;
				ManuallyDisabled = false;
				return;
			}
			//Draw.TextFixed(this, "infobox", "Button 1 Clicked", TextPosition.BottomLeft, Brushes.Green, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
			ForceRefresh();
			
		}
		
			#endregion
		
			#region Button 2
		
		protected void Button2Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			if (button == activateButton2 && button.Name == "StrategyButtonBoth" && button.Content == "Longs & Shorts")
			{
				button.Content = "Longs Only";
				button.Name = "StrategyButtonLongs";
				button.Background = Brushes.Lime;
				button.BorderBrush = Brushes.Black;
				orderState = CommonEnums.OrderState.LONGS;
				return;
			}
			
			if (button == activateButton2 && button.Name == "StrategyButtonLongs" && button.Content == "Longs Only")
			{
				button.Content = "Shorts Only";
				button.Name = "StrategyButtonShorts";
				button.Background = Brushes.Salmon;
				button.BorderBrush = Brushes.Black;
				orderState = CommonEnums.OrderState.SHORTS;
				return;
			}
			
			if (button == activateButton2 && button.Name == "StrategyButtonShorts" && button.Content == "Shorts Only")
			{
				button.Content = "Longs & Shorts";
				button.Name = "StrategyButtonBoth";
				button.Background = Brushes.Yellow;
				button.BorderBrush = Brushes.Black;
				orderState = CommonEnums.OrderState.BOTH;
				return;
			}
			//Draw.TextFixed(this, "infobox", "Button 2 Clicked", TextPosition.BottomLeft, Brushes.Red, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
			ForceRefresh();
		}		
		
			#endregion
		
			#region Button 3
		
		protected void Button3Click(object sender, RoutedEventArgs e)
		{
			if(Position.MarketPosition == MarketPosition.Long) {
				ExitLong("Manual Exit "+entryLongString1, entryLongString1);
				if (enableRunner == true) {
					ExitLong("Manual Exit "+entryLongString2, entryLongString2);
				}
			} else if(Position.MarketPosition == MarketPosition.Short) {
				ExitShort("Manual Exit "+entryShortString1, entryShortString1);
				if (enableRunner == true) {
					ExitShort("Manual Exit "+entryShortString2, entryShortString2);
				}
			}
		}		
		
			#endregion
		#endregion
		
		protected void CreateWPFControls()
		{
			
				#region Button Grid
			
			
			
			chartWindow				= Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;
			
			// if not added to a chart, do nothing
			if (chartWindow == null)
				return;
			

			chartTraderGrid			= (chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader).Content as System.Windows.Controls.Grid;

			// this grid contains the existing chart trader buttons
			chartTraderButtonsGrid	= chartTraderGrid.Children[0] as System.Windows.Controls.Grid;
			

			// Lower Grid - (Row1)Upper
			lowerButtonsGrid = new System.Windows.Controls.Grid();
			System.Windows.Controls.Grid.SetColumnSpan(lowerButtonsGrid, 1);
	
			//Columns * 1
			lowerButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());		
			
			
		
			// Lower Grid - (Row2)Middle
			lowerButtonsGrid2 = new System.Windows.Controls.Grid();
			System.Windows.Controls.Grid.SetColumnSpan(lowerButtonsGrid2, 1);
			
			//Columns * 2
			lowerButtonsGrid2.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			
			
			
			// Lower Grid - (Row3)Lower
			lowerButtonsGrid3 = new System.Windows.Controls.Grid();
			System.Windows.Controls.Grid.SetColumnSpan(lowerButtonsGrid3, 1);
			
			//Columns * 3
			lowerButtonsGrid3.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			
			
			
			addedRow	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(40) };
			addedRow2	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(40) };
			addedRow3	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(40) };
				

			// this style (provided by NinjaTrader_MichaelM) gives the correct default minwidth (and colors) to make buttons appear like chart trader buttons
			Style basicButtonStyle	= Application.Current.FindResource("BasicEntryButton") as Style;
	
			
			#endregion
			
			
				#region Button Content
				
				
					activateButton1 = new System.Windows.Controls.Button()//1
					{		
						
						Content			= Button1String,
						Name			= Button1Name,
						Height			= 25, 
						Margin			= new Thickness(5,0,5,0),
						Padding			= new Thickness(0,0,0,0),
						Style			= basicButtonStyle
					};		
				
				
			
					activateButton2 = new System.Windows.Controls.Button()//2
					{		
						
						Content			= Button2String,
						Name			= Button2Name,
						Height			= 25, 
						Margin			= new Thickness(5,0,5,0),
						Padding			= new Thickness(0,0,0,0),
						Style			= basicButtonStyle
					};		
			
				
					activateButton3 = new System.Windows.Controls.Button()//3
					{		
						
						Content			= Button3String,
						Height			= 25, 
						Margin			= new Thickness(5,0,5,0),
						Padding			= new Thickness(0,0,0,0),
						Style			= basicButtonStyle
					};		
			
				#endregion
					
	
				#region Button Colors
					
					//Row1
					activateButton1.Background		= Button1BackColor;
					activateButton1.BorderBrush		= Brushes.Black;	
					activateButton1.Foreground    	= Button1ForeColor;	
					activateButton1.BorderThickness = new Thickness(2.0);

					//Row2
					activateButton2.Background		= Button2BackColor;
					activateButton2.BorderBrush		= Brushes.Yellow;	
					activateButton2.Foreground    	= Button2ForeColor;	
					activateButton2.BorderThickness = new Thickness(2.0);

					activateButton3.Background		= Button3BackColor;
					activateButton3.BorderBrush		= Brushes.Black;	
					activateButton3.Foreground    	= Button3ForeColor;		
					activateButton3.BorderThickness = new Thickness(2.0);
			#endregion	
					
		
				#region Button Click 
				
					activateButton1.Click += Button1Click;
					activateButton2.Click += Button2Click;
					activateButton3.Click += Button3Click;
				
				#endregion	
					
					
				#region Button Location
		
					//activateButton1 (Row 1)
					System.Windows.Controls.Grid.SetColumn(activateButton1, 0);				
					System.Windows.Controls.Grid.SetRow(activateButton1, 0);	
				
					
					//New Grid - Start at Row 0. But we have 2 columns here (Row 2)
					System.Windows.Controls.Grid.SetColumn(activateButton2, 0);				
					System.Windows.Controls.Grid.SetRow(activateButton2, 0);
					
					System.Windows.Controls.Grid.SetColumn(activateButton3, 0);				
					System.Windows.Controls.Grid.SetRow(activateButton3, 0);
				#endregion	
					
					
				
				#region Add Buttons 1
			
					lowerButtonsGrid.Children.Add(activateButton1);
					
				
				#endregion
					
				#region Add Buttons 2-3
					
					lowerButtonsGrid2.Children.Add(activateButton2);
						
				#endregion	
					
				#region Add Buttons 4-6
					
					lowerButtonsGrid3.Children.Add(activateButton3);
						
				#endregion		
					
            if (totalGrids == 0) 
				totalGrids = chartTraderGrid.RowDefinitions.Count;


			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
			
		}
		
		static int totalGrids;

        public void DisposeWPFControls() 
		{
			#region Dispose
			
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
			
			//Row 1
			if (activateButton1 != null)
				activateButton1.Click -= Button1Click;
			
			
			//Row 2
			if (activateButton2 != null)
				activateButton2.Click -= Button2Click;
			
			if (activateButton3 != null)
				activateButton3.Click -= Button3Click;
			
			RemoveWPFControls();
			
			#endregion
		}
		
		public void InsertWPFControls()
		{
			#region Insert WPF
			
			if (panelActive)
				return;
			
			
			// add a new row (addedRow) for our lowerButtonsGrid below the ask and bid prices and pnl display			
			chartTraderGrid.RowDefinitions.Add(addedRow);
			System.Windows.Controls.Grid.SetRow(lowerButtonsGrid, totalGrids); 
			chartTraderGrid.Children.Add(lowerButtonsGrid);
			
			
			chartTraderGrid.RowDefinitions.Add(addedRow2);
			System.Windows.Controls.Grid.SetRow(lowerButtonsGrid2, totalGrids + 1); //Add 1 Grid
			chartTraderGrid.Children.Add(lowerButtonsGrid2);
			
			
			chartTraderGrid.RowDefinitions.Add(addedRow3);
			System.Windows.Controls.Grid.SetRow(lowerButtonsGrid3, totalGrids + 2); //Add 2 Grids 
			chartTraderGrid.Children.Add(lowerButtonsGrid3);
			
			
			panelActive = true;
			
			#endregion	
		}
		
		
		protected void RemoveWPFControls()
		{
			#region Remove WPF
			
			if (!panelActive)
				return;

			if (chartTraderButtonsGrid != null || (lowerButtonsGrid != null && lowerButtonsGrid2 != null && lowerButtonsGrid3 != null))
			{
				chartTraderGrid.Children.Remove(lowerButtonsGrid);
				chartTraderGrid.Children.Remove(lowerButtonsGrid2);
				chartTraderGrid.Children.Remove(lowerButtonsGrid3);
				
				chartTraderGrid.RowDefinitions.Remove(addedRow);
				chartTraderGrid.RowDefinitions.Remove(addedRow2);
				chartTraderGrid.RowDefinitions.Remove(addedRow3);
			}
			
			panelActive = false;
			
			#endregion
		}
		
		
		private bool TabSelected()
		{
			#region TabSelected 
			
			if (ChartControl == null || chartWindow == null || chartWindow.MainTabControl == null)
				return false;
			
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as Gui.Chart.ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
				
			#endregion
		}
		
		
		
		private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{	
			#region TabHandler
			
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as Gui.Chart.ChartTab;
			if (chartTab == null)
				return;

			if (TabSelected())
				InsertWPFControls();
			else
				RemoveWPFControls();
			
			#endregion
		}
		#endregion
		
		#region Methods
	
        protected abstract bool validateEntryLong();

        protected abstract bool validateEntryShort();

        protected virtual bool validateExitLong() {
			return false;
		}

        protected virtual bool validateExitShort() {
			return false;
		}
		
		protected abstract void initializeIndicators();
		
		
		protected virtual void addDataSeries() {}
		
		protected virtual bool isCustomStopSet() {
			return false;
		}
		
		protected virtual bool isCustomProfitSet() {
			return false;
		}
		
		protected virtual double customStopLong() {
			return -1;
		}
		
		protected virtual double customStopShort() {
			return -1;
		}
		
		protected virtual double customProfitTargetLong(double price) {
			return -1;
		}
		
		protected virtual double customProfitTargetShort(double price) {
			return -1;
		}
		
		protected bool validateTimeControlsAndTradeCount() {
			
			if (Time_1 == true 
				&& Times[0][0].TimeOfDay >= Start_Time_1.TimeOfDay
                 && Times[0][0].TimeOfDay <= Stop_Time_1.TimeOfDay
				&& Session1Count < MaxTradesPerSession) {
					SessionNumber = 1;
					return true;
					 
			}
			if (Time_2 == true 
				&& Times[0][0].TimeOfDay >= Start_Time_2.TimeOfDay
                 && Times[0][0].TimeOfDay <= Stop_Time_2.TimeOfDay
				&& Session2Count < MaxTradesPerSession) {
					SessionNumber = 2;
					 return true;
			}
			if (Time_3 == true 
				&& Times[0][0].TimeOfDay >= Start_Time_3.TimeOfDay
                 && Times[0][0].TimeOfDay <= Stop_Time_3.TimeOfDay
				&& Session3Count < MaxTradesPerSession) {
					SessionNumber = 3;
					 return true;
			}
			if (Time_4 == true 
				&& Times[0][0].TimeOfDay >= Start_Time_4.TimeOfDay
                 && Times[0][0].TimeOfDay <= Stop_Time_4.TimeOfDay
				&& Session4Count < MaxTradesPerSession) {
					SessionNumber = 4;
					 return true;
			}
		    return false;
		}
		
		protected void incrementSessionTradeCount() {
			if (State == State.Realtime) {
				if (Time_1 == true 
					&& Times[0][0].TimeOfDay >= Start_Time_1.TimeOfDay
	                 && Times[0][0].TimeOfDay <= Stop_Time_1.TimeOfDay) {
						 Session1Count++;
				}
				if (Time_2 == true 
					&& Times[0][0].TimeOfDay >= Start_Time_2.TimeOfDay
	                 && Times[0][0].TimeOfDay <= Stop_Time_2.TimeOfDay) {
						 Session2Count++;
				}
				if (Time_3 == true 
					&& Times[0][0].TimeOfDay >= Start_Time_3.TimeOfDay
	                 && Times[0][0].TimeOfDay <= Stop_Time_3.TimeOfDay) {
						 Session3Count++;
				}
				if (Time_4 == true 
					&& Times[0][0].TimeOfDay >= Start_Time_4.TimeOfDay
	                 && Times[0][0].TimeOfDay <= Stop_Time_4.TimeOfDay) {
						 Session4Count++;
				}
			}
		}
		
		protected void resetSessionTradeCount() {
			
			if (Time_1 == true 
                 && Times[0][0].TimeOfDay > Stop_Time_1.TimeOfDay) {
					 Session1Count = 0;
			}
			if (Time_2 == true 
                 && Times[0][0].TimeOfDay > Stop_Time_2.TimeOfDay) {
					 Session2Count = 0;
			}
			if (Time_3 == true 
                 && Times[0][0].TimeOfDay > Stop_Time_3.TimeOfDay) {
					 Session3Count = 0;
			}
			if (Time_4 == true 
                 && Times[0][0].TimeOfDay > Stop_Time_4.TimeOfDay) {
					 Session4Count = 0;
			}
		}
		
		private void validateStrategyPnl() {
			if (State == State.Realtime && validateStrategytPnlConditions()) {
					enableDisableStrat(false);
				}
		}
		
		private void validateMaxTradesPerSession() {
			if (State == State.Realtime) {
				if (isPnlAchieved == false && ManuallyDisabled == false) {
					if (Time_1 == true 
						&& Times[0][0].TimeOfDay >= Start_Time_1.TimeOfDay
		                 && Times[0][0].TimeOfDay <= Stop_Time_1.TimeOfDay
						&& Session1Count < MaxTradesPerSession) {		
							enableDisableStrat(true);
					}
					else if (Time_2 == true 
						&& Times[0][0].TimeOfDay >= Start_Time_2.TimeOfDay
		                 && Times[0][0].TimeOfDay <= Stop_Time_2.TimeOfDay
						&& Session2Count < MaxTradesPerSession) {
							enableDisableStrat(true);
					} 
					else if (Time_3 == true 
						&& Times[0][0].TimeOfDay >= Start_Time_3.TimeOfDay
		                 && Times[0][0].TimeOfDay <= Stop_Time_3.TimeOfDay
						&& Session3Count < MaxTradesPerSession) {
							 enableDisableStrat(true);
					}
					else if (Time_4 == true 
						&& Times[0][0].TimeOfDay >= Start_Time_4.TimeOfDay
		                 && Times[0][0].TimeOfDay <= Stop_Time_4.TimeOfDay
						&& Session4Count < MaxTradesPerSession) {
							enableDisableStrat(true);
					} else {
						enableDisableStrat(false);
					}
				}
			}
		}
		
		private void enableDisableStrat(bool isEnabled) {
			if (State == State.Realtime) {
				if (isEnabled == false) {
					ChartControl.Dispatcher.InvokeAsync(() => {
								activateButton1.Content = "Strategy Disabled";
								activateButton1.Name = "StrategyButtonDisabled";
								activateButton1.Background = Brushes.Gray;
								activateButton1.BorderBrush = Brushes.Black;
							});
				} else {
					ChartControl.Dispatcher.InvokeAsync(() => {
								activateButton1.Content = "Strategy Enabled";
								activateButton1.Name = "StrategyButtonEnabled";
								activateButton1.Background = Brushes.Aquamarine;
								activateButton1.BorderBrush = Brushes.Black;
					});
				}
				IsStratEnabled = isEnabled;
			}
		}
		
		private bool validateStrategytPnlConditions() {
			double cumProfit = getCumProfit();
			if (cumProfit <= MaxLoss || cumProfit >= MaxTarget) {
				IsStratEnabled = false;
				isPnlAchieved = true;
				return true;
			}
			return false;
		}
		#endregion
		
		#region StopLoss And ProfitTargetCalculation
		protected virtual void calculateStopLossPriceLong(double price, bool isRunner) {
			if (isCustomStopSet() == false) {
				if (stopLossType == CommonEnums.StopLossType.ATR) {
					//SetStopLoss(entryLongString1, CalculationMode.Price, StopLoss_ATR.TrailingStopLow[0], false);
					Print("Setting ATR StopLoss Long "+StopLoss_ATR.TrailingStopLow[0]);
					if (isRunner == false)
						ExitLongStopMarket(0, true, PositionSize, StopLoss_ATR.TrailingStopLow[0], "Stop " + entryLongString1, entryLongString1);
					else {
						//SetStopLoss(entryLongString2, CalculationMode.Price, StopLoss_ATR.TrailingStopLow[0], false);
						ExitLongStopMarket(0, true, RunnerPositionSize, StopLoss_ATR.TrailingStopLow[0], "Stop " + entryLongString2, entryLongString2);
					//	Print("Put StopLoss 2 ATR Long");
					} 
				} else if (stopLossType == CommonEnums.StopLossType.Fixed){
					Print("Setting Fixed StopLoss Long "+(price - InitialStopLong*TickSize));
					//SetStopLoss(entryLongString1, CalculationMode.Ticks, InitialStopLong, false);
					if (isRunner == false) 
						ExitLongStopMarket(0, true, PositionSize, (price - InitialStopLong*TickSize), "Stop " + entryLongString1, entryLongString1);
					//Print("Put StopLoss 1 Fixed Long");
					else {
					//	SetStopLoss(entryLongString2, CalculationMode.Ticks, InitialStopLong, false);
						ExitLongStopMarket(0, true, RunnerPositionSize, (price - InitialStopLong*TickSize), "Stop " + entryLongString2, entryLongString2);
					//	Print("Put StopLoss 2 Fixed Long");
					}
				}
			} else {
				double stopLossLong = customStopLong();
				Print ("Setting Custom Stop Loss Long");
				
				if (isRunner == false)
						ExitLongStopMarket(0, true, PositionSize, stopLossLong, "Stop " + entryLongString1, entryLongString1);
					else {
						//SetStopLoss(entryLongString2, CalculationMode.Price, StopLoss_ATR.TrailingStopLow[0], false);
						ExitLongStopMarket(0, true, RunnerPositionSize, stopLossLong, "Stop " + entryLongString2, entryLongString2);
					//	Print("Put StopLoss 2 ATR Long");
					} 
				
			}
		}
		
		protected virtual void calculateStopLossPriceShort(double price, bool isRunner) {
			if (isCustomStopSet() == false) {
				if (stopLossType == CommonEnums.StopLossType.ATR) {
					Print("Setting ATR StopLoss Short "+StopLoss_ATR.TrailingStopHigh[0]);
					//SetStopLoss(entryShortString1, CalculationMode.Price, StopLoss_ATR.TrailingStopHigh[0], false);
					if (isRunner == false)
						ExitShortStopMarket(0, true, PositionSize, StopLoss_ATR.TrailingStopHigh[0], "Stop " + entryShortString1, entryShortString1);
				//	Print("Put StopLoss 1 ATR Short");
					
					else {
						//SetStopLoss(entryShortString2, CalculationMode.Price, StopLoss_ATR.TrailingStopHigh[0], false);
						ExitShortStopMarket(0, true, RunnerPositionSize, StopLoss_ATR.TrailingStopHigh[0], "Stop " + entryShortString2, entryShortString2);
					//	Print("Put StopLoss 2 ATR Short");
					}
				} else {
					Print("Setting Fixed StopLoss Short "+(price + InitialStopLong*TickSize));
					//SetStopLoss(entryShortString1, CalculationMode.Ticks, InitialStopShort, false);
					if (isRunner == false)
						ExitShortStopMarket(0, true, PositionSize, (price + InitialStopShort*TickSize), "Stop " + entryShortString1, entryShortString1);
				//	Print("Put StopLoss 1 Fixed Short");
					else {
						//SetStopLoss(entryShortString2, CalculationMode.Ticks, InitialStopShort, false);
						ExitShortStopMarket(0, true, RunnerPositionSize, (price + InitialStopShort*TickSize), "Stop " + entryShortString2, entryShortString2);
					//	Print("Put StopLoss 2 Fixed Short");
					}
				}
			} else {
				double stopLossShort = customStopShort();
				Print ("Setting Custom Stop Loss Short");
				
				if (isRunner == false)
						ExitShortStopMarket(0, true, PositionSize, stopLossShort, "Stop " + entryShortString1, entryShortString1);
					else {
						//SetStopLoss(entryLongString2, CalculationMode.Price, StopLoss_ATR.TrailingStopLow[0], false);
						ExitShortStopMarket(0, true, RunnerPositionSize, stopLossShort, "Stop " + entryShortString2, entryShortString2);
					//	Print("Put StopLoss 2 ATR Long");
					} 
				
			}
			
		}
		
		protected virtual void calculateProfitTargetPriceLong(double price, bool isRunner) {
			if (isCustomProfitSet() == false) {
				if (profitTargetType == CommonEnums.ProfitTargetType.ATR) {
					Print("Setting ATR Profit Target Long "+ ProfitTarget_ATR.TrailingStopHigh[0]);
					//SetProfitTarget(entryLongString1, CalculationMode.Price, ProfitTarget_ATR.TrailingStopHigh[0], false);
					
					if (isRunner == false) 
						ExitLongLimit(0, true, PositionSize, ProfitTarget_ATR.TrailingStopHigh[0], "Profit Target "+entryLongString1, entryLongString1);
					//Print("Put Profit Target 1 ATR Long");
					
					else {
						//SetProfitTarget(entryLongString2, CalculationMode.Price, Runner_ATR.TrailingStopHigh[0], false);
						ExitLongLimit(0, true, RunnerPositionSize, Runner_ATR.TrailingStopHigh[0], "Profit Target "+entryLongString2, entryLongString2);
						
					//	Print("Put Profit Target 2 ATR Long");
					}
				} else {
			//	if (profitTargetType == CommonEnums.ProfitTargetType.Fixed) {
					Print("Setting Fixed Profit Target Long "+ (price + ProfitTargetLong*TickSize));
					//SetProfitTarget(entryLongString1, CalculationMode.Ticks, ProfitTargetLong, false);
					if (isRunner == false)
						ExitLongLimit(0, true, PositionSize, price + ProfitTargetLong*TickSize, "Profit Target "+entryLongString1, entryLongString1);
					//Print("Put Profit Target 1 Fixed Long");
					else {
						//SetProfitTarget(entryLongString2, CalculationMode.Ticks, ProfitTargetLong * Runner_Mult, false);
						ExitLongLimit(0, true, RunnerPositionSize, price+ (ProfitTargetLong * Runner_Mult)*TickSize, "Profit Target "+entryLongString2, entryLongString2);
						//Print("Put Profit Target 1 Fixed Long");
					}
				}
			} else {
					
				double profitTargetLong = customProfitTargetLong(price);
				double profitTargetInTicks = (profitTargetLong - price) / TickSize;
				Print("Setting Custom Profit Target Long "+ (profitTargetLong));
				if (isRunner == false)
						ExitLongLimit(0, true, PositionSize, profitTargetLong, "Profit Target "+entryLongString1, entryLongString1);
					//Print("Put Profit Target 1 Fixed Long");
					else {
						//SetProfitTarget(entryLongString2, CalculationMode.Ticks, ProfitTargetLong * Runner_Mult, false);
						ExitLongLimit(0, true, RunnerPositionSize, price + (profitTargetInTicks * Runner_Mult)*TickSize, "Profit Target "+entryLongString2, entryLongString2);
						//Print("Put Profit Target 1 Fixed Long");
					}
				
			}
		}
		
		protected virtual void calculateProfitTargetPriceShort(double price, bool isRunner) {
			if (isCustomProfitSet() == false) { 
				if (profitTargetType == CommonEnums.ProfitTargetType.ATR) {
					 Print("Setting ATR Profit Target Short "+ ProfitTarget_ATR.TrailingStopLow[0]);
				//	SetProfitTarget(entryShortString1, CalculationMode.Price,ProfitTarget_ATR.TrailingStopLow[0], false);
					if (isRunner == false)
					 	ExitShortLimit(0, true, PositionSize, ProfitTarget_ATR.TrailingStopLow[0], "Profit Target "+entryShortString1, entryShortString1);
					//Print("Put Profit Target 1 ATR Short");
					else {
						//SetProfitTarget(entryShortString2, CalculationMode.Price, Runner_ATR.TrailingStopLow[0], false);
						ExitShortLimit(0 , true, RunnerPositionSize, Runner_ATR.TrailingStopLow[0], "Profit Target "+entryShortString2, entryShortString2);
						//Print("Put Profit Target 2 ATR Short");
					}
				} else {
					Print("Setting Fixed Profit Target Short "+ (price - ProfitTargetLong*TickSize));
					//SetProfitTarget(entryShortString1, CalculationMode.Ticks, ProfitTargetShort, false);
					if (isRunner == false)
						ExitShortLimit(0, true, PositionSize, price - ProfitTargetShort*TickSize, "Profit Target "+entryShortString1, entryShortString1);
					//Print("Put Profit Target 1 Fixed Short");
					
					else {
						//SetProfitTarget(entryShortString2, CalculationMode.Ticks, ProfitTargetShort * Runner_Mult, false);
						ExitShortLimit(0, true, RunnerPositionSize, price - (ProfitTargetShort * Runner_Mult)*TickSize, "Profit Target "+entryShortString2, entryShortString2);
						//Print("Put Profit Target 2 Fixed Short "+Position.AveragePrice - (ProfitTargetShort * Runner_Mult)*TickSize);
					}
				}
			} else {
					
				double profitTargetShort = customProfitTargetShort(price);
				double profitTargetInTicks = (price - profitTargetShort) / TickSize;
				Print("Setting Custom Profit Target Short "+ (profitTargetShort));
				if (isRunner == false)
						ExitShortLimit(0, true, PositionSize, profitTargetShort, "Profit Target "+entryShortString1, entryShortString1);
					//Print("Put Profit Target 1 Fixed Long");
					else {
						//SetProfitTarget(entryLongString2, CalculationMode.Ticks, ProfitTargetLong * Runner_Mult, false);
						ExitShortLimit(0, true, RunnerPositionSize, price - (profitTargetInTicks * Runner_Mult)*TickSize, "Profit Target "+entryShortString2, entryShortString2);
						//Print("Put Profit Target 1 Fixed Long");
					}
				
			}
			
		}
		#endregion
		
		#region Trail stop and breakeven calculation
		protected virtual void calculateTrailStopAndBE() {
			switch (Position.MarketPosition)
            {
				// Resets the stop loss to the original value when all positions are closed
                case MarketPosition.Flat:
					/*calculateStopLossPriceLong();
					calculateProfitTargetPriceLong();
					calculateStopLossPriceShort();
					calculateProfitTargetPriceShort();*/
					previousPrice = 0;
					stopPlot = 0;
                    break;
				
					   
                case MarketPosition.Long:
						
					if (previousPrice == 0)
					{
						stopPlot = Position.AveragePrice - InitialStopLong * TickSize;  // initial stop plot level
					}
					
					if (EnableBreakeven == true) {
	                    // Once the price is greater than entry price+ breakEvenTicks ticks, set stop loss to plusBreakeven ticks
	                    if (Close[0] > Position.AveragePrice + BreakevenTicks * TickSize  && previousPrice == 0)
	                    {
							initialBreakEven = Position.AveragePrice + PlusBreakeven * TickSize;
	                       // SetStopLoss(entryLongString1, CalculationMode.Price, initialBreakEven, false);
							ExitLongStopMarket(0, true, PositionSize, initialBreakEven, "Stop " + entryLongString1, entryLongString1);
							if (enableRunner == true) {
								//SetStopLoss(entryLongString2, CalculationMode.Price, initialBreakEven, false);
								ExitLongStopMarket(0, true, RunnerPositionSize, initialBreakEven, "Stop " + entryLongString2, entryLongString2);
							}
							
							previousPrice = Position.AveragePrice;
							stopPlot = initialBreakEven;
	                    } else if (previousPrice	!= 0 ////StopLoss is at breakeven
 							&& GetCurrentAsk() > previousPrice + TrailProfitTrigger * TickSize && EnableTrail == true )
						{
							if (trailStopType == CommonEnums.TrailStopType.TickTrail) {
								newPrice = previousPrice + TrailStepTicks * TickSize; // Calculate trail stop adjustment
							} else if (trailStopType == CommonEnums.TrailStopType.ATRTrail) {
								//newPrice =  (previousPrice < TrailStop_ATR.TrailingStopLow[0]) ? TrailStop_ATR.TrailingStopLow[0] : previousPrice;
								newPrice =  TrailStop_ATR.TrailingStopLow[0];
							} else if (trailStopType == CommonEnums.TrailStopType.BarTrail && newPrice < Low[TrailByBars]) {
								newPrice = Low[TrailByBars];
							}
							//SetStopLoss(entryLongString1, CalculationMode.Price, newPrice, false);			// Readjust stoploss level	
							ExitLongStopMarket(0, true, PositionSize, newPrice, "Stop " + entryLongString1, entryLongString1);
							if (enableRunner == true) {
								//SetStopLoss(entryLongString2, CalculationMode.Price, newPrice, false);
								ExitLongStopMarket(0, true, RunnerPositionSize, newPrice, "Stop " + entryLongString2, entryLongString2);
							}
							previousPrice = newPrice;				 				// save for price adjust on next candle
							stopPlot = newPrice; 					 				// save to adjust plot line
							
						}
					} else if (EnableTrail == true) {
						 if (Close[0] > Position.AveragePrice + TrailProfitTrigger * TickSize  && previousPrice == 0) {
						 	previousPrice = Position.AveragePrice;
						 } else if (previousPrice	!= 0
 							&& GetCurrentAsk() > previousPrice + TrailProfitTrigger * TickSize)
						{
							if (trailStopType == CommonEnums.TrailStopType.TickTrail) {
								newPrice = previousPrice + TrailStepTicks * TickSize; // Calculate trail stop adjustment
							} else if (trailStopType == CommonEnums.TrailStopType.ATRTrail) {
								//newPrice = (previousPrice < TrailStop_ATR.TrailingStopLow[0]) ? TrailStop_ATR.TrailingStopLow[0] : previousPrice;
								newPrice =  TrailStop_ATR.TrailingStopLow[0];
							} 	// Calculate trail stop adjustment
							else if (trailStopType == CommonEnums.TrailStopType.BarTrail && newPrice < Low[TrailByBars]) {
								newPrice = Low[TrailByBars];
							}
						
							//SetStopLoss(entryLongString1, CalculationMode.Price, newPrice, false);			// Readjust stoploss level	
							ExitLongStopMarket(0, true, PositionSize, newPrice, "Stop " + entryLongString1, entryLongString1);
							if (enableRunner == true) {
								//SetStopLoss(entryLongString2, CalculationMode.Price, newPrice, false);
								ExitLongStopMarket(0, true, RunnerPositionSize, newPrice, "Stop " + entryLongString2, entryLongString2);
							}
							previousPrice = newPrice;				 				// save for price adjust on next candle
							stopPlot = newPrice; 					 				// save to adjust plot line
						}
						
					}
					
                    break;
					
					
                case MarketPosition.Short:
					
					if (previousPrice == 0) 
					{
						stopPlot = Position.AveragePrice + InitialStopShort * TickSize;  // initial stop plot level
					}
					
					if (EnableBreakeven == true) {
                    // Once the price is Less than entry price - breakEvenTicks ticks, set stop loss to breakeven
	                    if (Close[0] < Position.AveragePrice - BreakevenTicks * TickSize && previousPrice == 0)
	                    {
							initialBreakEven = Position.AveragePrice - PlusBreakeven * TickSize;
	                       // SetStopLoss(entryShortString1, CalculationMode.Price, initialBreakEven, false);
							ExitShortStopMarket(0, true, PositionSize, initialBreakEven, "Stop " + entryShortString1, entryShortString1);
							if (enableRunner == true) {
								//SetStopLoss(entryShortString2, CalculationMode.Price, initialBreakEven, false);
								ExitShortStopMarket(0, true, RunnerPositionSize, initialBreakEven, "Stop " + entryShortString2, entryShortString2);
							}
							previousPrice = Position.AveragePrice;
							stopPlot = initialBreakEven;
	                    }
						// Once at breakeven wait till trailProfitTrigger is reached before advancing stoploss by trailStepTicks size step
						else if (previousPrice	!= 0 ////StopLoss is at breakeven
	 							&& GetCurrentAsk() < previousPrice - TrailProfitTrigger * TickSize && EnableTrail == true )
						{
							
							if (trailStopType == CommonEnums.TrailStopType.TickTrail) {
								newPrice = previousPrice - TrailStepTicks * TickSize;
							} else if (trailStopType == CommonEnums.TrailStopType.ATRTrail) {
								//newPrice = (previousPrice > TrailStop_ATR.TrailingStopHigh[0]) ? TrailStop_ATR.TrailingStopHigh[0] : previousPrice;
								newPrice = TrailStop_ATR.TrailingStopHigh[0];
							} else if (trailStopType == CommonEnums.TrailStopType.BarTrail && newPrice > High[TrailByBars]) {
								newPrice = High[TrailByBars];
							}
							//SetStopLoss(entryShortString1, CalculationMode.Price, newPrice, false);
							ExitShortStopMarket(0, true, PositionSize, newPrice, "Stop " + entryShortString1, entryShortString1);
							if (enableRunner == true) {
								//SetStopLoss(entryShortString2, CalculationMode.Price, newPrice, false);
								ExitShortStopMarket(0, true, RunnerPositionSize, newPrice, "Stop " + entryShortString2, entryShortString2);
							}
							previousPrice = newPrice;
							stopPlot = newPrice;
						}
						
					} else if (EnableTrail == true) {
						if (Close[0] > Position.AveragePrice - TrailProfitTrigger * TickSize  && previousPrice == 0) {
						 	previousPrice = Position.AveragePrice;
						 } else if (previousPrice	!= 0 ////StopLoss is at breakeven
	 							&& GetCurrentAsk() < previousPrice - TrailProfitTrigger * TickSize )
							{
								if (trailStopType == CommonEnums.TrailStopType.TickTrail) {
									newPrice = previousPrice - TrailStepTicks * TickSize;
								}  else if (trailStopType == CommonEnums.TrailStopType.ATRTrail) {
									newPrice = (previousPrice > TrailStop_ATR.TrailingStopHigh[0]) ? TrailStop_ATR.TrailingStopHigh[0] : previousPrice;
									newPrice = TrailStop_ATR.TrailingStopHigh[0];
								} else if (trailStopType == CommonEnums.TrailStopType.BarTrail && newPrice > High[TrailByBars]) {
									newPrice = High[TrailByBars];
								}
								//SetStopLoss(entryShortString1, CalculationMode.Price, newPrice, false);
								ExitShortStopMarket(0, true, PositionSize, newPrice, "Stop " + entryShortString1, entryShortString1);
								if (enableRunner == true) {
									//SetStopLoss(entryShortString2, CalculationMode.Price, newPrice, false);
									ExitShortStopMarket(0, true, RunnerPositionSize, newPrice, "Stop " + entryShortString2, entryShortString2);
								}
								previousPrice = newPrice;
								stopPlot = newPrice;
							}
					}			

                    break;
                default:
                    break;
			}	
		}
		
		#endregion
		
		#region OnBarUpdate
		protected override void OnBarUpdate()
		{ 
            //Add your custom strategy logic here.
            if (CurrentBar < BarsRequiredToTrade)
                return;
			

            if (IsStratEnabled == true && (Position.MarketPosition == MarketPosition.Flat)
				&& (orderState == CommonEnums.OrderState.BOTH || orderState == CommonEnums.OrderState.LONGS) 
                && validateEntryLong()
				&& validateTimeControlsAndTradeCount()
                && ((BarsSinceEntryExecution(0, "", 0) == -1)
					 || (BarsSinceEntryExecution(0, "", 0) > 1))) {
                
				if (orderType == CommonEnums.OrderType.MARKET) {
                	EnterLong(Convert.ToInt32(PositionSize), entryLongString1);
					if (enableRunner == true) {
						EnterLong(Convert.ToInt32(RunnerPositionSize), entryLongString2);
					}
					
				} else if (orderType == CommonEnums.OrderType.LIMIT) {
					if (limitType == CommonEnums.LimitType.CLOSE) {
						EnterLongStopMarket(Convert.ToInt32(PositionSize), Close[0] + LimitOffset*TickSize, entryLongString1);
						if (enableRunner == true) {
							EnterLongStopMarket(Convert.ToInt32(RunnerPositionSize), Close[0] + LimitOffset*TickSize, entryLongString2);
						}
					} else if (limitType == CommonEnums.LimitType.HILO) {
						Print("LimitOffset "+LimitOffset);
						Print("High[0] "+High[0]);
						EnterLongStopMarket(Convert.ToInt32(PositionSize), High[0] + LimitOffset*TickSize, entryLongString1);
						if (enableRunner == true) {
							EnterLongStopMarket(Convert.ToInt32(RunnerPositionSize), High[0] + LimitOffset*TickSize, entryLongString2);
						}
					}
				}
				incrementSessionTradeCount();
            }

            if (IsStratEnabled == true && (Position.MarketPosition == MarketPosition.Flat)
				&& (orderState == CommonEnums.OrderState.BOTH || orderState == CommonEnums.OrderState.SHORTS) 
                && validateEntryShort()
				&& validateTimeControlsAndTradeCount()
                && ((BarsSinceEntryExecution(0, "", 0) == -1)
					 || (BarsSinceEntryExecution(0, "", 0) > 1))) {
				if (orderType == CommonEnums.OrderType.MARKET) {
                	EnterShort(Convert.ToInt32(PositionSize), entryShortString1);
					if (enableRunner == true) {
						EnterShort(Convert.ToInt32(RunnerPositionSize), entryShortString2);
					}
				} else if (orderType == CommonEnums.OrderType.LIMIT) {
					if (limitType == CommonEnums.LimitType.CLOSE) {
						EnterShortStopMarket(0, false, Convert.ToInt32(PositionSize), Close[0] - LimitOffset*TickSize, entryShortString1);
						if (enableRunner == true) {
							EnterShortStopMarket(Convert.ToInt32(RunnerPositionSize), Close[0] - LimitOffset*TickSize, entryShortString2);
						}
					} else if (limitType == CommonEnums.LimitType.HILO) {
						entryOrder = EnterShortStopMarket(0, false, Convert.ToInt32(PositionSize), Low[0] - LimitOffset*TickSize, entryShortString1);
						if (enableRunner == true) {
							EnterShortStopMarket(Convert.ToInt32(RunnerPositionSize), Low[0] - LimitOffset*TickSize, entryShortString2);
						}
					}
				}
				incrementSessionTradeCount();

            }

			calculateTrailStopAndBE();
			validateMaxTradesPerSession();
			validateStrategyPnl();	
			resetSessionTradeCount();
			
			if (Position.MarketPosition == MarketPosition.Long) {
				if (validateExitLong() == true) {
					ExitLong("Exit "+entryLongString1, entryLongString1);
					if (enableRunner == true) {
						ExitLong("Exit "+entryLongString2, entryLongString2);
					}
				}
			}
			
			if (Position.MarketPosition == MarketPosition.Short) {
				if (validateExitShort() == true) {
					ExitLong("Exit "+entryShortString1, entryShortString1);
					if (enableRunner == true) {
						ExitLong("Exit "+entryShortString2, entryShortString2);
					}
				}
			}
        }
        #endregion
		
		#region Custom Property Manipulation
		
		private void ModifyLimitTypeProperties(PropertyDescriptorCollection col) {
			if (showLimitTypeOptions == false) {
				col.Remove(col.Find("LimitType", true));
				col.Remove(col.Find("LimitOffset", true));
			}
		}
		
		private void ModifyTrailProperties(PropertyDescriptorCollection col) {
			if (showTrailOptions == false) {
				col.Remove(col.Find("TrailProfitTrigger", true));
				col.Remove(col.Find("TrailStopType", true));
				col.Remove(col.Find("TrailStepTicks", true));
				col.Remove(col.Find("TrailStop_ATR_Period", true));
				col.Remove(col.Find("TrailStop_ATR_Mult", true));
				col.Remove(col.Find("TrailByBars", true));
			}
		}
		
		private void ModifyTrailStopTypeProperties(PropertyDescriptorCollection col) {
			Print("showTickTrailOptions "+showTickTrailOptions);
			Print("showATRTrailOptions "+ showATRTrailOptions);
			Print("showBarTrailOptions "+showBarTrailOptions);
			if (showTickTrailOptions == false) {
				col.Remove(col.Find("TrailStepTicks", true));
			} 
			if (showATRTrailOptions == false) {
				col.Remove(col.Find("TrailStop_ATR_Period", true));
				col.Remove(col.Find("TrailStop_ATR_Mult", true));
			}
			if (showBarTrailOptions == false) {
				Print("Remove Trail By Bars");
				col.Remove(col.Find("TrailByBars", true));
			}
		}
		
		private void ModifyBreakevenProperties(PropertyDescriptorCollection col) {
			if (showBreakevenOptions == false) {
				col.Remove(col.Find("BreakevenTicks", true));
				col.Remove(col.Find("PlusBreakeven", true));
			}
		}
		
		private void ModifyRunnerProperties(PropertyDescriptorCollection col) {
			if (showRunnerOptions == false) {
				col.Remove(col.Find("RunnerPositionSize", true));
				col.Remove(col.Find("Runner_Mult", true));
				col.Remove(col.Find("JumpToProfit", true));
				col.Remove(col.Find("JumpToProfitTickOffset", true));
			}
		}
		
		private void ModifyStopLossTypeProperties(PropertyDescriptorCollection col) {
			if (showFixedStopLossOptions == false) {
				col.Remove(col.Find("InitialStopLong", true));
				col.Remove(col.Find("InitialStopShort", true));
			} 
			if (showATRStopLossOptions== false) {
				col.Remove(col.Find("StopLoss_ATR_Period", true));
				col.Remove(col.Find("StopLoss_ATR_Mult", true));
			}
		}
		
		private void ModifyProfitTargetTypeProperties(PropertyDescriptorCollection col) {
			if (showFixedProfitTargetOptions == false) {
				col.Remove(col.Find("ProfitTargetLong", true));
				col.Remove(col.Find("ProfitTargetShort", true));
			} 
			if (showATRProfitTargetOptions == false) {
				col.Remove(col.Find("ProfitTarget_ATR_Period", true));
				col.Remove(col.Find("ProfitTarget_ATR_Mult", true));
			}
		}
		
		#endregion
		
		#region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(GetType());
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(GetType());
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(GetType());
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(GetType());
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(GetType());
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(GetType());
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(GetType(), editorBaseType);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(GetType(), attributes);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(GetType());
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection orig = TypeDescriptor.GetProperties(GetType(), attributes);
            PropertyDescriptor[] arr = new PropertyDescriptor[orig.Count];
            orig.CopyTo(arr, 0);
            PropertyDescriptorCollection col = new PropertyDescriptorCollection(arr);

			ModifyLimitTypeProperties(col);
			ModifyTrailProperties(col);
			ModifyBreakevenProperties(col);
			ModifyRunnerProperties(col);
			ModifyTrailStopTypeProperties(col);
			ModifyStopLossTypeProperties(col);
			ModifyProfitTargetTypeProperties(col);
            return col;

        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(GetType());
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
		
		#region Trade Performance
		private void DrawHistoricalTradePerformance(ChartControl chartControl) {

			
			TradeCollection allTrades = SystemPerformance.AllTrades;
			TradeCollection winningTrades = allTrades.WinningTrades;
			TradeCollection loosingTrades = allTrades.LosingTrades;
			
			
			int totalTradeCount = allTrades.TradesCount;
			int winningTradesCount = winningTrades.TradesCount;
			int loosingTradesCount = loosingTrades.TradesCount;
			double profitFactor = allTrades.TradesPerformance.ProfitFactor;
			double grossProfit = allTrades.TradesPerformance.GrossProfit;
			double grossLoss	= allTrades.TradesPerformance.GrossLoss;
			double netProfitLoss	= allTrades.TradesPerformance.NetProfit;
			//double profitability = (winningTradesCount/totalTradeCount) * 100;
			
			string textLine0 = "Trade Performance (Historical)";
			string textLine1 = "Total # of Trades : "+totalTradeCount;
			string textLine2 = "# of Winning Trades : "+winningTradesCount;
			string textLine3 = "# of Loosing Trades : "+loosingTradesCount;
			string textLine4 = "Gross Profit : $"+grossProfit;
			string textLine5 = "Gross Loss : ($"+ grossLoss+")";
			string textLine6 = "Net Profit: "+ (netProfitLoss < 0 ? "($"+netProfitLoss+")" : "$"+netProfitLoss);
			string textLine7 = "Profit Factor: "+profitFactor;
			
			string tradePerfText = textLine0 + "\n" + textLine1 + "\n" + textLine2 + "\n" + textLine3 + "\n" + textLine4 + "\n" + textLine5 + "\n" + textLine6 + "\n" + textLine7 + "\n";
			
			SimpleFont font = new SimpleFont("Courier New", 12) {Bold = true };
			Draw.TextFixed(this, "tradePerformanceText", tradePerfText, DisplayHistoricalTradePerformanceOrientation, chartControl.Properties.ChartText, font, Brushes.AntiqueWhite, Brushes.Transparent, 0);
		}
		
		private double getCumProfit() {
			TradeCollection realTimeTrades = SystemPerformance.RealTimeTrades;
			return realTimeTrades.TradesPerformance.Currency.CumProfit;
		}
		
		private void DrawStrategyPnl(ChartControl chartControl) {
			
			double cumProfit = getCumProfit();
			string textLine0 = "Realtime Strategy PnL";
			string textLine1 = "Cumulative Profit: "+ (cumProfit < 0 ? "($"+cumProfit+")" : "$"+cumProfit);
			string textLine2 = "";
			string textLine3 = "";
			string textLine4 = "";
			if (cumProfit <= MaxLoss) {
				textLine4 = "Max Loss level reached :( ";
			} else if (cumProfit >= MaxTarget) {
				textLine4 = "Max Target level reached :) ";
			}
			
			textLine2  = "Session Number: " + SessionNumber;
			
			if (SessionNumber == 1)
				textLine3 = "Trades in this Session: " + Session1Count;
			if (SessionNumber == 2)
				textLine3 = "Trades in this Session: " + Session2Count;
			if (SessionNumber == 3)
				textLine3 = "Trades in this Session: " + Session3Count;
			if (SessionNumber == 4)
				textLine3 = "Trades in this Session: " + Session4Count;
			
			string realTimeTradeText = textLine0 + "\n" + textLine1 + "\n" + textLine2 + "\n" + textLine3 + "\n" + textLine4;
			SimpleFont font = new SimpleFont("Courier New", 12) { Size = 15, Bold = true };
			Draw.TextFixed(this, "realTimeTradeText", realTimeTradeText, DisplayStrategyPnLOrientation, Brushes.Aquamarine, font, Brushes.Aquamarine, Brushes.Transparent, 0);
		}
		#endregion
		
		#region onRender
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			//if (hasRanOnceFirstCycle)
			//{
			if (DisplayStrategyPnL == true) {
				DrawStrategyPnl(chartControl);
			}
			if (DisplayHistoricalTradePerformance == true) {
				DrawHistoricalTradePerformance(chartControl);
			}
		//	}
		}
		#endregion

		#region onExecutionUpdate
		protected override void OnExecutionUpdate (Execution execution, string executionId, double price, int quantity,MarketPosition marketPosition, string orderId, DateTime time) {
			if (execution.Order.Name == entryLongString1) {
				calculateStopLossPriceLong(price, false);
				calculateProfitTargetPriceLong(price, false);
			}  else if (execution.Order.Name == entryShortString1) {
				calculateStopLossPriceShort(price, false);
				calculateProfitTargetPriceShort(price, false);
			}
			
			if (execution.Order.Name == entryLongString2) {
				calculateStopLossPriceLong(price, true);
				calculateProfitTargetPriceLong(price, true);
			}  else if (execution.Order.Name == entryShortString2) {
				calculateStopLossPriceShort(price, true);
				calculateProfitTargetPriceShort(price, true);
			}
			
			if (execution.Order.Name == "Profit Target "+entryLongString1)
			{
				Print("Profit Target hit for long, moving runner SL to profit target price");
				isProfitTargetHit = true;
				profitTargetPrice = price;
				
				if ((Position.MarketPosition == MarketPosition.Long)
				&& (EnableRunner == true)
				&& (JumpToProfit == true)) {
				//	SetStopLoss(entryLongString2, CalculationMode.Price, price - JumptoProfitTickOffset*TickSize, false);
					ExitLongStopMarket(0, true, RunnerPositionSize, price - JumpToProfitTickOffset*TickSize, "Stop " + entryLongString2, entryLongString2);
					//jumpToProfitSet = true;
				}
			} else if (execution.Order.Name == "Profit Target "+entryShortString1) {
				
				if ((Position.MarketPosition == MarketPosition.Short)
					&& (EnableRunner == true)
					&& (JumpToProfit == true)) {
						//SetStopLoss(entryShortString2, CalculationMode.Price, price + JumptoProfitTickOffset*TickSize, false);
					ExitShortStopMarket(0, true, RunnerPositionSize, price + JumpToProfitTickOffset*TickSize, "Stop " + entryShortString2, entryShortString2);
						//	jumpToProfitSet = true;
					}
			}
		}
		#endregion
		
		#region Properties
		
		[NinjaScriptProperty]
		[Display(Name="BaseAlgoVersion", Order=1, GroupName="0. Strategy Information")]
		public string BaseAlgoVersion
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="StrategyVersion", Order=2, GroupName="0. Strategy Information")]
		public string StrategyVersion
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Author", Order=3, GroupName="0. Strategy Information")]
		public string Author
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Credits", Order=3, GroupName="0. Strategy Information")]
		public string Credits
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Disclaimer", Order=4, GroupName="0. Strategy Information")]
		public string Disclaimer
		{ get; set; }
		

        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PositionSize", Order=1, GroupName="2. Order Params")]
		public int PositionSize
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "OrderType", GroupName = "2. Order Params", Order = 2)]
		[RefreshProperties(RefreshProperties.All)]
        public CommonEnums.OrderType OrderType {
			get { return orderType;}
			set {
				orderType = value;
				
				if (orderType == CommonEnums.OrderType.LIMIT) {
					showLimitTypeOptions = true;
				} else {
					showLimitTypeOptions = false;
				}
			}
		}
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "LimitType", GroupName = "2. Order Params", Order = 3)]
        public CommonEnums.LimitType LimitType {
			get { return limitType;}
			set {
				limitType = value;
			}
		}
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="LimitOffset", Order=4, GroupName="2. Order Params")]
		public int LimitOffset
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MaxTradesPerSession", Order=1, GroupName="2. Order Params")]
		public int MaxTradesPerSession
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "StopLossType", Description= "Type of Stop Loss", GroupName = "2.1 Order Params - Stop Loss", Order = 1)]
        [RefreshProperties(RefreshProperties.All)]
		public CommonEnums.StopLossType StopLossType
        { 
			get { return stopLossType; } 
			set { 
				stopLossType = value; 
				
				if (stopLossType == CommonEnums.StopLossType.Fixed) {
					showFixedStopLossOptions = true;
					showATRStopLossOptions = false;
				} else if (stopLossType == CommonEnums.StopLossType.ATR) {
					showFixedStopLossOptions = false;
					showATRStopLossOptions = true;
				}
			}
		}

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
		[Display(Name="InitialStopLong", Order=2, GroupName="2.1 Order Params - Stop Loss")]
		public int InitialStopLong
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="InitialStopShort", Order=3, GroupName="2.1 Order Params - Stop Loss")]
		public int InitialStopShort
		{ get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
		[Display(Name="StopLoss_ATR_Period", Order=4, GroupName="2.1 Order Params - Stop Loss")]
		public int StopLoss_ATR_Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="StopLoss_ATR_Mult", Order=5, GroupName="2.1 Order Params - Stop Loss")]
		public double StopLoss_ATR_Mult
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTargetType", Description= "Type of Profit Target", GroupName = "2.2 Order Params - Profit Target", Order = 1)]
        [RefreshProperties(RefreshProperties.All)]
		public CommonEnums.ProfitTargetType ProfitTargetType
        { 
			get { return profitTargetType; } 
			set { 
				profitTargetType = value; 
				
				if (profitTargetType == CommonEnums.ProfitTargetType.Fixed) {
					showFixedProfitTargetOptions = true;
					showATRProfitTargetOptions = false;
				} else if (profitTargetType == CommonEnums.ProfitTargetType.ATR) {
					showFixedProfitTargetOptions = false;
					showATRProfitTargetOptions = true;
				}
			}
		}
		
        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ProfitTargetLong", Order=2, GroupName="2.2 Order Params - Profit Target")]
		public int ProfitTargetLong
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ProfitTargetShort", Order=3, GroupName="2.2 Order Params - Profit Target")]
		public int ProfitTargetShort
		{ get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
		[Display(Name="ProfitTarget_ATR_Period", Order=4, GroupName="2.2 Order Params - Profit Target")]
		public int ProfitTarget_ATR_Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="ProfitTarget_ATR_Mult", Order=5, GroupName="2.2 Order Params - Profit Target")]
		public double ProfitTarget_ATR_Mult
		{ get; set; }
		
		
		[NinjaScriptProperty]
        [Display(Name = "EnableTrail", Order = 1, GroupName = "2.3 Order Params - Trail")]
        [RefreshProperties(RefreshProperties.All)]
		public bool EnableTrail
        { 
			get{
				return enableTrail;
			} 
			set {
				enableTrail = value;
				
				if (enableTrail == true) {
					showTrailOptions = true;
				} else {
					showTrailOptions = false;
					showTickTrailOptions = false;
					showATRTrailOptions = false;
					showBarTrailOptions = false;
				}
			}
		}
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "TrailStopType", Description= "Type of Trail Stop", GroupName = "2.3 Order Params - Trail", Order = 2)]
        [RefreshProperties(RefreshProperties.All)]
		public CommonEnums.TrailStopType TrailStopType
        { 
			get { return trailStopType; } 
			set { 
				trailStopType = value; 
				if (trailStopType == CommonEnums.TrailStopType.TickTrail) {
					Print("Tick Trail");
					showTickTrailOptions = true;
					showATRTrailOptions = false;
					showBarTrailOptions = false;
				} else if (trailStopType == CommonEnums.TrailStopType.ATRTrail) {
					showTickTrailOptions = false;
					showATRTrailOptions = true;
					showBarTrailOptions = false;
				} else if (trailStopType == CommonEnums.TrailStopType.BarTrail) {
					showTickTrailOptions = false;
					showATRTrailOptions = false;
					showBarTrailOptions = true;
				}
			}
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TrailProfitTrigger", Order=3, GroupName="2.3 Order Params - Trail")]
		public int TrailProfitTrigger
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TrailStepTicks", Order=4, GroupName="2.3 Order Params - Trail")]
		public int TrailStepTicks
		{ get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
		[Display(Name="TrailStop_ATR_Period", Order=5, GroupName="2.3 Order Params - Trail")]
		public int TrailStop_ATR_Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="TrailStop_ATR_Mult", Order=6, GroupName="2.3 Order Params - Trail")]
		public double TrailStop_ATR_Mult
		{ get; set; }
		
		[NinjaScriptProperty]
        [Range(1, 5)]
		[Display(Name="TrailByBars", Order=7, GroupName="2.3 Order Params - Trail")]
		public int TrailByBars
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "EnableBreakeven", Order = 1, GroupName = "2.4 Order Params - Breakeven")]
        [RefreshProperties(RefreshProperties.All)]
		public bool EnableBreakeven
        { 
			get{
				return enableBreakeven;
			} 
			set {
				enableBreakeven = value;
				
				if (enableBreakeven == true) {
					showBreakevenOptions = true;
				} else {
					showBreakevenOptions = false;
				}
			}
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BreakevenTicks", Order=2, GroupName="2.4 Order Params - Breakeven")]
		public int BreakevenTicks
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PlusBreakeven", Order=3, GroupName="2.4 Order Params - Breakeven")]
		public int PlusBreakeven
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "EnableRunner", Order = 1, GroupName = "2.5 Order Params - Runner")]
        [RefreshProperties(RefreshProperties.All)]
		public bool EnableRunner
        { 
			get{
				return enableRunner;
			} 
			set {
				enableRunner = value;
				
				if (enableRunner == true) {
					showRunnerOptions = true;
				} else {
					showRunnerOptions = false;
				}
			}
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RunnerPositionSize", Order=2, GroupName="2.5 Order Params - Runner")]
		public int RunnerPositionSize
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Runner_Mult", Order=3, Description="If ATR is selected from profit target from above then this is ATR Multiplier for runner, if Fixed profit target is select from above then ProfitTargetInTicks * Runner_Mult ", GroupName="2.5 Order Params - Runner")]
		public double Runner_Mult
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "JumpToProfit", Order = 4, GroupName = "2.5 Order Params - Runner")]
		public bool JumpToProfit
        { get; set;
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="JumpToProfitTickOffset", Order=5, GroupName="2.5 Order Params - Runner")]
		public int JumpToProfitTickOffset
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Time_1", Order = 1, GroupName = "3. Time Controls")]
        public bool Time_1
        { 
			get; set;
		}

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_1", Order = 2, GroupName = "3. Time Controls")]
        public DateTime Start_Time_1
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_1", Order = 3, GroupName = "3. Time Controls")]
        public DateTime Stop_Time_1
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_2", Order = 4, GroupName = "3. Time Controls")]
        public bool Time_2
        { 
			get;  set;
		}

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_2", Order = 5, GroupName = "3. Time Controls")]
        public DateTime Start_Time_2
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_2",  Order = 6, GroupName = "3. Time Controls")]
        public DateTime Stop_Time_2
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_3", Order = 7, GroupName = "3. Time Controls")]
        public bool Time_3
        { 
			get; set; 
		}

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_3", Order = 8, GroupName = "3. Time Controls")]
        public DateTime Start_Time_3
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_3", Order = 9, GroupName = "3. Time Controls")]
        public DateTime Stop_Time_3
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_4", Order = 10, GroupName = "3. Time Controls")]
        public bool Time_4
        { 
			get; set;
		}

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_4", Order = 11, GroupName = "3. Time Controls")]
        public DateTime Start_Time_4
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_4", Order = 12, GroupName = "3. Time Controls")]
        public DateTime Stop_Time_4
        { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MaxTarget", Description="Maximum Target for one strategy run", Order=1, GroupName="4. Strategy PnL")]
		public int MaxTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MaxLoss", Description="Max Loss for one strategy run", Order=2, GroupName="4. Strategy PnL")]
		public int MaxLoss
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "DisplayStrategyPnL", Order = 2, GroupName = "5. Information Panel Controls")]
        public bool DisplayStrategyPnL
        { 
			get; set;  
		}
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "DisplayStrategyPnLOrientation", GroupName = "5. Information Panel Controls", Order = 3)]
		public TextPosition DisplayStrategyPnLOrientation
        { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "DisplayHistoricalTradePerformance", Order = 4, GroupName = "5. Information Panel Controls")]
        public bool DisplayHistoricalTradePerformance
        { 
			get; set;
		}
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "DisplayHistoricalTradePerformanceOrientation", GroupName = "5. Information Panel Controls", Order = 5)]
		public TextPosition DisplayHistoricalTradePerformanceOrientation
        { get; set; }
		#endregion

    }
}

#region Strategy Enums
namespace CommonEnums
{
	public enum OrderState
	{
		LONGS,
		SHORTS,
		BOTH
	}
	
	public enum OrderType
	{
		MARKET,
		LIMIT
	}
	
	public enum LimitType
	{
		CLOSE,
		HILO,
		CUSTOM
	}
	
	public enum StopLossType
	{
		Fixed,
		ATR,
		Custom
	}
	
	public enum ProfitTargetType
	{
		Fixed,
		ATR
	}
	
	public enum TrailStopType
	{
		TickTrail,
		ATRTrail,
		BarTrail
	}
}

#endregion