using System;
using System.IO;
using Deedle;
using machinelearning;
using Preprocessing;

namespace MLtrading
{
    class MainClass
    {
        public static void Main(string[] args)
        {
			Directory.SetCurrentDirectory("/Users/xuehaoran/Documents/GitHub/Machine_Learning_trading-Project/MachineLearningTrading/MachineLearningTrading");

            Console.WriteLine("Enter 0 for backtest the strategy, enter 1 for trading via the strategy");
            string Order = Console.ReadLine();

            if (Order =="0")
            {
				BackTest();
            }
            else if (Order =="1")
            {
                Trade();
            }
            else
            {
                Console.WriteLine("Error of setting trade or backtet. ");

            }




        }
        public static void BackTest()
        {

			// Data Preprocessing

			Console.WriteLine("Trading Fixed Income or Equity Strategy ?");
			String catagory = Console.ReadLine();

            var pro = new DataPreProcessing();
            pro.Run("2014-01-01",100,catagory);


            for (int i = 0; i < pro.Trade_ETF.Count; i++)
            {

                var features_array = pro.Feature_List[i].ToArray2D<double>();
                var y = pro.Target_List[i];
                var fy = new FrameBuilder.Columns<DateTime, string>{
                     { "Y"  , y  }
                }.Frame;

                var ary = fy.ToArray2D<double>();

                var data = pro.Feature_List[i].Join(fy);
                data.SaveCsv("dataset.csv");

                Console.WriteLine(features_array.GetLength(0));
                Learning.Fit(features_array, ary);

            }
 

            Console.ReadKey();

			//Learning_momentum
			//Learning.Run();

		}
        public static void Trade()
        {



        }
   
    }
}
