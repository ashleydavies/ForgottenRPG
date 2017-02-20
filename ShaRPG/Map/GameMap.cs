using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Map {
    public class GameMap {
        private readonly int[,] _tiles;
        private readonly List<KeyValuePair<TileCoordinate, string>> _spawnPositions;
        private readonly MapTileStore _tileStore;
        public readonly Vector2I Size;

        public GameMap(int[,] tiles, Vector2I size, MapTileStore tileStore,
                       List<KeyValuePair<TileCoordinate, string>> spawnPositions) {
            _tiles = tiles;
            _tileStore = tileStore;
            _spawnPositions = spawnPositions;
            Size = size;
        }

        public void Render(IRenderSurface renderSurface) {
            for (int x = 0; x < Size.X; x++) {
                for (int y = 0; y < Size.Y; y++) {
                    GetTile(new TileCoordinate(x, y)).Draw(renderSurface, new TileCoordinate(x, y));
                }
            }
        }

        public TileCoordinate GetSpawnPosition(string name) {
            return _spawnPositions.FirstOrDefault(x => x.Value == name).Key;
        }

        public MapTile GetTile(TileCoordinate coordinate) => _tileStore.GetTile(GetTileId(coordinate));

        public int GetTileId(TileCoordinate coordinate) {
            return _tiles[coordinate.X, coordinate.Y];
        }

        public void Update(float delta) {
            _tileStore.Update(delta);
        }
    }
}
