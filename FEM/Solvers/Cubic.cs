using System;
using System.Numerics;
using MathNet.Numerics;


namespace FEM.Solvers
{
    class Cubic
    {
        public static double[] Solve(double a, double b, double c, double d)
        {
            var roots = FindRoots.Cubic(d, c, b, a);

            Complex root1 = roots.Item1;
            Complex root2 = roots.Item2;
            Complex root3 = roots.Item3;

            double[] res = new double[3] {
                root1.Real,
                root2.Real,
                root3.Real
            };

            return res;
        }
    }
}
