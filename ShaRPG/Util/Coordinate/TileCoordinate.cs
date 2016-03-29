namespace ShaRPG.Util.Coordinate
{
    public class TileCoordinate : Coordinate
    {
        public TileCoordinate(int x, int y) : base(x, y) {}

        public static implicit operator GameCoordinate(TileCoordinate tileCoordinate)
        {
			return new GameCoordinate(0, 0);
        }
    }
}