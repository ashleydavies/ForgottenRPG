using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
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
                               _tileStore, LoadSpawnPoints(document));
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

        private List<GameMapEntitySpawnDetails> LoadSpawnPoints(XDocument map) {
            var spawnPoints = new List<GameMapEntitySpawnDetails>();

            var spawnPointElems = map.XPathSelectElements("/map/objectgroup[@name='SpawnPoints']/object");

            foreach (XElement spawnPointElem in spawnPointElems) {
                int xPosition = int.Parse(spawnPointElem.Attribute("x")?.Value ?? "") / 32;
                int yPosition = int.Parse(spawnPointElem.Attribute("y")?.Value ?? "") / 32;

                var path = LoadPathInformation(map, spawnPointElem);

                spawnPoints.Add(
                    new GameMapEntitySpawnDetails(new TileCoordinate(xPosition, yPosition),
                                                  spawnPointElem.Attribute("name")?.Value, path)
                );
            }

            ServiceLocator.LogService.Log(LogType.Information, $"Loaded {spawnPoints.Count} spawn points");
            return spawnPoints;
        }

        private static List<TileCoordinate> LoadPathInformation(XDocument map, XElement spawnPointElem) {
            var pathInfoElem = spawnPointElem.XPathSelectElement(".//properties/property[@name='FollowsPath']");

            List<TileCoordinate> path = new List<TileCoordinate>();

            if (pathInfoElem != null) {
                int pathId = int.Parse(pathInfoElem.Attribute("value").Value);
                var pathElements = map.XPathSelectElements($"/map/objectgroup[@name='Path'][properties/property"
                                                           + $"[@name='PathNumber' and @value={pathId}]]/object");
                foreach (var pathElem in pathElements) {
                    path.Add(new TileCoordinate(int.Parse(pathElem.Attribute("x").Value) / 32,
                                                int.Parse(pathElem.Attribute("y").Value) / 32));
                    ServiceLocator.LogService.Log(LogType.Information, $"{path[path.Count - 1]}");
                }
            }

            return path;
        }
    }
}
