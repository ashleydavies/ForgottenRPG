using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Map
{
    public class GameMap
    {
        public readonly Vector2I Size;
        private readonly int[,] _tiles; 
        private readonly MapTileStore _tileStore;

        private GameMap(int[,] tiles, Vector2I size, MapTileStore tileStore)
        {
            _tiles = tiles;
            _tileStore = tileStore;
            Size = size;
        }

        public static GameMap FromXml(MapTileStore tileStore) => new GameMap(new int[,] { {2, 1, 2, 5, 4},
                                                                                          {2, 2, 2, 1, 2},
                                                                                          {2, 1, 3, 3, 1},
                                                                                          {2, 2, 2, 2, 2}}, new Vector2I(5, 4), tileStore);

        public MapTile GetTile(TileCoordinate coordinate) => _tileStore.GetTile(GetTileId(coordinate));

        public int GetTileId(TileCoordinate coordinate)
        {
            return _tiles[coordinate.Y, coordinate.X];
        }

        public void Update(float delta)
        {
            _tileStore.Update(delta);
        }
    }
}