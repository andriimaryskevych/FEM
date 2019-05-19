using FEM.DTO;
using System.Collections.Generic;

namespace FEM.Helpers
{
    class MeshCreator
    {
        private Dictionary<int, int[]> adapter = Globals.magicDictionary;

        private int x, y, z, m, n, k, nqp, nel;

        private double SCALE_X, SCALE_Y, SCALE_Z;

        // this one holds all the points
        // when i devide a kube in 2x2x2 parts it's size beacames 5x5x5 * 3
        // to use only primitives, i decidet to store 5x5x5 vectors of length 3
        // this vectors hold exact position of each vertex in 3d space
        private double[,,][] matrix;

        // I changed order here:
        // As first argument I set global number and as a second one i set vector of lenght 3 that contains exact point coord
        private double[][] AKT;

        // Same as AKT, FE number goes first than it's local value that evaluates to global coord of selected FE
        private int[][] NT;

        // This is private helper matrix
        // it's purpose is to store global coord related to matrix sizes coord (I can't explain better, just look at createAKT))) )
        private int[,,] local_to_global;

        public MeshCreator(Parameters parameters) {
            x = parameters.sizeX;
            y = parameters.sizeY;
            z = parameters.sizeZ;

            m = parameters.xAxisFEMCount;
            n = parameters.yAxisFEMCount;
            k = parameters.zAxisFEMCount;

            SCALE_X = (double)x / m;
            SCALE_Y = (double)y / n;
            SCALE_Z = (double)z / k;

            matrix = new double[m * 2 + 1, n * 2 + 1, k * 2 + 1][];
            nqp = 0;
            // This is private helper matrix
            // it's purpose is to store global coord related to matrix sizes coord (I can't explain better, just look at createAKT))) )
            local_to_global = new int[m * 2 + 1, n * 2 + 1, k * 2 + 1];

            nel = m * n * k;
            NT = new int[nel][];
        }

        public Mesh GetMesh() {
            fillMatrixWithMainVertexes();
            fillMatrixWithIntermidiateVertexes();
            createAKT();
            createNT();

            Mesh mesh = new Mesh() {
                nel = nel,
                nqp = nqp,
                AKT = AKT,
                NT  = NT
            };

            return mesh;
        }

        private void fillMatrixWithMainVertexes()
        {
            for (int deltaZ = 0; deltaZ < k + 1; deltaZ++)
            {
                for (int deltaY = 0; deltaY < n + 1; deltaY++)
                {
                    for (int deltaX = 0; deltaX < m + 1; deltaX++)
                    {
                        matrix[deltaX * 2, deltaY * 2, deltaZ * 2] = new double[] { SCALE_X * deltaX, SCALE_Y * deltaY, SCALE_Z * deltaZ };
                        nqp++;
                    }
                }
            }
        }
        private void fillMatrixWithIntermidiateVertexes()
        {
            double[] current;
            for (int deltaZ = 0; deltaZ < k + 1; deltaZ++)
            {
                for (int deltaY = 0; deltaY < n + 1; deltaY++)
                {
                    for (int deltaX = 0; deltaX < m + 1; deltaX++)
                    {
                        current = matrix[deltaX * 2, deltaY * 2, deltaZ * 2];
                        if (deltaX != m)
                        {
                            matrix[deltaX * 2 + 1, deltaY * 2, deltaZ * 2] = new double[] { current[0] + SCALE_X / 2, current[1], current[2] };
                            nqp++;
                        }

                        if (deltaY != n)
                        {
                            matrix[deltaX * 2, deltaY * 2 + 1, deltaZ * 2] = new double[] { current[0], current[1] + SCALE_Y / 2, current[2] } ;
                            nqp++;
                        }

                        if (deltaZ != k)
                        {
                            matrix[deltaX * 2, deltaY * 2, deltaZ * 2 + 1] = new double[] { current[0], current[1], current[2] + SCALE_Z / 2 };
                            nqp++;
                        }
                    }
                }
            }
        }
        private void createAKT()
        {
            AKT = new double[nqp][];
            int counter = 0;
            for (int deltaZ = 0; deltaZ < k * 2 + 1; deltaZ++)
            {
                for (int deltaY = 0; deltaY < n * 2 + 1; deltaY++)
                {
                    for (int deltaX = 0; deltaX < m * 2 + 1; deltaX++)
                    {
                        if (matrix[deltaX, deltaY, deltaZ] != null)
                        {
                            AKT[counter] = matrix[deltaX, deltaY, deltaZ];
                            local_to_global[deltaX, deltaY, deltaZ] = counter;
                            counter++;
                        }
                    }
                }
            }
        }
        private void createNT()
        {
            for (int figure = 0; figure < nel; figure++)
            {
                int current_element = figure;

                int Z_COOORDINATE = figure / (m * n);

                current_element %= m * n;
                int Y_COORDINATE = current_element / m;

                current_element %= m;
                int X_COORDINATE = current_element;

                createIndependentElements(X_COORDINATE * 2, Y_COORDINATE * 2, Z_COOORDINATE * 2, figure);
            }
        }
        private void createIndependentElements(int x, int y, int z, int belongElementNumber)
        {
            int[] globalCoordinates = new int[20];
            int[] delta;

            for (int i = 0; i < 20; i++)
            {
                delta = adapter[i];
                globalCoordinates[i] = local_to_global[x + delta[0], y + delta[1], z + delta[2]];
            }

            NT[belongElementNumber] = globalCoordinates;
        }
    }
}
