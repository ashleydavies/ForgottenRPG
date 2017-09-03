using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SFML.Graphics;

namespace ShaRPG.Service {
    internal class CachedFileTextureStore : ITextureStore {
        private readonly Dictionary<int, string> _namespaceCacheDictionary = new Dictionary<int, string>();
        private readonly XDocument _resolutionDocument;
        private readonly string _rootDirectory;
        private readonly Dictionary<string, Texture> _textureCacheDirectory = new Dictionary<string, Texture>();
        private readonly Dictionary<int, string> _textureFileCacheDictionary = new Dictionary<int, string>();
        private readonly Texture _nullTexture = new Texture(1, 1);

        public CachedFileTextureStore(string directory) {
            _rootDirectory = directory;

            using (var fs = File.OpenRead(Path.Combine(directory, "Resolution.xml"))) {
                _resolutionDocument = XDocument.Load(fs);
            }
        }

        public Texture GetTexture(string name) {
            if (!_textureCacheDirectory.ContainsKey(name)) {
                _textureCacheDirectory[name] = LoadTexture(name);
                if (ReferenceEquals(_textureCacheDirectory[name], _nullTexture)) {
                    ServiceLocator.LogService.Log(LogType.Error, "Attempt to load image " + name + " failed.");
                }
            }

            return _textureCacheDirectory[name];
        }

        public Sprite GetNewSprite(string name) {
            return new Sprite(GetTexture(name));
        }

        private Texture LoadTexture(string name) {
            var imageData = _resolutionDocument
                .Elements("Resolutions")
                .Elements("ImageResolutions")
                .Elements("ImageResolution")
                .FirstOrDefault(elems => elems.Attribute("name").Value.Equals(name));

            if (imageData == null) return _nullTexture;

            var posData = imageData.Element("position");
            var sizeData = imageData.Element("size");

            if (posData == null || sizeData == null) return _nullTexture;

            int texture, x, y, width, height;

            if (!int.TryParse(imageData.Attribute("texture")?.Value, out texture)
                || !int.TryParse(posData.Attribute("x")?.Value, out x)
                || !int.TryParse(posData.Attribute("y")?.Value, out y)
                || !int.TryParse(sizeData.Attribute("width")?.Value, out width)
                || !int.TryParse(sizeData.Attribute("height")?.Value, out height)) return _nullTexture;

            if (!_textureFileCacheDictionary.ContainsKey(texture)) {
                _textureFileCacheDictionary[texture] = LoadTextureFile(texture.ToString());
            }

            return new Texture(Path.Combine(_textureFileCacheDictionary[texture]), new IntRect(x, y, width, height));
        }

        private string LoadTextureFile(string id) {
            var textureData = _resolutionDocument
                .Elements("Resolutions")
                .Elements("TextureResolutions")
                .Elements("TextureResolution")
                .FirstOrDefault(elems => elems.Attribute("id").Value.Equals(id));

            if (textureData == null) {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load texture " + id + " failed.");
                return null;
            }

            int textureNamespace;

            var name = textureData.Element("name")?.Value;
            var extension = textureData.Element("extension")?.Value;

            if (!int.TryParse(textureData.Attribute("namespace")?.Value, out textureNamespace)
                || name == null
                || extension == null) {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load texture " + id + " failed.");
                return null;
            }

            if (!_namespaceCacheDictionary.ContainsKey(textureNamespace)) {
                _namespaceCacheDictionary[textureNamespace] = LoadNamespace(textureNamespace.ToString());
            }

            name += $".{extension}";

            return Path.Combine(_rootDirectory, _namespaceCacheDictionary[textureNamespace], name);
        }

        private string LoadNamespace(string id) {
            var namespaceData = _resolutionDocument
                .Elements("Resolutions")
                .Elements("NamespaceResolutions")
                .Elements("NamespaceResolution")
                .FirstOrDefault(elems => elems.Attribute("id").Value.Equals(id));

            if (namespaceData == null) {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load namespace " + id + " failed");
                return null;
            }

            var name = namespaceData.Attribute("name")?.Value;

            if (name == null) {
                ServiceLocator.LogService.Log(LogType.Error, "Attempt to load namespace " + id + " failed");
                return null;
            }

            return name;
        }
    }
}
