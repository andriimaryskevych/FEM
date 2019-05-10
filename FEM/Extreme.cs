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
            return new BiConjugateGradientSolver<double>(A).Solve(b).ToArray();
        }
    }
}
