using System;
using System.Collections.Generic;
using Microsoft.SolverFoundation.Common;
using Microsoft.SolverFoundation.Services;
using Microsoft.SolverFoundation.Solvers;

namespace portfolio_optimization

{
    public static class Optim
    {
        public static void Fit(float[][] X, int iterations)
        {
            string[] stockNames = { "X1", "X2", "X3", "X4", "X5" };

			//int m = stockNames.Length;

   //         for (int reqIx = 0; reqIx < iterations; reqIx++)
   //         {
   //             InteriorPointSolver solver = new InteriorPointSolver();
   //         }
			//	int[] allocations = new int[m];

			//for (int invest = 0; invest < m; invest++)
			//{
			//	string name = stockNames[invest];
			//	solver.AddVariable(name, out allocations[invest]);
			//	solver.SetBounds(allocations[invest], 0, 1);
			//}
   //         solver
			//int expectedReturn;
			//solver.AddRow("expectedReturn", out expectedReturn);

			//solver.SetBounds(expectedReturn, (double)plan.Rows[reqIx]["minimum"],
            //double.PositiveInfinity);




		}
    }
}
