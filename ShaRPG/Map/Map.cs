using ShaRPG.Util.Coordinate;

namespace ShaRPG.Map
{
    public class Map
    {
        private readonly int[,] _tiles;
        private readonly int _width;
        private readonly int _height;

        private Map(int[,] tiles, int width, int height)
        {
            _tiles = tiles;
            _width = width;
            _height = height;
        }

        public Map fromXML()
        {
            return new Map(new int[,] {}, 0, 0);
        }

        public MapTile GetTile(TileCoordinate coordinate)
        {
            return MapTile.GetTile(GetTileId(coordinate));
        }

        public int GetTileId(TileCoordinate coordinate)
        {
            return _tiles[coordinate.X, coordinate.Y];
        }
    }
}