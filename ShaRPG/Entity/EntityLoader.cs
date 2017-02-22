using System.IO;
using System.Xml.Linq;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity {
    public class EntityLoader {
        private readonly string _directory;
        private readonly IEntityIdAssigner _idAssigner;
        private readonly ISpriteStoreService _spriteStore;

        public EntityLoader(string directory, IEntityIdAssigner idAssigner, ISpriteStoreService spriteStore) {
            _directory = directory;
            _idAssigner = idAssigner;
            _spriteStore = spriteStore;
        }

        public Entity LoadEntity(string entityName, GameMap map, TileCoordinate position) {
            XDocument document;

            using (FileStream fs = File.OpenRead(Path.Combine(_directory, entityName + ".xml"))) {
                document = XDocument.Load(fs);
            }

            var entity = document?.Element("Entity");

            string name = entity?.Attribute("name")?.Value;
            string avatar = entity?.Attribute("avatar")?.Value;
            int health;
            string healthString = entity?.Attribute("maxHealth")?.Value;

            string spriteName = entity?.Element("EntityTextureInformation")?.Attribute("name")?.Value;

            if (spriteName == null || name == null || avatar == null || !int.TryParse(healthString, out health)) {
                throw new EntityException($"Unable to load entity {entityName}");
            }

            return new Entity(_idAssigner, name, health, position, _spriteStore.GetSprite(spriteName), map);
        }
    }
}
