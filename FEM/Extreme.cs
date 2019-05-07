using System;
using Extreme.Mathematics;
using Extreme.Mathematics.LinearAlgebra;
using Extreme.Mathematics.LinearAlgebra.IterativeSolvers;
using Extreme.Mathematics.LinearAlgebra.IterativeSolvers.Preconditioners;

namespace FEM
{
    class Extreme
    {
        public static double[] Solve(SparseCompressedColumnMatrix<double> A, SparseVector<double> b)
        {
            Console.WriteLine("Start Ax=B");
            IterativeSparseSolver<double> solver = new BiConjugateGradientSolver<double>(A);
            Console.WriteLine("End Ax=B");

            return solver.Solve(b).ToArray();
        }
    }
}
