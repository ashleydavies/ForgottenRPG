using System.IO;
using System.Linq;
using System.Xml.Linq;
using ShaRPG.Service;
using ShaRPG.Util;

namespace ShaRPG.Map {
    internal class MapLoader {
        private readonly string _directory;
        private readonly MapTileStore _tileStore;

        public MapLoader(string directory, MapTileStore tileStore) {
            _directory = directory;
            _tileStore = tileStore;
        }

        public GameMap LoadMap(int id) {
            XDocument document;

            using (FileStream fs = File.OpenRead(Path.Combine(_directory, id + ".tmx"))) document = XDocument.Load(fs);

            var layer = document
                .Elements("map")
                .Elements("layer").FirstOrDefault();

            int colCount = -1, rowCount = -1;

            int.TryParse(layer?.Attribute("width")?.Value, out colCount);
            int.TryParse(layer?.Attribute("height")?.Value, out rowCount);

            if (layer == null || colCount == -1 || rowCount == -1) return null;

            var tiles = new int[colCount, rowCount];

            int x = 0, y = 0;

            foreach (string tile in layer.Elements("data")?.FirstOrDefault()?.Value?.Split(',')) {
                tiles[x++, y] = int.Parse(tile);

                ServiceLocator.LogService.Log(LogType.Information, $"Loaded tile {tiles[x - 1, y]}");

                if (x < colCount) continue;

                x = 0;
                y++;
            }

            return new GameMap(tiles, new Vector2I(colCount, rowCount), _tileStore);
        }
    }
}
