using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Deedle;
using machinelearning;
using Preprocessing;
using BacktestSystem;


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
				BackTest();
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
        public static void BackTest()
        {

			// Data Preprocessing

			Console.WriteLine("Please enter the start date you want to do the backtest(Format: YYYY-MM-DD)");
			String Date = Console.ReadLine();
            Console.WriteLine("Please enter the weeks you want to do the backtest");
            String Weeks = Console.ReadLine();

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


        }
        public static void Trade()
        {



        }
   
    }
}
