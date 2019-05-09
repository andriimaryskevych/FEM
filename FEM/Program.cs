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

            Console.WriteLine(parameters.sizeX);
            Console.WriteLine(parameters.sizeY);
            Console.WriteLine(parameters.sizeZ);

            new FiniteElementMethod(parameters).Start();
        }
    }
}
