namespace ShaRPG.Util.Coordinate
{
    public abstract class Coordinate
    {
        private int _x;
        private int _y;

        protected Coordinate(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }
}