namespace FEM.DTO
{
    class Points
    {
        public Points(double[][] _AKT, int[][] _NT) {
            AKT = _AKT;
            NT = _NT;
        }

        public double[][] AKT;
        public int[][] NT;
    }
}
