using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ShaRPG.Entity;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

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

            var map = document.Elements("map").FirstOrDefault();
            var tileLayer = map?.Elements("layer").FirstOrDefault();

            int colCount = -1, rowCount = -1;

            int.TryParse(tileLayer?.Attribute("width")?.Value, out colCount);
            int.TryParse(tileLayer?.Attribute("height")?.Value, out rowCount);

            if (tileLayer == null || colCount == -1 || rowCount == -1) return null;

            return new GameMap(LoadTiles(tileLayer, 0, 0, colCount, rowCount), new Vector2I(colCount, rowCount),
                               _tileStore, LoadSpawnPoints(map));
        }

        private static int[,] LoadTiles(XContainer layer, int x, int y, int colCount, int rowCount) {
            var tiles = new int[colCount, rowCount];

            var tileStrings = layer.Elements("data").FirstOrDefault()?.Value.Split(',');

            if (tileStrings == null) throw new LoadingException("Failed to load map - unable to load data section");

            foreach (string tile in tileStrings) {
                tiles[x++, y] = int.Parse(tile);
                if (x < colCount) continue;

                x = 0;
                y++;
            }

            return tiles;
        }

        private List<KeyValuePair<TileCoordinate, string>> LoadSpawnPoints(XElement layer) {
            var spawnPoints = new List<KeyValuePair<TileCoordinate, string>>();

            var elements = layer.Elements("objectgroup")
                                .FirstOrDefault(x => x.Attribute("name")?.Value.Equals("SpawnPoints") == true)
                                ?.Elements();

            if (elements == null) return spawnPoints;

            foreach (XElement spawnPoint in elements) {
                int xPosition = int.Parse(spawnPoint.Attribute("x")?.Value ?? "") / 32;
                int yPosition = int.Parse(spawnPoint.Attribute("y")?.Value ?? "") / 32;

                spawnPoints.Add(
                    new KeyValuePair<TileCoordinate, string>(new TileCoordinate(xPosition, yPosition),
                                                             spawnPoint.Attribute("name")?.Value)
                );
            }

            ServiceLocator.LogService.Log(LogType.Information, $"Loaded {spawnPoints.Count} spawn points");
            return spawnPoints;
        }
    }
}
