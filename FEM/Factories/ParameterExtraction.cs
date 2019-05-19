using FEM.Interfaces.ParameterExtractor;
using System;
using System.Linq;
using FEM.Models;

namespace FEM.Factories
{
    class ParameterExtraction
    {
        static public IParameterExtractor GetExtractor(CLIOptions options) {
            if (options.JSON == null) {
                return new DefaultExtractor();
            } else {
                return new JSONExtractor();
            }
        }
    }
}
