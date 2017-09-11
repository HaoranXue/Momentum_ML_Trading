using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Deedle;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.LinearAlgebra;
using machinelearning;
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

                var StrategyNetValue = BackTest( Date, Weeks).ToArray();

                double MaxDD = 0; 

                for (int i = 1; i < StrategyNetValue.Length; i++)
                {
                    var MaxNetValue= StrategyNetValue.Take(i).Max();
                    double drawdown = 1-StrategyNetValue[i] / MaxNetValue;

                    if (drawdown > MaxDD)
                    {
                        MaxDD = drawdown;
                    }
                }

                Console.WriteLine("Maximum drawdown of This Strategy is: {0}",MaxDD);

                var AnnualReturn= Math.Log(StrategyNetValue.Last()) /
                                                            (Convert.ToDouble(Weeks)/50);

                Console.WriteLine("Annual Return of This Strategy is: {0}",AnnualReturn);

                var StrategyReturn = NetValue2Return(StrategyNetValue);
                Console.WriteLine("Standard Deviation of This Strategy is: {0}",
                                          Statistics.StandardDeviation(StrategyReturn));
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
        public static List<double> BackTest(string Date, string Weeks)
        {

			// Data Preprocessing

            var Mybacktest = new Backtest();
            Mybacktest.init();

            List<double> Hisc_netValue = new List<double>();
            List<string> ETFs_holding = new List<string>();
            double TurnOver = 0;

            for (int i = 0; i < Convert.ToInt64(Weeks); i++)
            {   

                // seting Datapreprocessing class 
                var pro = new DataPreProcessing();
                var Today = DateTime.Parse(Date).AddDays(i*7);

                Console.WriteLine("Date: {0}", Today.ToString());

                pro.Run(Today.ToString(), 112, "Equity");

                double[] predictions = new double[pro.Trade_ETF.Count];

                for (int j = 0; j < pro.Trade_ETF.Count; j++)
                {

                    var y = pro.Target_List[j];

                    var fy = new FrameBuilder.Columns<DateTime, string>{
                     { "Y"  , y  }
                    }.Frame;
                    var data = pro.Feature_List[j].Join(fy);
                    var pred_Features = pro.pred_Feature_List[j];

                    data.SaveCsv("dataset.csv");

                    var prediction = Learning.Fit(pred_Features);

                    predictions[j] = prediction;
                }

                ArrayList prediction_sort = new ArrayList(predictions);
                prediction_sort.Sort();

                double hold = Convert.ToDouble(prediction_sort[prediction_sort.Count - 5]);

                List<string> ETFs = new List<string>();

                for (int m = 0; m < pro.Trade_ETF.Count; m++)
                {
                    if (predictions[m] >= hold)
                    {   
                        ETFs.Add(pro.Trade_ETF[m]);
                    }
                }

                if (i ==0)
                {
                    ETFs_holding = ETFs;
                }
                else
                {
                    var ETF_except = ETFs_holding.Except(ETFs);

                    TurnOver += ETF_except.ToArray().GetLength(0) * 0.2;

                    double[] holding_pred = ETFname2Prediction(ETFs_holding, predictions, pro);
                    double[] long_pred = ETFname2Prediction(ETFs, predictions, pro);

                    double trade_diff= long_pred.Sum() - holding_pred.Sum();

                    if (trade_diff < 0.10)
                    {
                        ETFs = ETFs_holding;
                    }
                    else
                    {
                        ETFs_holding = ETFs;
                    }
                }

                Console.WriteLine("Long the following ETFs: ");
                for (int n = 0; n < ETFs.Count; n++)
                {
                    Console.WriteLine(ETFs[n]);
                }

                double[] allocations = { 0.2, 0.2, 0.2, 0.2, 0.2 };
                Hisc_netValue.Add(Mybacktest.rebalance(Today, ETFs.ToArray(), allocations));
            }

           Console.WriteLine("Overall TurnOver: {0}", TurnOver);

           return Hisc_netValue;

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
        
        public static double CaculateTurnOver(string[] holding_ETFs, string[] new_ETFs)
        {

            
        }

    }
}
