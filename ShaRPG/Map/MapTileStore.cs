using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ShaRPG.Service;
using ShaRPG.Util;

namespace ShaRPG.Map {
    public class MapTileStore
    {
        private readonly ISpriteStoreService _spriteStore;
        private readonly Dictionary<int, MapTile> _mapTileDictionary;
        private readonly XDocument _tileDocument;

        public MapTileStore(string directory, ISpriteStoreService spriteStore)
        {
            _mapTileDictionary = new Dictionary<int, MapTile>();
            _spriteStore = spriteStore;

            using (var fs = File.OpenRead(Path.Combine(directory, "TileData.xml")))
            {
                _tileDocument = XDocument.Load(fs);
            }
        }

        public MapTile GetTile(int id)
        {
            if (!_mapTileDictionary.ContainsKey(id))
            {
                ServiceLocator.LogService.Log(LogType.Information, "Attempt to index non-existent tile " + id);
                _mapTileDictionary[id] = LoadTile(id);
            }

            return _mapTileDictionary[id];
        }

        private MapTile LoadTile(int id)
        {
            var tileData = _tileDocument
                .Elements("Tiles")
                .Elements("Tile").FirstOrDefault(elems => elems.Attribute("id").Value.Equals(id.ToString()));

            if (tileData == null)
            {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load tile " + id + " failed.");
                return null;
            }

            var name = tileData.Attribute("name")?.Value;
            var collision = tileData.Attribute("collision")?.Value == "1";
            var complex = tileData.Attribute("textureComplex")?.Value == "1";

            if (name == null)
            {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load tile " + id + " failed.");
            }

            IDrawable graphic;

            if (complex)
            {
                var spriteElems = tileData.Elements("Texture");

                List<Sprite> sprites = spriteElems.Select(spriteElem
                    => _spriteStore.GetSprite(spriteElem.Attribute("name").Value)).ToList();

                graphic = new AnimatedSprite(sprites, double.Parse(tileData.Attribute("textureTime").Value));
            }
            else
            {
                graphic = _spriteStore.GetSprite(name);
            }

            return new MapTile(id, graphic, name, collision);
        }

        public void Update(float delta)
        {
            foreach (var keyValuePair in _mapTileDictionary)
            {
                keyValuePair.Value.Update(delta);
            }
        }
    }
}
