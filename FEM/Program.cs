using System;
using FEM.Factories;
using FEM.Interfaces.ParameterExtractor;
using FEM.DTO;

namespace FEM
{
    class Program
    {
        static void Main()
        {
            IParameterExtractor extractor = ParameterExtraction.GetExtractor();
            Parameters parameters = extractor.extract();

            Environment.Exit(1);

            //int size_x = int.Parse(args[0]);
            //int size_y = int.Parse(args[1]);
            //int size_z = int.Parse(args[2]);

            //int elem_x = int.Parse(args[3]);
            //int elem_y = int.Parse(args[4]);
            //int elem_z = int.Parse(args[5]);

            //double puasson = double.Parse(args[6]);
            //double jung = double.Parse(args[7]);
            //double pressure = double.Parse(args[8]);

            FiniteElementMethod fem = new FiniteElementMethod(parameters);

            fem.Start();
        }
    }
}
