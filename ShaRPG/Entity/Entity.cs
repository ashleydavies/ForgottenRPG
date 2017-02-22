using System;
using System.Collections.Generic;
using System.Linq;
using ShaRPG.Entity.Components;
using ShaRPG.Map;
using ShaRPG.Map.Pathfinding;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity {
    public class Entity {
        public string Name { get; }
        public TileCoordinate Position { get; private set; }
        public float Health {
            get { return _health; }
            set {
                if (Dead) throw new EntityException(this, "Dead entity HP cannot change");

                _health = value;

                if (Dead) Death?.Invoke();
            }
        }
        public event Action Death;
        public int Id { get; }
        public bool Dead => Health <= 0;
        private GameCoordinate RenderOffset => new GameCoordinate(MapTile.Width / 2 - _sprite.Width / 2,
                                                                  -_sprite.Height + MapTile.Height / 2);

        private TileCoordinate _targetPosition;
        private int _pathIndex;
        private float _health;
        private readonly List<TileCoordinate> _path;
        private readonly int _maxHealth;
        private readonly Sprite _sprite;
        private readonly GameMap _map;
        private readonly List<IComponent> _components = new List<IComponent>();

        public Entity(IEntityIdAssigner idAssigner, string name, int maxHealth, TileCoordinate position, Sprite sprite,
                      GameMap map, List<TileCoordinate> path) {
            Id = idAssigner.GetNextId(this);
            Name = name;
            _health = _maxHealth = maxHealth;
            Position = _targetPosition = position;
            _map = map;
            _sprite = sprite;
            _path = path;

            ServiceLocator.LogService.Log(LogType.Information, $"Entity {name} spawned at {Position}");
        }

        public void AddComponent(IComponent component) => _components.Add(component);
        public void AddComponents(params IComponent[] components) => components.ToList().ForEach(AddComponent);
        public T GetComponent<T>() where T : class, IComponent => _components.OfType<T>().FirstOrDefault();

        public void Update(float delta) {
            _components.ForEach(x => x.Update());

            if (_targetPosition.Equals(Position) && _path.Count > 0) {
                _targetPosition = _path[_pathIndex++];

                if (_pathIndex >= _path.Count) _pathIndex = 0;
            }

            if (!_targetPosition.Equals(Position)) {
                List<TileCoordinate> path = _map.GetPath(Position, _targetPosition);
                Position = path[0];
            }

            Health += delta * 1;
        }

        public void Render(IRenderSurface renderSurface) {
            _components.ForEach(x => x.Render(renderSurface));
            renderSurface.Render(_sprite, Position + RenderOffset);
        }
    }
}
