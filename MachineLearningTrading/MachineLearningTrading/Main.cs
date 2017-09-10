using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Deedle;
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
                    double drawdown = StrategyNetValue[i] / MaxNetValue;

                    if (drawdown > MaxDD)
                    {
                        MaxDD = drawdown;
                    }
                }

                Console.WriteLine("Maximum drawdown of This Strategy is: {2}", MaxDD);

                var AnnualReturn= Math.Log(StrategyNetValue.Last()) /
                                                         (Convert.ToDouble(Weeks)/50);

                Console.WriteLine("Annual Return of This Strategy is: {2}", AnnualReturn);

                Console.ReadKey();
            }
            else if (Order =="1")
            {
                Trade();
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Error of setting trade or backtet. ");

            }

        }
        public static List<double> BackTest(string Date, string Weeks)
        {

			// Data Preprocessing

            var Mybacktest = new Backtest();
            Mybacktest.init();

            List<double> Hisc_netValue = new List<double>();

            for (int i = 0; i < Convert.ToInt64(Weeks); i++)
            {
                Console.WriteLine("ETF going to buy for next week.");
                // seting Datapreprocessing class 
                var pro = new DataPreProcessing();
                var Today = DateTime.Parse(Date).AddDays(i*7);

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
                        Console.WriteLine(pro.Trade_ETF[m]);
                        ETFs.Add(pro.Trade_ETF[m]);
                    }
                }

                double[] allocations = { 0.2, 0.2, 0.2, 0.2, 0.2 };
                Hisc_netValue.Add(Mybacktest.rebalance(Today, ETFs.ToArray(), allocations));
            }

           return Hisc_netValue;

        }
        public static void Trade()
        {



        }
   
    }
}
