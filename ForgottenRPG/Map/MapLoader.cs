using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using ForgottenRPG.GameState;
using ForgottenRPG.Inventories;
using ForgottenRPG.Service;
using ForgottenRPG.Util;
using ForgottenRPG.Util.Coordinate;

namespace ForgottenRPG.Map {
    internal class MapLoader {
        private readonly string _directory;
        private readonly MapTileStore _tileStore;
        private readonly ItemManager _itemManager;

        public MapLoader(string directory, MapTileStore tileStore, ItemManager itemManager) {
            _directory = directory;
            _tileStore = tileStore;
            _itemManager = itemManager;
        }

        public GameMap LoadMap(int id, StateGame game) {
            XDocument document;

            using (FileStream fs = File.OpenRead(Path.Combine(_directory, id + ".tmx"))) document = XDocument.Load(fs);

            var map = document.Elements("map").FirstOrDefault();
            var tileLayer = map?.Elements("layer").FirstOrDefault();

            int colCount = -1, rowCount = -1;

            int.TryParse(tileLayer?.Attribute("width")?.Value, out colCount);
            int.TryParse(tileLayer?.Attribute("height")?.Value, out rowCount);

            if (tileLayer == null || colCount == -1 || rowCount == -1) return null;

            return new GameMap(game, LoadTiles(tileLayer, 0, 0, colCount, rowCount), new Vector2I(colCount, rowCount),
                               _tileStore, LoadSpawnPoints(document), LoadItems(document));
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

            ServiceLocator.LogService.Log(LogType.Info, $"Loaded {spawnPoints.Count} spawn points");
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
                    ServiceLocator.LogService.Log(LogType.Info, $"Pathing point {path[path.Count - 1]}");
                }
            }

            return path;
        }

        private List<(ItemStack, GameCoordinate)> LoadItems(XDocument document) {
            List<(ItemStack, GameCoordinate)> items = new List<(ItemStack, GameCoordinate)>();

            var itemElems = document.XPathSelectElements("/map/objectgroup[@name='Items']/object");

            foreach (XElement itemElem in itemElems) {
                string name = itemElem.Attribute("name").Value;
                int xPosition = int.Parse(itemElem.Attribute("x")?.Value ?? "");
                int yPosition = int.Parse(itemElem.Attribute("y")?.Value ?? "");

                GameCoordinate position = TileCoordinate.IsoToCartesian(xPosition, yPosition);

                int.TryParse(
                    itemElem.XPathSelectElement("properties/property[@name='quantity']").Attribute("value").Value,
                    out int quantity
                );

                items.Add((new ItemStack(_itemManager.GetItem(name), quantity), position));
            }

            ServiceLocator.LogService.Log(LogType.Info, $"Loaded {items.Count} map-based items");

            return items;
        }
    }
}
