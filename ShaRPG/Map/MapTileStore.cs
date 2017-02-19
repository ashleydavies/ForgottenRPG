#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

#endregion

namespace ShaRPG.Map {
    public class MapTileStore {
        private readonly Dictionary<int, MapTile> _mapTileDictionary;
        private readonly ISpriteStoreService _spriteStore;
        private readonly XDocument _tileDocument;

        public MapTileStore(string directory, ISpriteStoreService spriteStore) {
            _mapTileDictionary = new Dictionary<int, MapTile>();
            _spriteStore = spriteStore;

            using (var fs = File.OpenRead(Path.Combine(directory, "TileData.xml"))) {
                _tileDocument = XDocument.Load(fs);
            }
        }

        public MapTile GetTile(int id) {
            if (!_mapTileDictionary.ContainsKey(id)) {
                ServiceLocator.LogService.Log(LogType.Information, "Loading tile " + id);
                _mapTileDictionary[id] = LoadTile(id);
                if (_mapTileDictionary[id] == MapTile.Null) {
                    ServiceLocator.LogService.Log(LogType.Error, "Attempt to load tile " + id + " failed.");
                }
            }

            return _mapTileDictionary[id];
        }

        private MapTile LoadTile(int id) {
            var tileData = _tileDocument
                .Elements("Tiles")
                .Elements("Tile").FirstOrDefault(elems => elems.Attribute("id").Value.Equals(id.ToString()));

            if (tileData == null) return MapTile.Null;

            var name = tileData.Attribute("name")?.Value;
            var collision = tileData.Attribute("collision")?.Value == "1";
            var complex = tileData.Attribute("textureComplex")?.Value == "1";

            if (name == null) return MapTile.Null;

            IDrawable graphic;

            if (complex) {
                var spriteElems = tileData.Elements("Texture");

                var sprites = spriteElems.Select(spriteElem
                                                 => _spriteStore.GetSprite(spriteElem.Attribute("name").Value)).ToList();

                graphic = new AnimatedSprite(sprites, double.Parse(tileData.Attribute("textureTime").Value));
            } else {
                graphic = _spriteStore.GetSprite(name);
            }

            var newTile = new MapTile(id, graphic, name, collision);

            int xOffset = 0, yOffset = 0;

            int.TryParse(tileData.Attribute("textureXOffset")?.Value, out xOffset);
            int.TryParse(tileData.Attribute("textureYOffset")?.Value, out yOffset);

            newTile.TextureOffset = new GameCoordinate(xOffset, yOffset);

            return newTile;
        }

        public void Update(float delta) {
            foreach (var keyValuePair in _mapTileDictionary) {
                keyValuePair.Value.Update(delta);
            }
        }
    }
}
