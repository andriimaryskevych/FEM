namespace FEM.DTO
{
    class Mesh
    {
        public Mesh() {}

        // Finite elements count
        public int nel;

        // Points count
        public int nqp;

        // Dimension: [nqp][3]
        // Stores x, y, and z coordinate of each point
        public double[][] AKT;

        // Dimension: [nel][20]
        // Stores global number of each point in it finite element
        public int[][] NT;
    }
}
