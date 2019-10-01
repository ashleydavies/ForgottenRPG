using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ForgottenRPG.Service;
using ForgottenRPG.Util;
using ForgottenRPG.Util.Coordinate;

namespace ForgottenRPG.Map {
    public class MapTileStore {
        private readonly Dictionary<int, MapTile> _mapTileDictionary;
        private readonly ITextureStore _textureStore;
        private readonly XDocument _tileDocument;

        public MapTileStore(string directory, ITextureStore textureStore) {
            _mapTileDictionary = new Dictionary<int, MapTile>();
            _textureStore = textureStore;

            using (var fs = File.OpenRead(Path.Combine(directory, "TileData.xml"))) {
                _tileDocument = XDocument.Load(fs);
            }
        }

        public MapTile GetTile(int id) {
            if (!_mapTileDictionary.ContainsKey(id)) {
                ServiceLocator.LogService.Log(LogType.Info, "Loading tile " + id);
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
                .Elements("Tile")
                .FirstOrDefault(elems => elems.Attribute("id").Value.Equals(id.ToString()));

            if (tileData == null) return MapTile.Null;

            var name = tileData.Attribute("name")?.Value;
            var collision = tileData.Attribute("collision")?.Value == "1";
            var complex = tileData.Attribute("textureComplex")?.Value == "1";

            if (name == null) return MapTile.Null;

            ISpriteable graphic;

            if (complex) {
                var spriteElems = tileData.Elements("Texture");

                var sprites = spriteElems.Select(
                    spriteElem =>
                        _textureStore.GetNewSprite(spriteElem.Attribute("name").Value)
                ).ToList();

                graphic = new AnimatedSprite(sprites, double.Parse(tileData.Attribute("textureTime").Value));
            } else {
                graphic = new SpriteableWrapper(_textureStore.GetNewSprite(name));
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
