using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using FEM.DTO;

namespace FEM
{
    class FiniteElementMethod
    {
        // this one holds all the points
        // when i devide a kube in 2x2x2 parts it's size beacames 5x5x5 * 3
        // to use only primitives, i decidet to store 5x5x5 vectors of length 3
        // this vectors hold exact position of each vertex in 3d space
        public double[,,][] matrix;

        // in case above, lists Count is equal 8
        //public List<FiniteElement> finiteElement;

        public int x;
        public int y;
        public int z;

        public int m;
        public int n;
        public int k;

        public double E;
        public double v;
        public double lam;
        public double mu;
        public double presure;

        public int nqp;
        // I changed order here:
        // As first argument I set global number and as a second one i set vector of lenght 3 that contains exact point coord
        public double[][] AKT;

        public int[,,] local_to_global;
        public Dictionary<int, int[]> adapter = Globals.magicDictionary;
        public int[][] PAdapter = new int[6][] {
            new int[8] { 0, 1, 5, 4, 8, 13, 16, 12},
            new int[8] { 1, 2, 6, 5, 9, 14, 17, 13},
            new int[8] { 2, 3, 7, 6, 10, 15, 18, 14 },
            new int[8] { 3, 0, 4, 7, 11, 12, 19, 15},
            new int[8] { 0, 1, 2, 3, 8, 9, 10, 11},
            new int[8] { 4, 5, 6, 7, 16, 17, 18, 19}
        };

        public double[][] GaussNodes = Globals.GaussNodes;
        public double[,,] DFIABG = Globals.DFIABG;
        public double[,,] DFIABG_P = Globals.DFIABG_P;

        public double[] c = new double[3] { 5.0 / 9.0, 8.0 / 9.0, 5.0 / 9.0 };

        int nel;
        // Same as AKT, FE number goes first than it's local value that evaluates to global coord of selected FE
        public int[][] NT;

        public double[,] MG;
        public double[] F;

        public int[] ZU;
        public double[,] ZP;

        public double[,,] DPSITE = Globals.DPSITE;
        public double[,] PSIET;

        private double SCALE_X;
        private double SCALE_Y;
        private double SCALE_Z;

        private double[] U;
        private double[][] TENSOR;
        public FiniteElementMethod(Parameters parameters)
        {
            x = parameters.sizeX;
            y = parameters.sizeY;
            z = parameters.sizeZ;

            m = parameters.xAxisFEMCount;
            n = parameters.yAxisFEMCount;
            k = parameters.zAxisFEMCount;

            v = parameters.puasson;
            E = parameters.jung;
            presure = parameters.pressure;

            lam = E / ((1 + v) * (1 - 2 * v));
            mu = E / (2 * (1 + v));

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

        public void Start()
        {
            fillMatrixWithMainVertexes();
            fillMatrixWithIntermidiateVertexes();
            initMatrix();
            createAKT();
            returnStartPositions();
            createZU();
            createZP();
            createNT();
            getMG();
            improveMG();
            createPSI();
            createF();
            getResult();
            createPressureVector();
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
        private void initMatrix()
        {
            MG = new double[3 * nqp, 3 * nqp];
            F = new double[3 * nqp];
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

        private void returnStartPositions()
        {
            using (StreamWriter sw = new StreamWriter("start.txt", false, System.Text.Encoding.Default))
            {
                sw.WriteLine(JsonConvert.SerializeObject((from a in AKT select new { x = a[0], y = a[1], z = a[2], })));
            }

            Console.Write("Generated start.txt");
        }

        private void createZU()
        {
            int i = 0;
            while (AKT[i][2] == 0)
            {
                i++;
            }
            ZU = Enumerable.Range(0, i).ToArray();
        }
        private void createZP()
        {
            int loadElementsCount = m * n;
            ZP = new double[loadElementsCount, 3];
            int firstOne = nel - loadElementsCount;
            for (int i = firstOne, counter = 0; i < nel; i++, counter++)
            {
                ZP[counter, 0] = i;
                ZP[counter, 1] = 5;
                ZP[counter, 2] = 10;
            }
        }

        private void createNT()
        {
            for (int figure = 0; figure < nel; figure++)
            {
                int current_element = figure;
                int Z_COOORDINATE = (int)(figure / (m * n));
                current_element %= (m * n);
                int Y_COORDINATE = (int)(current_element / m);
                current_element %= (m);
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

        private void getMG()
        {
            // defined once and used for each finite element
            double[,,] dxyzabg = new double[3, 3, 27];
            double[] dj = new double[27];
            double[,,] dfixyz = new double[27, 20, 3];

            for (int number = 0; number < nel; number++)
            {
                int[] coordinates = NT[number];

                // calc dxyzabg
                double globalCoordinate = 0;
                double diFi = 0;
                double sum = 0;

                // i stands for global coordinate
                // j for local
                // k for gaussNode
                // l for i-th function
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 27; k++)
                        {
                            sum = 0;
                            for (int l = 0; l < 20; l++)
                            {
                                globalCoordinate = AKT[coordinates[l]][i];
                                diFi = DFIABG[k, j, l];
                                sum += globalCoordinate * diFi;
                            }
                            dxyzabg[i, j, k] = sum;
                        }
                    }
                }

                // calc dj
                double[,] jak;
                for (int i = 0; i < 27; i++)
                {
                    jak = new double[3, 3] {
                    { dxyzabg[0,0,i], dxyzabg[1,0,i], dxyzabg[2,0,i] },
                    { dxyzabg[0,1,i], dxyzabg[1,1,i], dxyzabg[2,1,i] },
                    { dxyzabg[0,2,i], dxyzabg[1,2,i], dxyzabg[2,2,i] }
                };
                    dj[i] = (
                                jak[0, 0] * jak[1, 1] * jak[2, 2] +
                                jak[0, 1] * jak[1, 2] * jak[2, 0] +
                                jak[0, 2] * jak[1, 0] * jak[2, 1]
                            ) -
                            (
                                jak[0, 2] * jak[1, 1] * jak[2, 0] +
                                jak[0, 1] * jak[1, 0] * jak[2, 2] +
                                jak[0, 0] * jak[1, 2] * jak[2, 1]
                            );
                }

                // calc dfixyz
                // col is free column
                double[] col = new double[3];
                // i stands for gausNode
                // phi stands for i-th function
                for (int i = 0; i < 27; i++)
                {
                    for (int phi = 0; phi < 20; phi++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            col[k] = DFIABG[i, k, phi];
                        }
                        double[,] matrix = new double[3, 3] {
                            { dxyzabg[0,0,i], dxyzabg[1,0,i], dxyzabg[2,0,i] },
                            { dxyzabg[0,1,i], dxyzabg[1,1,i], dxyzabg[2,1,i] },
                            { dxyzabg[0,2,i], dxyzabg[1,2,i], dxyzabg[2,2,i] }
                        };
                        double[] gaussianSolve = Gaussian.Solve(matrix, col);

                        for (int k = 0; k < 3; k++)
                        {
                            dfixyz[i, phi, k] = gaussianSolve[k];
                        }
                    }
                }

                double[,][,] mge = new double[3, 3][,];

                mge[0, 0] = one_one(dfixyz, dj);
                mge[1, 1] = two_two(dfixyz, dj);
                mge[2, 2] = three_three(dfixyz, dj);

                mge[0, 1] = one_two(dfixyz, dj);
                mge[0, 2] = one_three(dfixyz, dj);
                mge[1, 2] = two_three(dfixyz, dj);

                mge[1, 0] = rotate(mge[0, 1]);
                mge[2, 0] = rotate(mge[0, 2]);
                mge[2, 1] = rotate(mge[1, 2]);

                int x, y, localX, localY, globalX, globalY;
                for (int i = 0; i < 60; i++)
                {
                    for (int j = 0; j < 60; j++)
                    {
                        x = i / 20;
                        y = j / 20;

                        localX = i % 20;
                        localY = j % 20;

                        globalX = (NT[number][localX]) * 3 + x;
                        globalY = (NT[number][localY]) * 3 + y;

                        MG[globalX, globalY] += mge[x, y][localX, localY];
                    }
                }
            };
            Console.WriteLine("Got MG");
        }

        private double[,] one_one(double[,,] dfixyz, double[] dj)
        {
            double[,] res = new double[20, 20];
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    if (i > j)
                    {
                        res[i, j] = res[j, i];
                    }
                    else
                    {
                        double sum = 0;
                        int counter = 0;
                        for (int k = 0; k < 3; k++)
                        {
                            for (int l = 0; l < 3; l++)
                            {
                                for (int m = 0; m < 3; m++)
                                {
                                    sum += (
                                            ( lam * (1 - v) * (dfixyz[counter, i, 0] * dfixyz[counter, j, 0]) )
                                            +
                                            ( mu * (dfixyz[counter, i, 1] * dfixyz[counter, j, 1] + dfixyz[counter, i, 2] * dfixyz[counter, j, 2]))
                                        ) * Math.Abs(dj[counter]) * c[m] * c[l] * c[k];
                                    ++counter;
                                }
                            }
                        }
                        res[i, j] = sum;
                    }
                }
            }
            return res;
        }
        private double[,] two_two(double[,,] dfixyz, double[] dj)
        {
            double[,] res = new double[20, 20];
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    if (i > j)
                    {
                        res[i, j] = res[j, i];
                    }
                    else
                    {
                        double sum = 0;
                        int counter = 0;
                        for (int k = 0; k < 3; k++)
                        {
                            for (int l = 0; l < 3; l++)
                            {
                                for (int m = 0; m < 3; m++)
                                {
                                    sum += (
                                            (lam * (1 - v) * (dfixyz[counter, i, 1] * dfixyz[counter, j, 1]))
                                            +
                                            (mu * (dfixyz[counter, i, 0] * dfixyz[counter, j, 0] + dfixyz[counter, i, 2] * dfixyz[counter, j, 2]))
                                        ) * Math.Abs(dj[counter]) * c[m] * c[l] * c[k];
                                    ++counter;

                                }
                            }
                        }
                        res[i, j] = sum;
                    }
                }
            }
            return res;
        }
        private double[,] three_three(double[,,] dfixyz, double[] dj)
        {
            double[,] res = new double[20, 20];
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    if (i > j)
                    {
                        res[i, j] = res[j, i];
                    }
                    else
                    {
                        double sum = 0;
                        int counter = 0;
                        for (int k = 0; k < 3; k++)
                        {
                            for (int l = 0; l < 3; l++)
                            {
                                for (int m = 0; m < 3; m++)
                                {
                                    sum += (
                                            (lam * (1 - v) * (dfixyz[counter, i, 2] * dfixyz[counter, j, 2]))
                                            +
                                            (mu * (dfixyz[counter, i, 0] * dfixyz[counter, j, 0] + dfixyz[counter, i, 1] * dfixyz[counter, j, 1]))
                                        ) * Math.Abs(dj[counter]) * c[m] * c[l] * c[k];
                                    ++counter;
                                }
                            }
                        }
                        res[i, j] = sum;
                    }
                }
            }
            return res;
        }

        private double[,] one_two(double[,,] dfixyz, double[] dj)
        {
            double[,] res = new double[20, 20];
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    double sum = 0;
                    int counter = 0;
                    for (int k = 0; k < 3; k++)
                    {
                        for (int l = 0; l < 3; l++)
                        {
                            for (int m = 0; m < 3; m++)
                            {
                                sum += (
                                    ( lam * v * (dfixyz[counter, i, 0] * dfixyz[counter, j, 1]) )
                                      +
                                    ( mu * (dfixyz[counter, i, 1] * dfixyz[counter, j, 0]) )
                                    ) * Math.Abs(dj[counter]) * c[m] * c[l] * c[k];
                                ++counter;
                            }
                        }
                    }
                    res[i, j] = sum;

                }
            }
            return res;
        }
        private double[,] one_three(double[,,] dfixyz, double[] dj)
        {
            double[,] res = new double[20, 20];
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    double sum = 0;
                    int counter = 0;
                    for (int k = 0; k < 3; k++)
                    {
                        for (int l = 0; l < 3; l++)
                        {
                            for (int m = 0; m < 3; m++)
                            {
                                sum += (
                                    (lam * v * (dfixyz[counter, i, 0] * dfixyz[counter, j, 2]))
                                      +
                                    (mu * (dfixyz[counter, i, 2] * dfixyz[counter, j, 0]))
                                    ) * Math.Abs(dj[counter]) * c[m] * c[l] * c[k];
                                ++counter;
                            }
                        }
                    }
                    res[i, j] = sum;

                }
            }
            return res;
        }
        private double[,] two_three(double[,,] dfixyz, double[] dj)
        {
            double[,] res = new double[20, 20];
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    double sum = 0;
                    int counter = 0;
                    for (int k = 0; k < 3; k++)
                    {
                        for (int l = 0; l < 3; l++)
                        {
                            for (int m = 0; m < 3; m++)
                            {
                                sum += (
                                    (lam * v * (dfixyz[counter, i, 1] * dfixyz[counter, j, 2]))
                                      +
                                    (mu * (dfixyz[counter, i, 2] * dfixyz[counter, j, 1]))
                                    ) * Math.Abs(dj[counter]) * c[m] * c[l] * c[k];
                                ++counter;
                            }
                        }
                    }
                    res[i, j] = sum;

                }
            }
            return res;
        }

        private double[,] rotate(double[,] toRotate)
        {
            double[,] res = new double[20, 20];
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    res[i, j] = toRotate[j, i];
                }
            }
            return res;
        }

        private bool isSymetricMG()
        {
            for (int i = 0; i < 3 * nqp; i++)
            {
                for (int j = i; j < 3 * nqp; j++)
                {
                    if (MG[i, j] != MG[j, i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool isMgGreaterThanZero()
        {
            for (int i = 0; i < nqp * 3; i++)
            {
                if (MG[i, i] < 0)
                {
                    Console.WriteLine(i);
                }
            }
            return true;
        }

        private void improveMG()
        {
            int index;
            for (int i = 0; i < ZU.Length; i++)
            {
                index = ZU[i] * 3;
                for (int j = 0; j < 3; j++)
                {
                    MG[index + j, index + j] = Globals.BIG_NUMBER;
                }
            }

        }

        private void createPSI()
        {
            PSIET = new double[8, 9];

            double[] values;
            double[][] nodes = Globals.GaussNodes9;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    values = nodes[j];
                    PSIET[i,j] = PSI.getPsi(i, values[0], values[1]);
                }
            }
        }

        private void createF()
        {
            double[,,] DXYZET;

            int site = 5;

            int loadElementsCount = m * n;
            int start = nel - loadElementsCount;
            for (int number = start; number < nel; number++)
            {
                DXYZET = new double[3, 2, 9];

                int[] coordinates = NT[number];

                // calc dxyzet
                double globalCoordinate = 0;
                double diPsi = 0;
                double sum = 0;

                // i stands for global coordinate
                // j for local
                // k for gaussNode
                // l for i-th function
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            sum = 0;
                            for (int l = 0; l < 8; l++)
                            {
                                globalCoordinate = AKT[coordinates[PAdapter[site][l]]][i];
                                diPsi = DPSITE[k, j, l];
                                sum += globalCoordinate * diPsi;
                            }
                            DXYZET[i, j, k] = sum;
                        }
                    }
                }

                // not the best code below

                double[] f2 = new double[8];

                for (int i = 0; i < 8; i++)
                {
                    sum = 0;
                    int counter = 0;
                    for (int m = 0; m < 3; m++)
                    {
                        for (int n = 0; n < 3; n++)
                        {
                            sum += presure *
                                (DXYZET[0, 0, counter] * DXYZET[1, 1, counter] - DXYZET[1, 0, counter] * DXYZET[0, 1, counter]) *
                                PSIET[i, counter]
                                * c[n] * c[m];
                            ++counter;
                        }
                    }
                    f2[i] = sum;
                }

                for (int i = 0; i < 8; i++)
                {
                    F[coordinates[PAdapter[site][i]] * 3 + 2] += f2[i];
                }
            }
            Console.WriteLine("Got F");
            Console.WriteLine(nqp);
        }

        private void getResult()
        {
            U = Extreme.Solve(MG, F);

            double[][] AKTres = new double[nqp][];
            for (int i = 0; i < nqp; i++)
            {
                double[] prev = AKT[i];
                double[] point = U.Skip(i * 3).Take(3).ToArray();
                AKTres[i] = new double[3] { Math.Round(prev[0] + point[0], 4), Math.Round(prev[1] + point[1], 4), Math.Round(prev[2] + point[2], 4) };
            }

            using (StreamWriter sw = new StreamWriter("points.txt", false, System.Text.Encoding.Default))
            {
                sw.WriteLine(JsonConvert.SerializeObject((from a in AKTres select new { x = a[0], y = a[1], z = a[2], })));
            }

            Console.Write("Generated points.txt");
        }

        private void createPressureVector()
        {
            // Currently I have MG, F and U calculated
            // Now I have 9 steps to reproduce to calcularte pressure vector

            // step 1,2,3

            // defined once and used for each finite element
            double[,,] dxyzabg = new double[3, 3, 20];
            double[,,] dfixyz = new double[20, 20, 3];
            double[,,] duxyz = new double[20, 3, 3];

            double[][,] SUM = new double[nqp][,];
            for (int i = 0; i < nqp; i++)
            {
                SUM[i] = new double[3,3];
            }

            double[][] sigma = new double[nqp][];

            TENSOR = new double[nqp][];

            double[] amount = new double[nqp];
            int[] coordinates;

            // calc number of entries for each node
            for (int number = 0; number < nel; number++)
            {
                coordinates = NT[number];
                for (int j = 0; j < 20; j++)
                {
                    amount[coordinates[j]]++;
                }
            }

            // Fill sum matrix
            for (int number = 0; number < nel; number++)
            {
                coordinates = NT[number];

                // calc dxyzabg
                double globalCoordinate = 0;
                double diFi = 0;
                double sum = 0;

                // i stands for global coordinate
                // j for local
                // k for gaussNode
                // l for i-th function
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 20; k++)
                        {
                            sum = 0;
                            for (int l = 0; l < 20; l++)
                            {
                                globalCoordinate = AKT[coordinates[l]][i];
                                diFi = DFIABG_P[k, j, l];
                                sum += globalCoordinate * diFi;
                            }
                            dxyzabg[i, j, k] = sum;
                        }
                    }
                }

                // calc dfixyz
                // col is free column
                double[] col = new double[3];
                // i stands for gausNode
                // phi stands for i-th function
                for (int i = 0; i < 20; i++)
                {
                    for (int phi = 0; phi < 20; phi++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            col[k] = DFIABG_P[i, k, phi];
                        }
                        double[,] matrix = new double[3, 3] {
                            { dxyzabg[0,0,i], dxyzabg[1,0,i], dxyzabg[2,0,i] },
                            { dxyzabg[0,1,i], dxyzabg[1,1,i], dxyzabg[2,1,i] },
                            { dxyzabg[0,2,i], dxyzabg[1,2,i], dxyzabg[2,2,i] }
                        };
                        double[] gaussianSolve = Gaussian.Solve(matrix, col);

                        for (int k = 0; k < 3; k++)
                        {
                            dfixyz[i, phi, k] = gaussianSolve[k];
                        }
                    }
                }

                // calc duxyz
                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            sum = 0;
                            for (int l = 0; l < 20; l++)
                            {
                                sum += U[coordinates[l] * 3 + j] * dfixyz[i, l, k];
                            }
                            duxyz[i, j, k] = sum;
                        }
                    }
                }

                // calc all sums: in each global point add all 9 values
                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            SUM[coordinates[i]][j, k] += duxyz[i, j, k];
                        }
                    }
                }
            }

            // get the avarage for each point
            for (int i = 0; i < nqp; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        SUM[i][j, k] /= amount[i];
                    }
                }
            }

            for (int i = 0; i < nqp; i++)
            {
                sigma[i] = getSigma(SUM[i]);
            }

            for (int i = 0; i < nqp; i++)
            {
                TENSOR[i] = getMainPressure(sigma[i]);
            }
        }

        private double[] getSigma(double[,] u)
        {
            double[] res = new double[6];

            res[0] = lam * ( (1 - v) * u[0, 0] + v * (u[1, 1] + u[2, 2]) );
            res[1] = lam * ( (1 - v) * u[1, 1] + v * (u[0, 0] + u[2, 2]) );
            res[2] = lam * ( (1 - v) * u[2, 2] + v * (u[0, 0] + u[1, 1]) );
            res[3] = mu * (u[0, 1] + u[1, 0]);
            res[4] = mu * (u[1, 2] + u[2, 1]);
            res[5] = mu * (u[0, 2] + u[2, 0]);

            return res;
        }

        private double[] getMainPressure(double[] sigma)
        {
            double[] res = new double[3];

            res[0] = sigma[0] + sigma[1] + sigma[2];
            res[1] = sigma[0] * sigma[1] + sigma[0] * sigma[2] + sigma[1] * sigma[2] -
                (Math.Pow(sigma[3], 2) + Math.Pow(sigma[4], 2) + Math.Pow(sigma[5], 2));
            res[2] = sigma[0] * sigma[1] * sigma[2] + 2 * sigma[3] * sigma[4] * sigma[5] -
                ( sigma[0] * Math.Pow(sigma[4], 2) + sigma[1] * Math.Pow(sigma[5], 2) + sigma[2] * Math.Pow(sigma[3], 2));

            return res;
        }
    }
}
