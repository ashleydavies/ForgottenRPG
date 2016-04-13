using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Map
{
    public class GameMap
    {
        public readonly Vector2I Size;
        private readonly int[][] _tiles; 
        private readonly MapTileStore _tileStore;

        public GameMap(int[][] tiles, Vector2I size, MapTileStore tileStore)
        {
            _tiles = tiles;
            _tileStore = tileStore;
            Size = size;
        }

        public MapTile GetTile(TileCoordinate coordinate) => _tileStore.GetTile(GetTileId(coordinate));

        public int GetTileId(TileCoordinate coordinate)
        {
            return _tiles[coordinate.Y][coordinate.X];
        }

        public void Update(float delta)
        {
            _tileStore.Update(delta);
        }
    }
}