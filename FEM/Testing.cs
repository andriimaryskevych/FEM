using System;

namespace FEM
{
    class Testing
    {
        public Testing()
        {
            // #1: Testing AKT and NT

            //FiniteElementMethod solve = new FiniteElementMethod(4, 4, 4, 4, 4, 4);
            //solve.Start();
            //int[][] nt = solve.NT;
            //double[][] akt = solve.AKT;
            //double[] point;

            //for (int i = 0; i < 20; i++)
            //{
            //    point = akt[nt[0][i]];
            //    Console.WriteLine($"{i}:  {nt[0][i]} --- ({point[0]}, {point[1]}, {point[2]})");
            //}


            // #2: Testing modified SquareGenerator

            //double[][] fi = new SquareGenerator(-1, 0, 1).getItInSingleDArray();
            //Console.WriteLine(fi.Length);
            //for (int i = 0; i < fi.Length; i++)
            //{
            //    Console.WriteLine($"{fi[i][0],2} {fi[i][1],2} {fi[i][2],2}");
            //}


            // #3: Testing Functions

            double[][] fi = new SquareGenerator(-1, 0, 1).getItInSingleDArray();
            for (int i = 0; i < 20; i++)
            {
                double[] coord = fi[i];
                for (int j = 0; j < 20; j++)
                {
                    Console.Write(FI.getFi(j, coord[0], coord[1], coord[2]));
                }
                Console.WriteLine();
            }


            // #4: Gauss nodes

            //double[][] val = Globals.GaussNodes;
            //for (int i = 0; i < 27; i++)
            //{
            //    Console.WriteLine($"{val[i][0],15} {val[i][1],15} {val[i][2],15}");
            //}


            // #5: DFIABG

            //double[,,] dfiabg = Globals.DFIABG;
            //for (int i = 0; i < 27; i++)
            //{
            //    Console.WriteLine(dfiabg[i, 0, 10]);
            //}

            // #6: Gaussian

            //double[,] matrix = new double[3, 3] {
            //    { 1, 2, 0 },
            //    { 4, 5, 0 },
            //    { 0, 1, 1 }
            //};

            //double[] res = new double[] { 25, 70, 13 };

            //double[] result = Gaussian.Solve(matrix, res);
            //foreach (double a in result)
            //{
            //    Console.WriteLine(a);
            //}


            // #7: one one

            //Console.WriteLine("Here");
            //double[,] value = new FiniteElementMethod(4, 4, 4, 4, 4, 4, 0.3).one_two(new double[1,1,1]);
            //for (int i = 0; i < 20; i++)
            //{
            //    for (int j = 0; j < 20; j++)
            //    {
            //        Console.Write(value[i, j]);
            //    }
            //    Console.WriteLine();
            //}


            // #8: rotating

            //double[,] arr = {
            //    { 1,2,3 },
            //    { 4,5,6 },
            //    { 7,8,9 }
            //};
            //double[,] newArr = new FiniteElementMethod(4, 4, 4, 4, 4, 4, 0.3).rotate(arr);
            //for (int i = 0; i < 3; i++)
            //{
            //    for (int j = 0; j < 3; j++)
            //    {
            //        Console.Write(arr[i,j]);
            //    }
            //    Console.WriteLine();
            //}
            //for (int i = 0; i < 3; i++)
            //{
            //    for (int j = 0; j < 3; j++)
            //    {
            //        Console.Write(newArr[i,j]);
            //    }
            //    Console.WriteLine();
            //}

            // #9: Mod

            //for (int i = 0; i < 60; i++)
            //{
            //    Console.WriteLine($"{i} --- {i % 20}");
            //}

            // #10: MG

            //FiniteElementMethod solve = new FiniteElementMethod(4, 4, 4, 1, 1, 1, 0.3);
            //solve.Start();
            //double[,] mg = solve.MG;
            //Console.WriteLine("Test");


            // #11: ZU

            //FiniteElementMethod solve = new FiniteElementMethod(4, 4, 4, 4, 4, 4, 0.3);
            //solve.Start();
            //int[] zu = solve.ZU;
            //foreach (int a in zu)
            //{
            //    Console.WriteLine(a);
            //}


            // #12: Modified MG

            //FiniteElementMethod solve = new FiniteElementMethod(4, 4, 4, 4, 4, 4, 0.3);
            //solve.Start();
            //double[,] MG = solve.MG;
            //for (int i = 0; i < 5; i++)
            //{
            //    for (int j = 0; j < 5; j++)
            //    {
            //        Console.Write($"{MG[i, j],20}");
            //    }
            //    Console.WriteLine();
            //}


            // #13: eta tau check

            //for (int i = 0; i < 2; i++)
            //{
            //    for (int j = 0; j < 8; j++)
            //    {
            //        PSI.getDiPsi(i, j, 1, 1);
            //    }
            //}


            // #14: 9 Gauss Nodes

            //for (int i = 0; i < 9; i++)
            //{
            //    Console.WriteLine($"{Globals.GaussNodes9[i][0]} {Globals.GaussNodes9[i][1]}");
            //}

            // #15: PSI

            //double[][] nodes = Globals.GaussNodes9;
            //Dictionary<int, double[]> adapter = new Dictionary<int, double[]>
            //{
            //    {0, new double[] {-1, -1} },
            //    {1, new double[] { 1, -1} },
            //    {2, new double[] { 1,  1} },
            //    {3, new double[] {-1,  1} },

            //    {4, new double[] { 0, -1} },
            //    {5, new double[] { 1,  0} },
            //    {6, new double[] { 0,  1} },
            //    {7, new double[] {-1,  0} }
            //};

            //for (int i = 0; i < 8; i++)
            //{
            //    for (int j = 0; j < 8; j++)
            //    {
            //        double[] values = adapter[j];
            //        Console.Write(PSI.getPsi(i, values[0], values[1]));
            //    }
            //    Console.WriteLine();
            //}

        }
    }
}
