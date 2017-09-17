using System;
using System.IO;
using System.Collections.Generic;
using RDotNet;
using Preprocessing;
using Deedle;


namespace portfolio_optimization

{
    public static class PO
    {
        public static double[] ETFs2Allocation(List<string> ETFs,  DataPreProcessing pro)
        {




            // Get the history data 

            List<Series<DateTime, double>> ETF_Hisc = new List<Series<DateTime, double>>() ;

            for (int i = 0; i < ETFs.Count; i++)
            {
                string ETFname = ETFs[i];

                for (int j = 0; j < pro.Trade_ETF.Count; j++)
                {
                    if (ETFname == pro.Trade_ETF[j])
                    {

                        ETF_Hisc.Add(pro.ETF_list[j]);

                    }
                }
            }



            // Transfer list<series> to array 

            double[][] ETF_Hisc_arrary = new double[ETF_Hisc.Count][];

            for (int i = 0; i < ETF_Hisc.Count; i++)
            {   
                
                var len = ETF_Hisc[i].ValueCount;
                ETF_Hisc_arrary[i] = new double[len];

                for (int j = 0; j < len; j++)
                {
                    ETF_Hisc_arrary[i][j] = Math.Exp(ETF_Hisc[i].GetAt(j)) -1;
                
                }

            }

            SaveArrayAsCSV(ETF_Hisc_arrary, "Hisc_array.csv");
            // Optimization 

            REngine.SetEnvironmentVariables(); // <-- May be omitted; the next line would call it.
            REngine engine = REngine.GetInstance();

            engine.Evaluate("library(PortfolioAnalytics) ");
            
            engine.Evaluate("raw_data <- t(read.csv('Hisc_array.csv',header = FALSE))[-101,]");
            engine.Evaluate("data <- matrix(vector(),1000,5)");
            engine.Evaluate("for(i in 1:5){data[,i] = sample(raw_data[,i], 1000,replace = TRUE)}");

            engine.Evaluate("rownames(data) <- seq(1,1000,1)");
            engine.Evaluate("colnames(data) <- c('x1','x2','x3','x4','x5')");

            engine.Evaluate("portfolio <- portfolio.spec(colnames(data)) ");

            engine.Evaluate("portfolio <- add.constraint(portfolio,  type = 'weighted_sum',min_sum=0.99, max_sum=1.01)");
            engine.Evaluate("portfolio <- add.constraint(portfolio, type = 'box', min = 0.05, max = 0.3)");

            engine.Evaluate("portfolio <- add.objective(portfolio=portfolio, type='risk', name='VaR',arguments = list(p = 0.99, method = 'historical', portfolio_method = 'component'), enabled = TRUE)");
            engine.Evaluate("result<- optimize.portfolio(as.ts(data), portfolio = portfolio, search_size = 2000, trace=TRUE, traceDE=5)");

            double[] allocations = engine.Evaluate("result$weights").AsNumeric().ToArray();

            return allocations;
          
        }

        public static void SaveArrayAsCSV<T>(T[][] jaggedArrayToSave, string fileName)
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
