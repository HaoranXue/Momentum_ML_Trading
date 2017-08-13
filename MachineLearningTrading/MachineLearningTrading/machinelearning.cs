using System;
using System.Linq;
//using Accord.MachineLearning;
using Deedle;
using XGBoost;

namespace machinelearning
{
    public static class Learning
    {
        public static XGBClassifier Fit(float[,] X, float[,] y)
        {
           

            var model = new XGBClassifier(nEstimators: 1000, 
                                          colSampleByTree: 0.5f, 
                                          colSampleByLevel: 0.5f,
                                          regLambda: 10);

            var X_trans = Trans_x(X);
            var y_trans = Trans_y(y);

            model.Fit(X_trans, y_trans);

            return model;


            //var model = new XGBRegressor(learningRate: 0.001f);

            //model.Fit(X,y);
            //var prediction = model.Predict(X);

            //Console.WriteLine(prediction);

            //for (int i = 0; i < learningrate.Length; i++)
            //{
            //    for (int j = 0; j < estimator.Length; j++)
            //    {
            //        for (int k = 0; k < max_depth.Length; k++)
            //        {
            //            for (int l = 0; l < sample_tree.Length; l++)
            //            {
            //                for (int m = 0; m < sample_level.Length; m++)
            //                {
            //                    for (int n = 0; n < reg_lamda.Length; n++)
            //                    {


            //                        var model = new XGBClassifier();

            //                    }
            //                }
            //            }
            //        }
            //    }

            //}

        }
        public static float[][] Trans_x(float[,] X)
        {

			var dim0 = X.GetLength(0);
			var dim1 = X.GetLength(1);


			float[][] X_trans = new float[dim0][];

			for (int i = 0; i < dim0; i++)
			{
				for (int j = 0; j < dim1; j++)
				{

					X_trans[i][j] = X[i, j];


				}
			}

            return X_trans;
		}

		public static float[] Trans_y(float[,] X)
		{

			var dim0 = X.GetLength(0);
			var dim1 = X.GetLength(1);


			float[] X_trans = new float[dim0];

			for (int i = 0; i < dim0; i++)
			{
				

					X_trans[i] = X[i, 0];


				
			}

			return X_trans;
		}



		public static float GetRandomNumber(double minimum, double maximum)
		{
			Random random = new Random();
            var input = random.NextDouble() * (maximum - minimum) + minimum;

			float result = (float)input;
			if (float.IsPositiveInfinity(result))
			{
				result = float.MaxValue;
			}
			else if (float.IsNegativeInfinity(result))
			{
				result = float.MinValue;
			}

            return result ;
		}
    }
}