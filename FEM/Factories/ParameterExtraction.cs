using FEM.Interfaces.ParameterExtractor;
using System;
using System.Linq;

namespace FEM.Factories
{
    class ParameterExtraction
    {
        static public IParameterExtractor GetExtractor() {
            String[] arguments = Environment.GetCommandLineArgs().ToArray();

            // First parameter is .dll file location so it is always present
            if (arguments.Length == 1)
            {
                return new DefaultExtractor();
            }

            if (arguments[1] == "--json" || arguments[1] == "-j")
            {
                return new JSONExtractor();
            }

            return new DefaultExtractor();
        }
    }
}
