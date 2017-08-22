using System;
using System.IO;
using SharpLearning.GradientBoost.Learners;
using SharpLearning.InputOutput.Csv;

namespace machinelearning
{
    public static class Learning
    {
        public static double Fit(double[] pred_Features)
        {
   
            var parser = new CsvParser(() => new StreamReader("dataset.csv"),separator:',');
            var targetName = "Y";

			var observations = parser.EnumerateRows(c => c != targetName)
		    .ToF64Matrix();

			var targets = parser.EnumerateRows(targetName)
				.ToF64Vector();
            // read regression targets

            var learner = new RegressionHuberLossGradientBoostLearner();
			var model = learner.Learn(observations, targets);
            var prediction = model.Predict(pred_Features);

            return prediction;
        }


        public static float[][] Trans_x(double[,] X)
        {

			var dim0 = X.GetLength(0);
			var dim1 = X.GetLength(1);


            float[][] X_trans = new float[dim0][];

			for (int i = 0; i < dim0; i++)
			{
                X_trans[i] = new float[dim1];

				for (int j = 0; j < dim1; j++)
				{
                      X_trans[i][j] = (float) X[i, j];
                }
			}

            return X_trans;
		}

        public static float[] Trans_y(double[,] X)
		{

			var dim0 = X.GetLength(0);
			var dim1 = X.GetLength(1);


            float[] X_trans = new float[dim0];

			for (int i = 0; i < dim0; i++)
			{

                if (X[i, 0] > 0 )
                {
                    X_trans[i]=1f;
                }

                else
                {
                    X_trans[i] = 0f;
                }

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