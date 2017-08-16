﻿﻿using System;
using System.Collections.Generic;
using Deedle;

namespace Preprocessing
{  

    public class DataPreProcessing
    {
        
		public List<Frame<DateTime, string>> Feature_List = new List<Frame<DateTime, string>>();
		public List<Series<DateTime, double>> Target_List = new List<Series<DateTime, double>>();
		public List<Series<DateTime, double>> ETF_list = new List<Series<DateTime, double>>();
		public List<string> Trade_Index = new List<string>();
		public List<string> Trade_ETF = new List<string>();

        public void Run(string starttime, int weeks,string catagory)
        {


            string[] Index_namelist = Trading_category(catagory);
            string[] Mapping_ETF_namelist = Get_mapping_ETF(Index_namelist);

            var Index_data = Get_DataFromList(Index_namelist, "Index");
            var ETF_data = Get_DataFromList(Mapping_ETF_namelist, "ETF");

            var Adj_Index_data = Adjust_index_fx(Index_data, Index_namelist);
            var Adj_ETF_data = Adjust_etf_fx(ETF_data, Mapping_ETF_namelist);

            var startDate = DateTime.Parse(starttime);
            var endDate = startDate.AddDays(weeks * 7);

            //List<Frame<DateTime, string>> Feature_List = new List<Frame<DateTime, string>>();
            //List<Series<DateTime, double>> Target_List = new List<Series<DateTime, double>>();
            //List<Series<DateTime, double>> ETF_list = new List<Series<DateTime, double>>();
            //List<string> Trade_Index = new List<string>();
            //List<string> Trade_ETF = new List<string>();
           
            for (int i = 0; i < Index_namelist.Length; i++)
            {

                // Check if the Index exist and if the ETF tradable during this period

                double init_value_Index = Index_data[i].Get(startDate);
                double init_value_ETF = ETF_data[i].Get(startDate);

                if (init_value_ETF.Equals(1)  || init_value_Index.Equals(1))
                {
                    continue;

                }
                else
                {
                    var raw_y = Adj_Index_data[i].Between(startDate, endDate);
                    var features = MultiLagFeaturesEng(Price2Return(raw_y)
                                                        .Chunk(7)
                                                       .Select(x => x.Value.Sum())).DropSparseRows();
                    
                    var raw_etf = Price2Return(Adj_ETF_data[i].Between(startDate,endDate))
                                                          .Chunk(7)
                                                          .Select(x => x.Value.Sum());
                    
                    var First_key = features.GetRowKeyAt(0);
                    var y = Price2Return(raw_y).Chunk(7).Select(x => x.Value.Sum()).Between(First_key,endDate);
                    var etf = raw_etf.Between(First_key, endDate);

                    Target_List.Add(y);
                    Feature_List.Add(features);
                    ETF_list.Add(etf);
                    Trade_Index.Add(Index_namelist[i]);
                    Trade_ETF.Add(Mapping_ETF_namelist[i]);

                }

            }

        }


        public static String[] Trading_category(String category)
        {
            if (category == "Fixed Income")
            {
                String[] fi_namelist = { "LECPTREU Index", "BCEX2T Index", "LGCPTRUU Index", "IYJD Index",
                                      "IBOXIG Index","LTITTREU Index","LETSTREU Index","FTFIBGT Index",
                                      "IDCOT1TR Index","BEIG1T Index"};

                return fi_namelist;

            }

            else if (category == "Equity")
            {
                var Index_meta = Frame.ReadCsv("data/Index Metadata.csv");
                var TYP = Index_meta.GetColumn<String>("SECURITY_TYP");
                var Ticker = Index_meta.GetColumn<String>("Ticker");

                List<String> equ_namelist = new List<string>();

                for (int i = 0; i < 135; i++)
                {
                    if (TYP[i] == "Equity Index")
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

                for (int i = 0; i < namelist.Length; i++)
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
                    if (Full_name == namelist[j])
                    {
                        ETF_list[j] = etf;
                    }
                }
            }

            return ETF_list;
        }


        public static List<Series<DateTime, double>> Adjust_index_fx(List<Series<DateTime, double>> data, string[] namelist)

        {
            var FX = Frame.ReadCsv("data/FX Returns.csv").IndexRows<DateTime>("Date");

            var GBPUSD = FX.Rows.Select(x => x
                                         .Value.GetAs<double>("GBPUSD Curncy"))
                                         .FillMissing(Direction.Forward);
            
            var GBPEUR = FX.Rows.Select(x =>
                                        x.Value.GetAs<double>("GBPUSD Curncy") /
                                        x.Value.GetAs<double>("EURUSD")).FillMissing(Direction.Forward);
          
            var Index_meta = Frame.ReadCsv("data/Index Metadata.csv");

			List<Series<DateTime, double>> List_data = new List<Series<DateTime, double>>();

			for (int i = 0; i < namelist.Length; i++)
            {
                var meta = Index_meta.Where(index => index.Value.GetAs<string>("Ticker") == namelist[i]);
                var CRNCY = meta.Rows.Select(x => x.Value.GetAs<string>("CRNCY")).GetAt(0);

                if(CRNCY == "USD")
                {   
                    List_data.Add(data[i] / GBPUSD); 
                }
                else if(CRNCY == "EUR")
                {
                    List_data.Add(data[i] / GBPEUR);
                }
                else
                {
					List_data.Add(data[i]);
                }

            }

            return List_data;
        }


        public static List<Series<DateTime, double>> Adjust_etf_fx(List<Series<DateTime, double>> data, string[] namelist)
		{
			var FX = Frame.ReadCsv("data/FX Returns.csv").IndexRows<DateTime>("Date");

			var GBPUSD = FX.Rows.Select(x =>
										x.Value.GetAs<double>("GBPUSD Curncy")).FillMissing(Direction.Forward);

			var GBPEUR = FX.Rows.Select(x =>
										x.Value.GetAs<double>("GBPUSD Curncy") /
										x.Value.GetAs<double>("EURUSD")).FillMissing(Direction.Forward);

			var ETF_meta = Frame.ReadCsv("data/ETF Metadata.csv");

			List<Series<DateTime, double>> List_data = new List<Series<DateTime, double>>();

			for (int i = 0; i < namelist.Length; i++)
			{
                var meta = ETF_meta.Where(index => index.Value.GetAs<string>("Ticker") == namelist[i]);
				var CRNCY = meta.Rows.Select(x => x.Value.GetAs<string>("CRNCY")).GetAt(0);

				if (CRNCY == "USD")
				{
					List_data.Add(data[i] / GBPUSD);
				}
				else if (CRNCY == "EUR")
				{
					List_data.Add(data[i] / GBPEUR);
				}
				else
				{
					List_data.Add(data[i]);
				}

			}

			return List_data;
		}


        public static Series<DateTime,double> Price2Return(Series<DateTime, double> data, string Return_type="log")
        {
            if (Return_type =="log")
            {   
                
				return (data / data.Shift(1)).Log();
            }

                var relative_return = (data / data.Shift(1)).Diff(1);
                return relative_return;

        }


        public static Frame<DateTime,string> MultiLagFeaturesEng(Series<DateTime,double> data)
        {
           
            return Features_engineering(data,1)
                .Join(Features_engineering(data,2), JoinKind.Inner)
                .Join(Features_engineering(data,3),JoinKind.Inner);

        }

        public static List<DateTime> Resample_key(string starttime, int weeks)
        {
            List<DateTime> keys =new List<DateTime>();

            var startDate = DateTime.Parse(starttime);
            keys.Add(startDate);

            for (int i = 1; i < weeks+1; i++)
            {
                var date = startDate.AddDays(7*i);      
                keys.Add(date);
            }

            return keys;
        }


        public static Frame<DateTime, string> Features_engineering(Series<DateTime, double> input, int shiftn)
        {   
            var data = input.Shift(shiftn);

            var MA3 = data.Window(3).Select(x => x.Value.Mean());
            var MA5 = data.Window(5).Select(x => x.Value.Mean());
            var MA10 = data.Window(10).Select(x => x.Value.Mean());

			var diffMA3MA5 = (MA3 - MA5).Select(x => x.Value.CompareTo(0)); 
            var diffMA3MA10 = (MA3 - MA10).Select(x => x.Value.CompareTo(0));
			var diffMA5MA10 = (MA5 - MA10).Select(x => x.Value.CompareTo(0)); 

            var SD3 = data.Window(3).Select(x => x.Value.StdDev());
            var SD5 = data.Window(5).Select(x => x.Value.StdDev());
            var SD10 = data.Window(10).Select(x => x.Value.StdDev());

			var RSI3 = data.Window(3).Select(x => RSI_func(x, 3));
			var RSI5 = data.Window(5).Select(x => RSI_func(x, 5));
			var RSI10 = data.Window(10).Select(x => RSI_func(x, 10));

			var K3 = data.Window(3).Select(x => K_func(x, 3));
			var K5 = data.Window(5).Select(x => K_func(x, 5));
			var K10 = data.Window(10).Select(x => K_func(x, 10));

            string MA3column = "MA3" + Convert.ToString(shiftn);
            string MA5column = "MA5" + Convert.ToString(shiftn);
            string MA10column = "MA10" + Convert.ToString(shiftn);

            string SD3column = "SD3" + Convert.ToString(shiftn);
			string SD5column = "SD5" + Convert.ToString(shiftn);
			string SD10column = "SD10" + Convert.ToString(shiftn);

            string Compare1column = "MA3MA5" +Convert.ToString(shiftn);
			string Compare2column = "MA3MA10" + Convert.ToString(shiftn);
			string Compare3column = "MA5MA10" + Convert.ToString(shiftn);

            string RSI3column = "RSI3" + Convert.ToString(shiftn);
            string RSI5column = "RSI5" + Convert.ToString(shiftn);
            string RSI10column = "RSI10" + Convert.ToString(shiftn);

            string K3column = "K3" + Convert.ToString(shiftn);
            string K5column = "K5" + Convert.ToString(shiftn);
            string K10column = "K10" + Convert.ToString(shiftn);

            var Features = new FrameBuilder.Columns<DateTime, string>{
                { MA3column  , MA3  },
                { MA5column  , MA5  },
                { MA10column , MA10 },
                {Compare1column, diffMA3MA5},
                {Compare2column,diffMA3MA10},
                {Compare3column,diffMA5MA10},
                {SD3column,SD3},
                {SD5column,SD5},
                {SD10column,SD10},
                {RSI3column, RSI3},
                {RSI5column, RSI5},
                {RSI10column, RSI10}, 
                {K3column,K3},
                {K5column,K5},
                {K10column,K10}

            }.Frame;

          
            return Features;
         
		}


        public static double RSI_func(KeyValuePair<DateTime,Series<DateTime,double>> x, int len)
        {   
            double positive=0, negative=0;
            int pos_num = 0, neg_num = 0;


            for (int i = 0; i < len; i++)
            {
               var item = x.Value.GetAt(i);

                if(item > 0)
                {
                    positive = positive + item;
                    pos_num++;
                }
                else if(item < 0)
                {
                    negative = negative - item;
                    neg_num++;
                }
            }

			if (pos_num == 0 && neg_num == 0)
			{
				double RS = 0;
				double RSI = 100 - 100 / (1 + RS);
				return RSI;
			}
			else if (pos_num == 0)
			{
				double RS = 0;
				double RSI = 100 - 100 / (1 + RS);
				return RSI;

			}
			else if (neg_num == 0)
			{
				double RS = 100;
				double RSI = 100 - 100 / (1 + RS);
				return RSI;
			}
			else
			{
				double RS = (positive / pos_num) / (negative / neg_num);
				double RSI = 100 - 100 / (1 + RS);
				return RSI;
			}

        }


        public static double K_func(KeyValuePair<DateTime, Series<DateTime, double>> x, int len)
        {
            double init = 1;

            double[] list = new double[len+1];
            list[0] = init;

            double min_item = 100, max_item = -100;

            for (int i = 0; i < len; i++)
            {   
                
                var item = list[i] * Math.Exp(x.Value.GetAt(i));
                if (item > max_item)
                {
                    max_item = item;
                }

                if (item < min_item)
                {
                    min_item = item;
                }

                list[i+1] = item;
            }

            double K = (list[len] - min_item) / (max_item - min_item);
            return K;
        }


    }

}
