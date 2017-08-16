using System;
using System.IO;
using System.Linq;
//using Accord.MachineLearning;
using Deedle;
using Accord;
using Accord.IO;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Statistics;
using Accord.Statistics.Analysis;
using Accord.Statistics.Kernels;
using AForge;
using SharpLearning.DecisionTrees.Learners;
using SharpLearning.AdaBoost.Learners;
using SharpLearning.Containers;
using SharpLearning.InputOutput.Csv;

namespace machinelearning
{
    public static class Learning
    {
        public static void Fit(double[,] X, double[,] y)
        {
            
            var parser = new CsvParser(() => new StreamReader("dataset.csv"),separator:',');
            var targetName = "Y";

			
            
			var observations = parser.EnumerateRows(c => c != targetName)
		    .ToF64Matrix();

			var targets = parser.EnumerateRows(targetName)
				.ToF64Vector();
            // read regression targets

            var learner2 = new RegressionAdaBoostLearner();
			var learner = new RegressionDecisionTreeLearner(maximumTreeDepth: 5);

			// learns a RegressionRandomForestModel
			var model = learner.Learn(observations, targets);
            var prediction = model.Predict(observations);
		}


        public static double[][] Trans_x(double[,] X)
        {

			var dim0 = X.GetLength(0);
			var dim1 = X.GetLength(1);


            double[][] X_trans = new double[dim0][];

			for (int i = 0; i < dim0; i++)
			{
                X_trans[i] = new double[dim1];

				for (int j = 0; j < dim1; j++)
				{
                      X_trans[i][j] = X[i, j];
                }
			}

            return X_trans;
		}

        public static bool[] Trans_y(double[,] X)
		{

			var dim0 = X.GetLength(0);
			var dim1 = X.GetLength(1);


            bool[] X_trans = new bool[dim0];

			for (int i = 0; i < dim0; i++)
			{

                if (X[i, 0] > 0 )
                {
                    X_trans[i]=true;
                }

                else
                {
                    X_trans[i] = false;
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