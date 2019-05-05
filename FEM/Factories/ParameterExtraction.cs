using FEM.Interfaces.ParameterExtractor;

namespace FEM.Factories
{
    class ParameterExtraction
    {
        static public IParameterExtractor GetExtractor() {
            return new DefaultExtractor();
        }
    }
}
