namespace FEM.DTO
{
    public class Pressure {
        public Pressure () {}

        public int fe { get; set; }
        public int part { get; set; }
        public double pressure { get; set; }
    }

    public class Parameters
    {
        public Parameters() {}

        public int sizeX { get; set; }
        public int sizeY { get; set; }
        public int sizeZ { get; set; }

        public int xAxisFEMCount { get; set; }
        public int yAxisFEMCount { get; set; }
        public int zAxisFEMCount { get; set; }

        public double puasson { get; set; }
        public double jung { get; set; }
        public Pressure[] load { get; set; }
    }
}
