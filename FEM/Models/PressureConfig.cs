namespace FEM.Models
{
    class PressureConfig
    {
        public int axisAddition;
        public int[,] dxyzabgIndexs;
        public PressureConfig (int addition, int[,] indexes)
        {
            axisAddition = addition;
            dxyzabgIndexs = indexes;
        }
    }
}
