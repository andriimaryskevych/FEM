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
                sw.WriteLine(JsonConvert.SerializeObject(new Points() { NT = mesh.NT, AKT = mesh.AKT }));
            }

            Console.WriteLine("start.txt");

            if (options.MeshOnly) {
                Environment.Exit(0);
            }

            Console.WriteLine("parameters.sizeX: {0}", parameters.sizeX);
            Console.WriteLine("parameters.sizeY: {0}", parameters.sizeY);
            Console.WriteLine("parameters.sizeZ: {0}", parameters.sizeZ);

            Console.WriteLine("parameters.xAxisFEMCount: {0}", parameters.xAxisFEMCount);
            Console.WriteLine("parameters.yAxisFEMCount: {0}", parameters.yAxisFEMCount);
            Console.WriteLine("parameters.zAxisFEMCount: {0}", parameters.zAxisFEMCount);

            Console.WriteLine("parameters.puasson: {0}", parameters.puasson);
            Console.WriteLine("parameters.young: {0}", parameters.young);

            foreach (Pressure a in parameters.load) {
                Console.WriteLine("fe: {0}, part: {1}, load: {2}", a.fe, a.part, a.pressure);
            }

            new FiniteElementMethod(parameters, mesh).Start();
        }
    }
}
