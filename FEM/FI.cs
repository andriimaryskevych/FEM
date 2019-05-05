using System;

namespace FEM
{
    class FI
    {
        // sooo... here I define fi that will evaluate some integer in range from
        // 0 to 19 including both to some double vector size 3
        // Example: fi[0] -> (-1,1,-1)
        // these are needed coordinates for PHI 1 function
        public static double[][] fi = new SquareGenerator(-1, 0, 1).getItInSingleDArray();

        public static double ONE_EIGHT = 0.125;
        public static double ONE_FOURTH = 0.25;

        private static double firstFi(int i, double alpha, double beta, double gamma)
        {
            double result;
            double[] coord = fi[i];
            result = ONE_EIGHT *
                (1 + alpha * coord[0]) *
                (1 + beta * coord[1]) *
                (1 + gamma * coord[2]) *
                (alpha * coord[0] + beta * coord[1] + gamma * coord[2] - 2);

            return result;
        }
        private static double secondFi(int i, double alpha, double beta, double gamma)
        {
            double result;
            double[] coord = fi[i];
            result = ONE_FOURTH *
                (1 + alpha * coord[0]) *
                (1 + beta * coord[1]) *
                (1 + gamma * coord[2]) *
                (1 - Math.Pow((alpha * coord[1] * coord[2]), 2) - Math.Pow((beta * coord[0] * coord[2]), 2) - Math.Pow((gamma * coord[0] * coord[1]), 2));

            return result;
        }
        // calculates PHIi(A,B,G)
        public static double getFi(int i, double alpha, double beta, double gamma)
        {
            return i < 8 ? firstFi(i, alpha, beta, gamma) : secondFi(i, alpha, beta, gamma);
        }

        // functions that calculated deriviates
        public static double diAlphaFirst(int i, double alpha, double beta, double gamma)
        {
            double result;
            double[] coord = fi[i];
            result = ONE_EIGHT *
                coord[0] *
                (1 + beta * coord[1]) *
                (1 + gamma * coord[2]) *
                (2 * alpha * coord[0] + beta * coord[1] + gamma * coord[2] - 1);

            return result;
        }
        public static double diBetaFirst(int i, double alpha, double beta, double gamma)
        {
            double result;
            double[] coord = fi[i];
            result = ONE_EIGHT *
                (1 + alpha * coord[0]) *
                coord[1] *
                (1 + gamma * coord[2]) *
                (alpha * coord[0] + 2 * beta * coord[1] + gamma * coord[2] - 1);

            return result;
        }
        public static double diGammaFirst(int i, double alpha, double beta, double gamma)
        {
            double result;
            double[] coord = fi[i];
            result = ONE_EIGHT *
                (1 + alpha * coord[0]) *
                (1 + beta * coord[1]) *
                coord[2] *
                (alpha * coord[0] + beta * coord[1] + 2 * gamma * coord[2] - 1);

            return result;
        }
        public static double diAlphaSecond(int i, double alpha, double beta, double gamma)
        {
            double result;
            double[] coord = fi[i];
            result = ONE_FOURTH *
                (1 + beta * coord[1]) *
                (1 + gamma * coord[2]) *
                (coord[0] * (1 - Math.Pow((alpha * coord[1] * coord[2]), 2) - Math.Pow((beta * coord[0] * coord[2]), 2) - Math.Pow((gamma * coord[0] * coord[1]), 2)) -
                  2 * alpha * Math.Pow((coord[1] * coord[2]), 2) * (1 + alpha * coord[0]));

            return result;
        }
        public static double diBetaSecond(int i, double alpha, double beta, double gamma)
        {
            double result;
            double[] coord = fi[i];
            result = ONE_FOURTH *
                (1 + alpha * coord[0]) *
                (1 + gamma * coord[2]) *
                (coord[1] * (1 - Math.Pow((alpha * coord[1] * coord[2]), 2) - Math.Pow((beta * coord[0] * coord[2]), 2) - Math.Pow((gamma * coord[0] * coord[1]), 2)) -
                  2 * beta * Math.Pow((coord[0] * coord[2]), 2) * (1 + beta * coord[1]));

            return result;
        }
        public static double diGammaSecond(int i, double alpha, double beta, double gamma)
        {
            double result;
            double[] coord = fi[i];
            result = ONE_FOURTH *
                (1 + alpha * coord[0]) *
                (1 + beta * coord[1]) *
                (coord[2] * (1 - Math.Pow((alpha * coord[1] * coord[2]), 2) - Math.Pow((beta * coord[0] * coord[2]), 2) - Math.Pow((gamma * coord[0] * coord[1]), 2)) -
                  2 * gamma * Math.Pow((coord[0] * coord[1]), 2) * (1 + gamma * coord[2]));

            return result;
        }

        // functinons delegate calculation to more specified functions depending on their index
        private static double diAlpha(int i, double alpha, double beta, double gamma)
        {
            return i < 8 ? diAlphaFirst(i, alpha, beta, gamma) : diAlphaSecond(i, alpha, beta, gamma);
        }
        private static double diBeta(int i, double alpha, double beta, double gamma)
        {
            return i < 8 ? diBetaFirst(i, alpha, beta, gamma) : diBetaSecond(i, alpha, beta, gamma);
        }
        private static double diGamma(int i, double alpha, double beta, double gamma)
        {
            return i < 8 ? diGammaFirst(i, alpha, beta, gamma) : diGammaSecond(i, alpha, beta, gamma);
        }

        // calculates ( Di PHIi(A,B,G) ) / ( Di variable )
        // variable 1 is Aplha and so on
        // it delegates execution depending on alpha beta gamma
        public static double getDiFi(int variable, int i, double alpha, double beta, double gamma)
        {
            double result = 0;

            switch (variable)
            {
                case 0: result = diAlpha(i, alpha, beta, gamma); break;
                case 1: result = diBeta(i, alpha, beta, gamma); break;
                case 2: result = diGamma(i, alpha, beta, gamma); break;
            }

            return result;
        }
    }
}
