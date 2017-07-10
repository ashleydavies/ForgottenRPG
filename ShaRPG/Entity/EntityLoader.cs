using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using ShaRPG.Entity.Components;
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

        public GameEntity LoadEntity(string entityName, GameMap map, TileCoordinate position, List<TileCoordinate> path) {
            XDocument document;

            using (FileStream fs = File.OpenRead(Path.Combine(_directory, entityName + ".xml"))) {
                document = XDocument.Load(fs);
            }

            var entityInformation = document.Element("Entity");

            string name = entityInformation?.Attribute("name")?.Value;
            string avatar = entityInformation?.Attribute("avatar")?.Value;
            int health;
            string healthString = entityInformation?.Attribute("maxHealth")?.Value;

            string spriteName = entityInformation?.Element("EntityTextureInformation")?.Attribute("name")?.Value;

            if (spriteName == null || name == null || avatar == null || !int.TryParse(healthString, out health)) {
                throw new EntityException($"Unable to load entity {entityName}");
            }

            GameEntity entity = new GameEntity(_idAssigner, name, position, _spriteStore.GetSprite(spriteName));
            entity.AddComponent(new HealthComponent(entity, health));
            entity.AddComponent(new MovementComponent(entity, map));
            if (path.Count > 0) entity.AddComponent(new PathFollowingComponent(entity, path));
            return entity;
        }
    }
}
