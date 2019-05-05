using System.Collections.Generic;

namespace FEM
{
    class SquareGenerator
    {
        private double[] value;
        private double[,,][] matrix;
        private Dictionary<int, int[]> adapter = Globals.magicDictionary;
        // c-tor accepts 3 parameters: lower, middle and higher bounds
        // -1 , 0 , 1 will create 27 nodes [3][3][3]
        // [0][0][0] of which would be equal to new Point(-1,1,-1)
        public SquareGenerator(double lower, double middle, double higher)
        {
            value = new double[] { lower, middle, higher };
        }

        public double[,,][] getMatrix()
        {
            matrix = new double[3, 3, 3][];
            for (int z = 0; z < 3; z++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        matrix[x, y, z] = new double[] { value[x], value[2 - y], value[z] };
                        //Console.WriteLine($"{value[k]}, {value[2 - j]}, {value[i]}");
                    }
                }
            }

            return matrix;
        }

        public double[][] getItInSingleDArray()
        {
            getMatrix();
            double[][] oneline = new double[20][];
            int[] point;
            for (int i = 0; i < 20; i++)
            {
                point = adapter[i];
                oneline[i] = matrix[point[0], point[1], point[2]];
            }
            return oneline;
        }
    }
}
