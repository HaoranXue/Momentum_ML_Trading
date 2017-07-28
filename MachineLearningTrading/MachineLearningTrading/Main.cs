using System;
using Preprocessing;
using System.IO;

namespace MLtrading
{
    class MainClass
    {
        public static void Main(string[] args)
        {
			Directory.SetCurrentDirectory("/Users/xuehaoran/Documents/GitHub/Machine_Learning_trading-Project/MachineLearningTrading/MachineLearningTrading");

			// Data Preprocessing
			DataPreProcessing.Run();
            Console.ReadKey();

        }
    }
}
