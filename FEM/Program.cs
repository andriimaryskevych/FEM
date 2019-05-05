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

            new FiniteElementMethod(parameters).Start();
        }
    }
}
