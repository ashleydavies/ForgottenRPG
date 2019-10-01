using System.Collections.Generic;
using System.Linq;
using ForgottenRPG.Entity.Components;
using ForgottenRPG.Entity.Components.Messages;
using ForgottenRPG.Map;
using ForgottenRPG.Service;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;

namespace ForgottenRPG.Entity {
    public class GameEntity {
        public string Name { get; }
        public int Id { get; }
        public TileCoordinate Position { get; set; }
        public bool ActionBlocked() => _manager.FightMode && GetComponent<CombatComponent>().Ap <= 0;

        private readonly Sprite _sprite;
        private readonly List<IComponent> _components = new List<IComponent>();
        private EntityManager _manager;

        public GameCoordinate RenderPosition => (GameCoordinate) Position + RenderOffset;

        private GameCoordinate RenderOffset => new GameCoordinate(-_sprite.TextureRect.Width / 2,
                                                                  -_sprite.TextureRect.Height + MapTile.Height / 2)
                                               + GetComponent<MovementComponent>()?.RenderOffset;

        public bool FightMode => _manager.FightMode;

        public GameEntity(EntityManager manager, string name, TileCoordinate position, Sprite sprite) {
            Name = name;
            Position = position;
            _sprite = sprite;
            _manager = manager;
            Id = manager.GetNextId(this);

            ServiceLocator.LogService.Log(LogType.Info, $"Entity {name} spawned at {Position}");
        }

        public void AddComponent(IComponent component) => _components.Add(component);
        public void AddComponents(params IComponent[] components) => components.ToList().ForEach(AddComponent);
        public T GetComponent<T>() where T : class, IComponent => _components.OfType<T>().FirstOrDefault();
        public List<IComponent> GetComponents() => _components;

        public void Update(float delta) {
            _components.ForEach(x => x.Update(delta));
        }

        public void Render(RenderTarget renderSurface) {
            _components.ForEach(x => x.Render(renderSurface));
            _sprite.Position = RenderPosition;
            renderSurface.Draw(_sprite);
        }

        public void SendMessage<T>(T message) where T : IComponentMessage {
            _components.OfType<IMessageHandler<T>>().ToList().ForEach(x => x.Message(message));
        }

        public bool MouseOver(GameCoordinate position) {
            return position.Overlaps(RenderPosition, _sprite.TextureRect.Width, _sprite.TextureRect.Height);
        }

        public override string ToString() {
            return $"GameEntity<{Name}>";
        }

        public bool IsAdjacent(GameEntity other) {
            return Position.ManhattanDistance(other.Position) == 1;
        }
    }
}
