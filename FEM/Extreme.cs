using System;
using Extreme.Mathematics;
using Extreme.Mathematics.LinearAlgebra;
using Extreme.Mathematics.LinearAlgebra.IterativeSolvers;
using Extreme.Mathematics.LinearAlgebra.IterativeSolvers.Preconditioners;

namespace FEM
{
    class Extreme
    {
        public static double[] Solve(double[,] A, double[] b)
        {
            int size = b.Length;

            var MG = Matrix.CreateSparse<double>(size, size);
            var F = Vector.CreateSparse<double>(size);

            for (int i = 0; i < size; i++) {
                for(int j = 0; j < size; j++) {
                    MG[i,j] = A[i,j];
                }
            }

            for(int i = 0; i < size; i++) {
                F[i] = b[i];
            }

            IterativeSparseSolver<double> solver = new BiConjugateGradientSolver<double>(MG);

            return solver.Solve(F).ToArray();
        }
    }
}
