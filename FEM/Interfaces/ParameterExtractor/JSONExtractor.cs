using System;
using System.Linq;
using CommandLine;
using FEM.DTO;
using FEM.Models;

namespace FEM.Interfaces.ParameterExtractor
{
    class JSONExtractor : IParameterExtractor
    {
        public JSONExtractor() { }

        public Parameters extract()
        {
            CLIOptions options = new CLIOptions();
            Parser.Default.ParseArguments<CLIOptions>(Environment.GetCommandLineArgs()).WithParsed<CLIOptions>(o => options = o);

            Parameters parameters = Newtonsoft.Json.JsonConvert.DeserializeObject<Parameters>(options.JSON);

            return parameters;
        }
    }
}
