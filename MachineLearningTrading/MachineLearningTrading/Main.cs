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
            Mybacktest.Init();

            List<double> Hisc_netValue = new List<double>();
            List<string> ETFs_holding_FI = new List<string>();
            List<string> ETFs_holding_Equ = new List<string>();

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

                double[] predictions_FI = new double[pro_FI.Trade_ETF.Count];
                double[] predictions_Equ = new double[pro_Equ.Trade_ETF.Count];

                List<string> Blend_ETFs = new List<string>();

                // FI prediction and buying

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


                var hold_FI = PredRanking(predictions_FI,5);

                List<string> ETFs_FI = new List<string>();

                for (int m = 0; m < pro_FI.Trade_ETF.Count; m++)
                {
                    if (predictions_FI[m] >= hold_FI)
                    {   
                        ETFs_FI.Add(pro_FI.Trade_ETF[m]);
                    }
                }

                if (i ==0)
                {
                    ETFs_holding_FI = ETFs_FI;
                }
                else
                {
                    
                    double[] holding_pred = ETFname2Prediction(ETFs_holding_FI, predictions_FI, pro_FI);
                    double[] long_pred = ETFname2Prediction(ETFs_FI, predictions_FI, pro_FI);

                    double trade_diff= long_pred.Sum() - holding_pred.Sum();

                    if (trade_diff < 0.10)
                    {
                        ETFs_FI = ETFs_holding_FI;
                    }
                    else
                    {
                        ETFs_holding_FI = ETFs_FI;
                    }

                    TurnOver += CaculateTurnOver(ETFs_holding_FI, ETFs_FI,0.04);
                }


                // double[] AllocationFI= PO.ETFs2Allocation(ETFs_FI, pro_FI);

                Console.WriteLine("Long the following Fixed Income ETFs: ");
                for (int n = 0; n < ETFs_FI.Count; n++)
                {
                    Console.WriteLine(ETFs_FI[n]);
                    Blend_ETFs.Add(ETFs_FI[n]);
                }

				// Equity ETF predict and buy 


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

                // double[] AllocationEqu = PO.ETFs2Allocation(ETFs_FI, pro_FI);

                Console.WriteLine("Long the following Equity ETFs: ");
				for (int n = 0; n < ETFs_Equ.Count; n++)
				{
					Console.WriteLine(ETFs_Equ[n]);
                    Blend_ETFs.Add(ETFs_Equ[n]);
				}

                //  Update Netvalue

                //double[] allocations = new double[10];

                //for (int fi = 0; fi < 5; fi++)
                //{
                //    allocations[fi] = AllocationFI[fi] * 0.2;
                //}

                //for (int equ = 0; equ < 5; equ++)
                //{
                //    allocations[equ + 5] = AllocationEqu[equ] * 0.8;
                //}

                //foreach (var item in allocations)
                //{
                //    Console.WriteLine(item);
                //}

                double[] EqualAllo = { 0.04,0.04,0.04,0.04,0.04,0.16,0.16,0.16,0.16,0.16};
                
                Hisc_netValue.Add(Mybacktest.Rebalance(Today, Blend_ETFs.ToArray(), EqualAllo));
            
            }


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

            SaveArrayAsCSV(BTmetrics,"BacktestMetrics.csv");
            SaveArrayAsCSV(StrategyNetValue, "Net_value.CSV");


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



    }
}
