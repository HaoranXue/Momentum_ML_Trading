using System;
using Deedle;
using XGBoost;
using Accord.MachineLearning;

namespace machinelearning
{
    public static class Learning
    {
        public static void Run()
        {
            
            float[] learningrate = new float[] { 0.01f, 0.5f, 1, 10 };
            int[] estimator = new int[] { 100, 500, 1000 };
            int[] max_depth = new int[] { 3, 5, 10, 15, 20 };
            float[] sample_tree = new float[] { 0.3f, 0.5f, 0.75f, 1 };
            float[] sample_level = new float[] { 0.5f, 0.75f };
            float[] reg_lamda = new float[] { 0.01f, 1, 10, 1000 };


            for (int i = 0; i < learningrate.Length; i++)
            {
                for (int j = 0; j < estimator.Length; j++)
                {
                    for (int k = 0; k < max_depth.Length; k++)
                    {
                        for (int l = 0; l < sample_tree.Length; l++)
                        {
                            for (int m = 0; m < sample_level.Length; m++)
                            {
                                for (int n = 0; n < reg_lamda.Length; n++)
                                {


                                    var model = new XGBClassifier(nThread: 4);







                                }
                            }
                        }
                    }
                }

            }





        }
    }
}