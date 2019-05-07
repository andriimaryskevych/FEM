using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;
using System;

namespace FEM
{
    class Iterative
    {
        public static double[] Solve(double[,] A, double[] b)
        {
            var matrixA = SparseMatrix.OfArray(A);
            var vectorB = SparseVector.OfEnumerable(b);

            var iterationCountStopCriterion = new IterationCountStopCriterion<double>(1000);
            var residualStopCriterion = new ResidualStopCriterion<double>(1e-10);
            var monitor = new Iterator<double>(iterationCountStopCriterion, residualStopCriterion);

            var solver = new CompositeSolver(new [] {new UserBiCgStab()});

            var resultX = matrixA.SolveIterative(vectorB, solver, monitor).ToArray();

            return resultX;
        }
    }

    public class UserBiCgStab : IIterativeSolverSetup<double>
    {
        public Type SolverType
        {
            get { return null; }
        }

        public Type PreconditionerType
        {
            get { return null; }
        }

        public IIterativeSolver<double> CreateSolver()
        {
            return new BiCgStab();
        }

        public IPreconditioner<double> CreatePreconditioner()
        {
            return new ILU0Preconditioner();
        }

        public double SolutionSpeed
        {
            get { return 0.99; }
        }

        public double Reliability
        {
            get { return 0.99; }
        }
    }
}
