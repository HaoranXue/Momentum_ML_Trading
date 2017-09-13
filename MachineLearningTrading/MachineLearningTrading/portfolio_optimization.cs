using System;
using System.Collections.Generic;
using MathNet.Numerics.Optimization;
using Preprocessing;
using Deedle;


namespace portfolio_optimization

{
    public static class PO
    {
        public static void ETFs2Allocation(List<string> ETFs,  DataPreProcessing pro)
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

                        ETF_Hisc[i] = pro.ETF_list[j];

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
                    ETF_Hisc_arrary[i][j] = ETF_Hisc[i].GetAt(j);
                
                }

            }

			// Optimization 










		}
    }
}
