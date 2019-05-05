using System;
using System.Linq;
using FEM.DTO;

namespace FEM.Interfaces.ParameterExtractor
{
    class JSONExtractor : IParameterExtractor
    {
        public JSONExtractor() { }

        public Parameters extract()
        {
            String jsonString = Environment.GetCommandLineArgs().ToArray()[2];

            Parameters parameters = Newtonsoft.Json.JsonConvert.DeserializeObject<Parameters>(jsonString);

            return parameters;
        }
    }
}
