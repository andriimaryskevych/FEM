using System;
using CommandLine;
using FEM.Factories;
using FEM.Interfaces.ParameterExtractor;
using FEM.DTO;
using FEM.Models;

namespace FEM
{
    class Program
    {
        static void Main(string[] args)
        {
            CLIOptions options = new CLIOptions();
            Parser.Default.ParseArguments<CLIOptions>(args).WithParsed<CLIOptions>(o => options = o);

            IParameterExtractor extractor = ParameterExtraction.GetExtractor(options);
            Parameters parameters = extractor.extract();

            new FiniteElementMethod(parameters).Start();
        }
    }
}
