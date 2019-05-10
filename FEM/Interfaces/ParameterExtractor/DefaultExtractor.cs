using FEM.DTO;

namespace FEM.Interfaces.ParameterExtractor
{
    class DefaultExtractor : IParameterExtractor
    {
        public DefaultExtractor() { }

        public Parameters extract() {
            Parameters parameters = new Parameters();

            parameters.sizeX = 100;
            parameters.sizeY = 100;
            parameters.sizeZ = 100;

            parameters.xAxisFEMCount = 15;
            parameters.yAxisFEMCount = 15;
            parameters.zAxisFEMCount = 15;

            parameters.puasson = 0.3;
            parameters.jung = 1;
            parameters.pressure = -0.3;

            return parameters;
        }
    }
}
