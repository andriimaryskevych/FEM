using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Extreme.Mathematics;
using Extreme.Mathematics.LinearAlgebra;
using FEM.DTO;
using FEM.Solvers;
using FEM.Helpers;
using FEM.Models;

namespace FEM
{
    class FiniteElementMethod
    {
        private TimeLogger timer;
        private TimeLogger global;

        public int m;
        public int n;
        public int k;

        public double E;
        public double v;
        public double lam;
        public double mu;
        public Pressure[] pressuredPart;

        public double[][] AKT;
        public int[][] NT;

        public int nel;
        public int nqp;

        public int[][] PAdapter = new int[6][] {
            new int[8] { 0, 1, 5, 4, 8, 13, 16, 12},
            new int[8] { 1, 2, 6, 5, 9, 14, 17, 13},
            new int[8] { 2, 3, 7, 6, 10, 15, 18, 14 },
            new int[8] { 3, 0, 4, 7, 11, 12, 19, 15},
            new int[8] { 0, 1, 2, 3, 8, 9, 10, 11},
            new int[8] { 4, 5, 6, 7, 16, 17, 18, 19}
        };

        public double[,,] DFIABG = Globals.DFIABG;
        public double[,,] DFIABG_P = Globals.DFIABG_P;
        public double[,,] DPSIET = Globals.DPSIET;
        public double[,] PSIET = Globals.PSIET;

        public double[] c = new double[3] { 5.0 / 9.0, 8.0 / 9.0, 5.0 / 9.0 };

        public SparseCompressedColumnMatrix<double> MG;
        public SparseVector<double> F;
        private double[] U;

        public int[] ZU;
        public double[][] ZP;

        private double[][] TENSOR;

        public FiniteElementMethod(Parameters parameters, Mesh mesh)
        {
            timer = new TimeLogger();
            global = new TimeLogger();

            m = parameters.xAxisFEMCount;
            n = parameters.yAxisFEMCount;
            k = parameters.zAxisFEMCount;

            v = parameters.puasson;
            E = parameters.jung;
            pressuredPart = parameters.load;

            lam = E / ((1 + v) * (1 - 2 * v));
            mu = E / (2 * (1 + v));

            AKT = mesh.AKT;
            NT = mesh.NT;

            nqp = mesh.nqp;
            nel = mesh.nel;

            int size = nqp * 3;

            MG = Matrix.CreateSparseSymmetric<double>(size, 0.05);
            F = Vector.CreateSparse<double>(size);
        }

        public void Start()
        {
            timer.LogTime("Generated mesh, ATK");

            createZU();
            createZP();
            getMG();
            fastenNodes();
            createF();
            getResult();
            returnEndPositions();
            createPressureVector();
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
            int pressuredPartsCount = pressuredPart.Length;

            ZP = new double[pressuredPartsCount][];
            for (int i = 0; i < pressuredPartsCount; i++)
            {
                Pressure load = pressuredPart[i];
                ZP[i] = new double[3];

                ZP[i][0] = load.fe;
                ZP[i][1] = load.part;
                ZP[i][2] = load.pressure;
            }
        }

        private double[,][,] getMGE() {
            double[,,] dxyzabg = new double[3, 3, 27];
            double[] dj = new double[27];
            double[,,] dfixyz = new double[27, 20, 3];

            int number = 0;
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

            return mge;
        }

        private void getMG()
        {
            timer.LogTime("Generated ZU, ZP, NT");

            // defined once and used for each finite element
            double[,,] dxyzabg = new double[3, 3, 27];
            double[] dj = new double[27];
            double[,,] dfixyz = new double[27, 20, 3];

            // As all elements are the same,
            // Calculating MGE only once
            double[,][,] mge = getMGE();
            timer.LogTime("Generated MGE");

            for (int number = 0; number < nel; number++)
            {
                int x, y, localX, localY, globalX, globalY;
                for (int i = 0; i < 60; i++)
                {
                    for (int j = i; j < 60; j++)
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

            timer.LogTime("Generated MG");
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
                        res[i, j] = 0;
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
                        res[i, j] = 0;
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
                        res[i, j] = 0;
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

        private void fastenNodes()
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

        private void createF()
        {
            PressureConfig[] PressurePartsConfig = Globals.PressurePartsConfig;

            // Pressure configurations
            double[][] config = new double[6][];
            double[,,] DXYZET;

            int site, number;
            double elementPressure;

            for (int num = 0; num < ZP.Length; num++)
            {
                number = (int)ZP[num][0];
                site = (int)ZP[num][1];
                elementPressure = ZP[num][2];

                PressureConfig current = PressurePartsConfig[site];

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
                                diPsi = DPSIET[k, j, l];
                                sum += globalCoordinate * diPsi;
                            }
                            DXYZET[i, j, k] = sum;
                        }
                    }
                }

                // not the best code below

                double[] fe = new double[8];

                for (int i = 0; i < 8; i++)
                {
                    sum = 0;
                    int counter = 0;
                    for (int m = 0; m < 3; m++)
                    {
                        for (int n = 0; n < 3; n++)
                        {
                            sum += elementPressure *
                                (
                                    DXYZET[current.dxyzabgIndexs[0, 0], current.dxyzabgIndexs[0, 1], counter] *
                                    DXYZET[current.dxyzabgIndexs[1, 0], current.dxyzabgIndexs[1, 1], counter] -
                                    DXYZET[current.dxyzabgIndexs[2, 0], current.dxyzabgIndexs[2, 1], counter] *
                                    DXYZET[current.dxyzabgIndexs[3, 0], current.dxyzabgIndexs[3, 1], counter]) *
                                PSIET[i, counter]
                                * c[n] * c[m];
                            ++counter;
                        }
                    }

                    fe[i] = sum;
                }

                for (int i = 0; i < 8; i++)
                {
                    F[coordinates[PAdapter[site][i]] * 3 + current.axisAddition] += fe[i];
                }
            }

            timer.LogTime("Generated F");
        }

        private void getResult()
        {
            U = Solvers.Extreme.Solve(MG, F);
            timer.LogTime("Solved Ax=B");
        }

        private void returnEndPositions ()
        {
            double[][] AKTres = new double[nqp][];
            for (int i = 0; i < nqp; i++)
            {
                double[] prev = AKT[i];
                double[] point = U.Skip(i * 3).Take(3).ToArray();
                AKTres[i] = new double[3] { Math.Round(prev[0] + point[0], 4), Math.Round(prev[1] + point[1], 4), Math.Round(prev[2] + point[2], 4) };
            }

            using (StreamWriter sw = new StreamWriter("points.txt", false, System.Text.Encoding.Default))
            {
                sw.WriteLine(JsonConvert.SerializeObject(new Points(AKTres, NT)));
            }

            timer.LogTime("Generated points.txt");
            global.LogTime("Finished solving");
        }

        private void createPressureVector()
        {
            // Currently I have MG, F and U calculated
            // Now I have 9 steps to reproduce to calcularte pressure vector

            // step 1,2,3

            // defined once and used for each finite element
            double[,,] dxyzabg = new double[3, 3, 20];
            double[,,] dfixyz = new double[20, 20, 3];
            double[][,] duxyz = new double[20][,];

            TENSOR = new double[nqp][];

            double[][,] SUM = new double[nqp][,];
            for (int i = 0; i < nqp; i++)
            {
                SUM[i] = new double[3,3];
                TENSOR[i] = new double[3];
            }


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
                double[][] sigma =      new double[20][];
                double[][] J =          new double[20][];
                double[][] TENSOR_E =   new double[20][];

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
                    duxyz[i] = new double[3, 3];

                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            sum = 0;
                            for (int l = 0; l < 20; l++)
                            {
                                sum += U[coordinates[l] * 3 + j] * dfixyz[i, l, k];
                            }

                            duxyz[i][j, k] = sum;
                        }
                    }
                }

                for (int i = 0; i < 20; i++)
                {
                    sigma[i] = getSigma(duxyz[i]);
                }

                for (int i = 0; i < 20; i++)
                {
                    J[i] = getMainPressure(sigma[i]);
                }

                for (int i = 0; i < 20; i++)
                {
                    TENSOR_E[i] = Cubic.Solve(1, -J[i][0], J[i][1], -J[i][2]);

                    TENSOR[coordinates[i]][0] += TENSOR_E[i][0];
                    TENSOR[coordinates[i]][1] += TENSOR_E[i][1];
                    TENSOR[coordinates[i]][2] += TENSOR_E[i][2];
                }
            }

            for (int i = 0; i < nqp; i++)
            {
                TENSOR[i][0] /= amount[i];
                TENSOR[i][1] /= amount[i];
                TENSOR[i][2] /= amount[i];

                Console.Write("Vertex number {0} ", i);
                Console.Write("{0} {1} {2}", TENSOR[i][0], TENSOR[i][1], TENSOR[i][2]);
                Console.WriteLine();
            }
        }

        private double[] getSigma(double[,] duxyz)
        {
            double[] res = new double[6];

            res[0] = lam * ( (1 - v) * duxyz[0, 0] + v * (duxyz[1, 1] + duxyz[2, 2]) );
            res[1] = lam * ( (1 - v) * duxyz[1, 1] + v * (duxyz[0, 0] + duxyz[2, 2]) );
            res[2] = lam * ( (1 - v) * duxyz[2, 2] + v * (duxyz[0, 0] + duxyz[1, 1]) );
            res[3] = mu * (duxyz[0, 1] + duxyz[1, 0]);
            res[4] = mu * (duxyz[1, 2] + duxyz[2, 1]);
            res[5] = mu * (duxyz[0, 2] + duxyz[2, 0]);

            return res;
        }

        private double[] getMainPressure(double[] sigma)
        {
            double[] res = new double[3];

            res[0] =
                sigma[0] +
                sigma[1] +
                sigma[2];

            res[1] =
                sigma[0] * sigma[1] +
                sigma[0] * sigma[2] +
                sigma[1] * sigma[2] -
                (
                    Math.Pow(sigma[3], 2) +
                    Math.Pow(sigma[4], 2) +
                    Math.Pow(sigma[5], 2)
                );

            res[2] =
                sigma[0] * sigma[1] * sigma[2] +
                sigma[3] * sigma[4] * sigma[5] * 2 -
                (
                    sigma[0] * Math.Pow(sigma[4], 2) +
                    sigma[1] * Math.Pow(sigma[5], 2) +
                    sigma[2] * Math.Pow(sigma[3], 2)
                );

            return res;
        }
    }
}
