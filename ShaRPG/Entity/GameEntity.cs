using System;
using System.Collections.Generic;
using System.Linq;
using ShaRPG.Entity.Components;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity {
    public class GameEntity {
        public string Name { get; }
        public int Id { get; }
        public TileCoordinate Position { get; set; }
        private readonly Sprite _sprite;
        private readonly List<IComponent> _components = new List<IComponent>();

        private GameCoordinate RenderOffset => new GameCoordinate(- _sprite.Width / 2,
                                                                  -_sprite.Height + MapTile.Height / 2)
                                               + GetComponent<MovementComponent>()?.RenderOffset;
        
        public GameEntity(IEntityIdAssigner idAssigner, string name, TileCoordinate position, Sprite sprite) {
            Name = name;
            Position = position;
            _sprite = sprite;
            Id = idAssigner.GetNextId(this);

            ServiceLocator.LogService.Log(LogType.Information, $"Entity {name} spawned at {Position}");
        }

        public void AddComponent(IComponent component) => _components.Add(component);
        public void AddComponents(params IComponent[] components) => components.ToList().ForEach(AddComponent);
        public T GetComponent<T>() where T : class, IComponent => _components.OfType<T>().FirstOrDefault();

        public void Update(float delta) {
            _components.ForEach(x => x.Update(delta));
        }

        public void Render(IRenderSurface renderSurface) {
            _components.ForEach(x => x.Render(renderSurface));
            renderSurface.Render(_sprite, (GameCoordinate) Position + RenderOffset);
        }
    }
}
