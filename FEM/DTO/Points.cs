namespace FEM.DTO
{
    class Points
    {
        public double[][] AKT;
        public int[][] NT;
        public double[] STRESS;

        public double maxStress { get; set; }
    }
}
