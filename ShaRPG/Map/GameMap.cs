#region

using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

#endregion

namespace ShaRPG.Map {
    public class GameMap {
        private readonly int[][] _tiles;
        private readonly MapTileStore _tileStore;
        public readonly Vector2I Size;

        public GameMap(int[][] tiles, Vector2I size, MapTileStore tileStore) {
            _tiles = tiles;
            _tileStore = tileStore;
            Size = size;
        }

        public void Render(IRenderSurface renderSurface) {
            for (int x = 0; x < Size.X; x++) {
                for (int y = Size.Y - 1; y >= 0; y--) {
                    GetTile(new TileCoordinate(x, y)).Draw(renderSurface, new TileCoordinate(x, y));
                }
            }
        }

        public MapTile GetTile(TileCoordinate coordinate) => _tileStore.GetTile(GetTileId(coordinate));

        public int GetTileId(TileCoordinate coordinate) {
            return _tiles[coordinate.Y][coordinate.X];
        }

        public void Update(float delta) {
            _tileStore.Update(delta);
        }
    }
}
