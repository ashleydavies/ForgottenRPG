using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShaRPG.Util;
using System.IO;
using System.Xml.Linq;

namespace ShaRPG.Service {
    class CachedFileSpriteStoreService : ISpriteStoreService
    {
        private readonly XDocument _resolutionDocument;
        private readonly Dictionary<int, string> _namespaceCacheDictionary;
        private readonly Dictionary<int, Texture> _textureCacheDictionary; 
        private readonly Dictionary<string, Sprite> _spriteCacheDictionary;
        private readonly string _rootDirectory;

        public CachedFileSpriteStoreService(string directory)
        {
            _rootDirectory = directory;
            _namespaceCacheDictionary = new Dictionary<int, string>();
            _textureCacheDictionary = new Dictionary<int, Texture>();
            _spriteCacheDictionary = new Dictionary<string, Sprite>();

            using (var fs = File.OpenRead(Path.Combine(directory, "Resolution.xml")))
            {
                _resolutionDocument = XDocument.Load(fs);
            }
        }

        public Sprite GetSprite(string name)
        {
            if (!_spriteCacheDictionary.ContainsKey(name))
            {
                _spriteCacheDictionary[name] = LoadSprite(name);
            }

            return _spriteCacheDictionary[name];
        }

        private Sprite LoadSprite(string name)
        {
            var imageData = _resolutionDocument
                .Elements("Resolutions")
                .Elements("ImageResolutions")
                .Elements("ImageResolution").FirstOrDefault(elems => elems.Attribute("name").Value.Equals(name));

            if (imageData == null)
            {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load image " + name + " failed.");
                return null;
            }

            var posData = imageData.Element("position");
            var sizeData = imageData.Element("size");

            if (posData == null || sizeData == null)
            {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load image " + name + " failed.");
                return null;
            }

            int texture, x, y, width, height;

            if (!int.TryParse(imageData.Attribute("texture")?.Value, out texture)
                || !int.TryParse(posData.Attribute("x")?.Value, out x)
                || !int.TryParse(posData.Attribute("y")?.Value, out y)
                || !int.TryParse(sizeData.Attribute("width")?.Value, out width)
                || !int.TryParse(sizeData.Attribute("height")?.Value, out height))
            {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load image " + name + "failed.");
                return null;
            }

            if (!_textureCacheDictionary.ContainsKey(texture))
            {
                _textureCacheDictionary[texture] = LoadTexture(texture.ToString());
            }

            return new Sprite(_textureCacheDictionary[texture], x, y, width, height);
        }

        private Texture LoadTexture(string id)
        {
            var textureData = _resolutionDocument
                .Elements("Resolutions")
                .Elements("TextureResolutions")
                .Elements("TextureResolution").FirstOrDefault(elems => elems.Attribute("id").Value.Equals(id));

            if (textureData == null)
            {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load texture " + id + " failed.");
                return null;
            }

            int textureNamespace;

            var name = textureData.Element("name")?.Value;
            var extension = textureData.Element("extension")?.Value;

            if (!int.TryParse(textureData.Attribute("namespace")?.Value, out textureNamespace)
                || name == null
                || extension == null)
            {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load texture " + id + " failed.");
                return null;
            }

            if (!_namespaceCacheDictionary.ContainsKey(textureNamespace))
            {
                _namespaceCacheDictionary[textureNamespace] = LoadNamespace(textureNamespace.ToString());
            }

            name += "." + extension;
            
            return new Texture(Path.Combine(_rootDirectory, _namespaceCacheDictionary[textureNamespace], name));
        }

        private string LoadNamespace(string id)
        {
            var namespaceData = _resolutionDocument
                .Elements("Resolutions")
                .Elements("NamespaceResolutions")
                .Elements("NamespaceResolution").FirstOrDefault(elems => elems.Attribute("id").Value.Equals(id));

            if (namespaceData == null)
            {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load namespace " + id + " failed");
                return null;
            }

            var name = namespaceData.Attribute("name")?.Value;

            if (name == null)
            {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load namespace " + id + " failed");
                return null;
            }

            return name;
        }
    }
}
