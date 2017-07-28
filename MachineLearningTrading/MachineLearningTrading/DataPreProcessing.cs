﻿﻿using System;
using System.Collections.Generic;
using Deedle;

namespace Preprocessing
{

	public static class DataPreProcessing
    {
        
        public static void Run()
        {

            Console.WriteLine("Trading Fixed Income or Equity Strategy ?");
            String catagory = Console.ReadLine();

            String[] Index_list = Trading_category(catagory);
            String[] Mapping_ETF_list = Get_mapping_ETF(Index_list);

            var Index_data = Get_DataFromList(Index_list, "Index");
            var ETF_data = Get_DataFromList(Mapping_ETF_list, "ETF");

            Adjust_fx(ETF_data,Mapping_ETF_list);

            Index_data[0].Print();

   		}


        public static String[] Trading_category(String category)
        {
            if(category == "Fixed Income")
            {
				String[] fi_namelist = { "LECPTREU Index", "BCEX2T Index", "LGCPTRUU Index", "IYJD Index",
	                                  "IBOXIG Index","LTITTREU Index","LETSTREU Index","FTFIBGT Index",
	                                  "IDCOT1TR Index","BEIG1T Index"};
                
                return fi_namelist;

            }

            else if(category == "Equity")
            {
                var Index_meta = Frame.ReadCsv("data/Index Metadata.csv");
                var TYP = Index_meta.GetColumn<String>("SECURITY_TYP");
                var Ticker = Index_meta.GetColumn<String>("Ticker");

                List<String> equ_namelist = new List<string>();

                for(int i =0; i < 135; i++)
                {
                 if(TYP[i] == "Equity Index")
                    {
                        equ_namelist.Add(Ticker[i]);  
                    }
                }

                return equ_namelist.ToArray();
			}

            else
            {
                Console.WriteLine("Error: Input should be Fixed Income or Equity, Using default fixed income as category.");

				String[] fi_namelist = { "LECPTREU Index", "BCEX2T Index", "LGCPTRUU Index", "IYJD Index",
									  "IBOXIG Index","LTITTREU Index","LETSTREU Index","FTFIBGT Index",
									  "IDCOT1TR Index","BEIG1T Index"};
                
				return fi_namelist;
            }
        }


        public static List<Series<DateTime, double>> Get_DataFromList(String[] namelist, string tag)

        {  

            if (tag == "Index")
            {
                var Data = Frame.ReadCsv("data/Index Returns.csv").IndexRows<DateTime>("Date");
                List<Series<DateTime, double>> List_data = new List<Series<DateTime, double>>();

                for (int i=0; i < namelist.Length; i++)
                {
					var data = Data.Rows.Select(x => x.Value.GetAs<double>(namelist[i]));
					List_data.Add(data);
                }

                return List_data;
			}
            else
            {
                var Data = Frame.ReadCsv("data/ETF Returns.csv").IndexRows<DateTime>("Date");
                List<Series<DateTime, double>> List_data = new List<Series<DateTime, double>>();

				for (int i = 0; i < namelist.Length; i++)
				{
                    var data = Data.Rows.Select(x => x.Value.GetAs<double>(namelist[i]));
					List_data.Add(data);
				}

				return List_data;
			}
        }


        public static string[] Get_mapping_ETF(string[] namelist)
        {
            var Mapping_table = Frame.ReadCsv("data/Mapping_Table.csv");
            var Tracking_ETF = Mapping_table.GetColumn<String>("Best Tracking ETF");
            var Index = Mapping_table.GetColumn<String>("Index");

            string[] ETF_list = new String[namelist.Length];

            for (int i = 0; i < 133; i++)
            {
                var index = Index[i];
                var etf = Tracking_ETF[i];

                for (int j = 0; j < namelist.Length; j++)
                {   
                    string Full_name = index + " Index";
                    if(Full_name == namelist[j])
                    {
                        ETF_list[j] = etf;
                    }
                }
            }

            return ETF_list;
        }

        public static void Adjust_fx(List<Series<DateTime, double>> data, string[] namelist)
        {
            var FX = Frame.ReadCsv("data/FX Returns.csv").IndexRows<DateTime>("Date");

            var GBPUSD = FX.Rows.Select(x => 
                                        x.Value.GetAs<double>("GBPUSD Curncy")).FillMissing(Direction.Forward);

            var GBPEUR = FX.Rows.Select(x =>
                                        x.Value.GetAs<double>("GBPUSD Curncy")/
                                        x.Value.GetAs<double>("EURUSD")).FillMissing(Direction.Forward);
            
			var Index_meta = Frame.ReadCsv("data/Index Metadata.csv");
            var ETF_meta = Frame.ReadCsv("data/ETF Metadata.csv");

            var Index_Ticker = Index_meta.GetColumn<String>("Ticker");
            var ETF_Ticker = ETF_meta.GetColumn<String>("Ticker");

            //foreach(string x in namelist)
            //{
			
				




            //}


        }


    }
}

