using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using CommandLine;
using FEM.Factories;
using FEM.Interfaces.ParameterExtractor;
using FEM.DTO;
using FEM.Models;
using FEM.Helpers;

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

            Mesh mesh = new MeshCreator(parameters).GetMesh();

            using (StreamWriter sw = new StreamWriter("start.txt", false, System.Text.Encoding.Default))
            {
                sw.WriteLine(JsonConvert.SerializeObject(new Points(mesh.AKT, mesh.NT)));
            }

            Console.WriteLine("start.txt");

            if (options.MeshOnly) {
                Environment.Exit(0);
            }

            new FiniteElementMethod(parameters, mesh).Start();
        }
    }
}
