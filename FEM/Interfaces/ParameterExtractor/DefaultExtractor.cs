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

            parameters.xAxisFEMCount = 3;
            parameters.yAxisFEMCount = 3;
            parameters.zAxisFEMCount = 3;

            int totalCount = parameters.xAxisFEMCount * parameters.yAxisFEMCount * parameters.zAxisFEMCount;

            parameters.puasson = 0.3;
            parameters.jung = 1;
            parameters.pressure = -0.3;

            parameters.pr = new Pressure[1] {
                new Pressure() { fe = totalCount - 1, part = 5, pressure = -0.3 }
            };

            return parameters;
        }
    }
}
