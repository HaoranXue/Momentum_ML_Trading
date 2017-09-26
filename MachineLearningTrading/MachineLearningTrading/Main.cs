using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Deedle;
using MathNet.Numerics.Statistics;
using machinelearning;
using portfolio_optimization;
using Preprocessing;
using BacktestSystem;
using System.Linq;


namespace MLtrading
{
    class MainClass
    {
        public static void Main(string[] args)
        {
			Directory.SetCurrentDirectory("C:/Users/Jeremy/Documents/GitHub/Momentum_ML_Trading_Vega/MachineLearningTrading/MachineLearningTrading");
			
            Console.WriteLine("Enter 0 for backtest the strategy, enter 1 for trading via the strategy");
            string Order = Console.ReadLine();

            if (Order =="0")
            {
                Console.WriteLine("Please enter the start date you want to do the backtest(Format: YYYY-MM-DD)");
                string Date = Console.ReadLine();
                Console.WriteLine("Please enter the weeks you want to do the backtest");
                string Weeks = Console.ReadLine();

                BackTest( Date, Weeks);
            }
            else if (Order =="1")
            {
                Trade();
            }
            else
            {
                Console.WriteLine("Error of setting trade or backtet. ");
            }

            Console.ReadKey();
        }

        public static void BackTest(string Date, string Weeks)
        {

			// Data Preprocessing

            var Mybacktest = new Backtest();
            var Mybacktest_adj = new Backtest();
            Mybacktest.Init();
            Mybacktest_adj.Init();

            // Set list to store data
            List<double> Hisc_netValue = new List<double>();
            List<double> Adj_netValue = new List<double>();
            List<string> ETFs_holding_FI = new List<string>();
            List<string> ETFs_holding_Equ = new List<string>();
            string[][] trading_history_ETF = new string[Convert.ToInt64(Weeks)][];
            double[][] trading_history_allocation = new double[Convert.ToInt64(Weeks)][];
            double[][] ADJtrading_history_allocation = new double[Convert.ToInt64(Weeks)][];

            // init overall turnover
            double TurnOver = 0;

            for (int i = 0; i < Convert.ToInt64(Weeks); i++)
            {   

                // seting Datapreprocessing class 
                var pro_FI = new DataPreProcessing();
                var pro_Equ = new DataPreProcessing();

                var Today = DateTime.Parse(Date).AddDays(i*7);

                Console.WriteLine("Date: {0}", Today.ToString());

                // cleaning data use data preprocessing class 
                pro_FI.Run(Today.ToString(), 112, "Fixed Income");
                pro_Equ.Run(Today.ToString(), 112, "Equity");

                // Set prediction vector
                double[] predictions_FI = new double[pro_FI.Trade_ETF.Count];
                double[] predictions_Equ = new double[pro_Equ.Trade_ETF.Count];

                // Set blend ETFs list
                List<string> Blend_ETFs = new List<string>();

                // FI prediction and long //

                for (int j = 0; j < pro_FI.Trade_ETF.Count; j++)
                {
                    var y = pro_FI.Target_List[j];

                    var fy = new FrameBuilder.Columns<DateTime, string>{
                     { "Y"  , y  }
                    }.Frame;
                    var data = pro_FI.Feature_List[j].Join(fy);
                    var pred_Features = pro_FI.pred_Feature_List[j];

                    data.SaveCsv("dataset.csv");

                    var prediction = Learning.Fit(pred_Features);

                    predictions_FI[j] = prediction;
                
                }

                // Get the minimum scores of top 5 ETF 
                var hold_FI = PredRanking(predictions_FI,5);

                // Get the namelist of top 5 ETF
                List<string> ETFs_FI = new List<string>();

                for (int m = 0; m < pro_FI.Trade_ETF.Count; m++)
                {
                    if (predictions_FI[m] >= hold_FI)
                    {   
                        ETFs_FI.Add(pro_FI.Trade_ETF[m]);
                    }
                }

                // Cacualte the Unitility and decide if we should trade this week
                if (i ==0)
                {
                    ETFs_holding_FI = ETFs_FI;
                }
                else
                {
                    
                    double[] holding_pred = ETFname2Prediction(ETFs_holding_FI, predictions_FI, pro_FI);
                    double[] long_pred = ETFname2Prediction(ETFs_FI, predictions_FI, pro_FI);

                    double trade_diff= long_pred.Sum() - holding_pred.Sum();

                    if (trade_diff < 0.03)
                    {
                        ETFs_FI = ETFs_holding_FI;
                    }
                    else
                    {
                        ETFs_holding_FI = ETFs_FI;
                    }

                    TurnOver += CaculateTurnOver(ETFs_holding_FI, ETFs_FI,0.04);
                }

                Console.WriteLine("Long the following ETFs: ");

                for (int n = 0; n < ETFs_FI.Count; n++)
                {
                    Console.WriteLine(ETFs_FI[n]);
                    Blend_ETFs.Add(ETFs_FI[n]);
                }

				// Equity ETF predict and long 


                for (int j = 0; j < pro_Equ.Trade_ETF.Count; j++)
				{
                    var y = pro_Equ.Target_List[j];

					var fy = new FrameBuilder.Columns<DateTime, string>{
					 { "Y"  , y  }
					}.Frame;
                    var data = pro_Equ.Feature_List[j].Join(fy);
                    var pred_Features = pro_Equ.pred_Feature_List[j];

					data.SaveCsv("dataset.csv");

					var prediction = Learning.Fit(pred_Features);

                    predictions_Equ[j] = prediction;
				}


				List<string> ETFs_Equ = new List<string>();

                var hold_Equ = PredRanking(predictions_Equ, 5);

                for(int m = 0; m < pro_Equ.Trade_ETF.Count; m++)
                {
                    if (predictions_Equ[m] >= hold_Equ)
					{
                        ETFs_Equ.Add(pro_Equ.Trade_ETF[m]);
					}
				}

				if (i == 0)
				{
					ETFs_holding_Equ = ETFs_Equ;
				}
				else
				{

                    double[] holding_pred = ETFname2Prediction(ETFs_holding_Equ, predictions_Equ, pro_Equ);
                    double[] long_pred = ETFname2Prediction(ETFs_Equ, predictions_Equ, pro_Equ);

					double trade_diff = long_pred.Sum() - holding_pred.Sum();

					if (trade_diff < 0.1)
					{
						ETFs_Equ = ETFs_holding_Equ;
					}
					else
					{
						ETFs_holding_Equ = ETFs_Equ;
					}

					TurnOver += CaculateTurnOver(ETFs_holding_Equ, ETFs_Equ,0.16);
				}

				for (int n = 0; n < ETFs_Equ.Count; n++)
				{
					Console.WriteLine(ETFs_Equ[n]);
                    Blend_ETFs.Add(ETFs_Equ[n]);
				}

                //  Caculate optimized allocations

                double[] AllocationFI = PO.ETFs2Allocation(ETFs_FI, pro_FI);
                double[] AllocationEqu = PO.ETFs2Allocation(ETFs_Equ, pro_Equ);

                double[] allocations = new double[10];

                for (int fi = 0; fi < 5; fi++)
                {
                    allocations[fi] = AllocationFI[fi] * 0.2;
                }

                for (int equ = 0; equ < 5; equ++)
                {
                    allocations[equ + 5] = AllocationEqu[equ] * 0.8;
                }

                double DrawDown;

                if (i == 0)
                {
                    DrawDown = 0;     
                }
                else
                {
                    DrawDown = 1 - Hisc_netValue.Last() / Hisc_netValue.Max();
                }


                double Position_ratio;

                if (DrawDown > 0.03)
                {
                     Position_ratio = 0.9; 
                }
                else if (DrawDown > 0.07)
                {
                     Position_ratio = 0.8;
                }
                else if (DrawDown > 0.1)
                {
                     Position_ratio = 0.6; 
                }
                else
                {
                     Position_ratio = 1;
                }

                Console.WriteLine("ADJ position is {0}", Position_ratio);
                double[] ALLOCATION = new double[10];

                for (int allo = 0; allo < 10; allo++)
                {
                    ALLOCATION[allo] = Position_ratio * allocations[allo];
                }

                //Console.WriteLine("Current Drawdown is {0}", DrawDown );
                //Console.WriteLine("Position is {0}", ALLOCATION.Sum());

                trading_history_ETF[i] = new string[10];

                string[] ETFs = new string[10];

                for (int etf = 0; etf < 10; etf++)
                {
                    ETFs[etf] = Blend_ETFs[etf];
                }

                trading_history_ETF[i] = ETFs;

                trading_history_allocation[i] = new double[10];
                trading_history_allocation[i] = allocations;

                ADJtrading_history_allocation[i] = new double[10];
                ADJtrading_history_allocation[i] = ALLOCATION;


                Hisc_netValue.Add(Mybacktest.Rebalance(Today, ETFs, allocations));
                Adj_netValue.Add(Mybacktest_adj.Rebalance(Today, ETFs, ALLOCATION));
            }


            // Result analysis for NetValue

            var StrategyNetValue = Hisc_netValue.ToArray();

			double MaxDD = 0;

			for (int i = 1; i < StrategyNetValue.Length; i++)
			{
				var MaxNetValue = StrategyNetValue.Take(i).Max();
				double drawdown = 1 - StrategyNetValue[i] / MaxNetValue;

				if (drawdown > MaxDD)
				{
					MaxDD = drawdown;
				}
			}

			Console.WriteLine("Maximum drawdown of This Strategy is: {0}", MaxDD);

			var AnnualReturn = Math.Log(StrategyNetValue.Last()) /
														(Convert.ToDouble(Weeks) / 50);
            
			Console.WriteLine("Annual Return of This Strategy is: {0}", AnnualReturn);

			var StrategyReturn = NetValue2Return(StrategyNetValue);
			Console.WriteLine("Standard Deviation of This Strategy is: {0}",
									  Statistics.StandardDeviation(StrategyReturn));

            Console.WriteLine("Overall TurnOver: {0}", TurnOver);

            double[] BTmetrics = new double[4];
            BTmetrics[0] = AnnualReturn;
            BTmetrics[1] = MaxDD;
            BTmetrics[2] = Statistics.StandardDeviation(StrategyReturn)*Math.Sqrt(50);
            BTmetrics[3] = TurnOver;

            // Result analysis for AdjNetValue

            var ADJStrategyNetValue = Adj_netValue.ToArray();

            double ADJMaxDD = 0;

            for (int i = 1; i < ADJStrategyNetValue.Length; i++)
			{
				var MaxNetValue = ADJStrategyNetValue.Take(i).Max();
				double drawdown = 1 - ADJStrategyNetValue[i] / MaxNetValue;

                if (drawdown > ADJMaxDD)
				{
                    ADJMaxDD = drawdown;
				}
			}
            Console.WriteLine("Maximum drawdown of ADJ Strategy is: {0}", ADJMaxDD);

            var ADJAnnualReturn = Math.Log(ADJStrategyNetValue.Last()) /
														(Convert.ToDouble(Weeks) / 50);
            Console.WriteLine("Annual Return of ADJ Strategy is: {0}", ADJAnnualReturn);

            var ADJStrategyReturn = NetValue2Return(ADJStrategyNetValue);
            Console.WriteLine("Standard Deviation of This Strategy is: {0}",
									  Statistics.StandardDeviation(ADJStrategyReturn));

			double[] ADJBTmetrics = new double[4];
            ADJBTmetrics[0] = ADJAnnualReturn;
			ADJBTmetrics[1] = ADJMaxDD;
            ADJBTmetrics[2] = Statistics.StandardDeviation(ADJStrategyReturn) * Math.Sqrt(50);
			ADJBTmetrics[3] = TurnOver;


            // Output all results to CSV
            SaveArrayAsCSV_<string>(trading_history_ETF, "TradingHistoryETF.csv");
            SaveArrayAsCSV_(trading_history_allocation, "TradingHistoryAllocation.csv");
            SaveArrayAsCSV_(ADJtrading_history_allocation,"ADJTradingHistoryAllocation.csv");
			SaveArrayAsCSV(ADJBTmetrics, "ADJBacktestMetrics.csv");
            SaveArrayAsCSV(BTmetrics,"BacktestMetrics.csv");
            SaveArrayAsCSV(StrategyNetValue, "Net_value.csv");
            SaveArrayAsCSV(ADJStrategyNetValue,"AdjNetValue.csv");

        }



        public static void Trade()
        {
            // Under Construction // 


        }

        public static double[] NetValue2Return(double[] netvalue)
        {

            double[] NVskip1 = netvalue.Skip(1).ToArray();
            double[] NVTake1 = netvalue.Take(netvalue.GetLength(0)-1).ToArray();


           double[] Return = new double[NVskip1.Count()];

            for (int i = 0; i < NVskip1.Count(); i++)
            {

                var This_Return =  NVskip1[i] / NVTake1[i];
                Return[i] = This_Return;

            }

            return Return;
        }

        public static double[] ETFname2Prediction(  List<string> ETF, 
                                                    double[] prediction,
                                                    DataPreProcessing Pro)
        {
            double[] PRE = new double[ETF.Count];

            for (int j = 0; j < ETF.Count; j++)
            {   
                for (int i = 0; i < Pro.Trade_ETF.Count; i++)
                {
                    if (Pro.Trade_ETF[i] == ETF[j])
                    {
                        PRE[j] = prediction[i];
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return PRE;
        }
        
        public static double CaculateTurnOver(List<string> holding_ETFs, List<string> new_ETFs,double Pcent)
        {
            double turnover = 0;
            var Intersection = new_ETFs.Intersect(holding_ETFs);
            turnover += (5 - Intersection.Count()) * Pcent; 
            return turnover;
        }


        public static void SaveArrayAsCSV<T>(T[] arrayToSave, string fileName)
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                foreach (T item in arrayToSave)
                {
                    file.Write(item + ",");
                }
            }
        }

        public static double PredRanking(double[] predictions, int place)
        {
            ArrayList prediction_sort = new ArrayList(predictions);
			prediction_sort.Sort();
            double hold = Convert.ToDouble(prediction_sort[prediction_sort.Count - place]);

            return hold;
		}

        public static void SaveArrayAsCSV_<T>(T[][] jaggedArrayToSave, string fileName)
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                foreach (T[] array in jaggedArrayToSave)
                {
                    foreach (T item in array)
                    {
                        file.Write(item + ",");
                    }
                    file.Write(Environment.NewLine);
                }
            }
        }


    }
}
