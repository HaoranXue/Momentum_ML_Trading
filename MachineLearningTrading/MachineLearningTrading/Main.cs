using System;
using System.IO;
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
			DataPreProcessing.Run();
			Console.ReadKey();



			//Learning_momentum
			//Learning.Run();

		}
        public static void Trade()
        {



        }
   
    }
}
