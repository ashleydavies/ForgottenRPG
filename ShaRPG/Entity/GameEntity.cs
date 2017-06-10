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
        private const float PositionLerpTime = 0.5f;
        private const float PositionLerpTimeMultiplier = 1 / PositionLerpTime;

        public string Name { get; }
        public TileCoordinate Position { get; private set; }
        public TileCoordinate PreviousPosition { get; private set; }
        public int Id { get; }
        private TileCoordinate _targetPosition;
        private int _pathIndex;
        private float _positionLerpTime;
        private float PositionLerpFraction => _positionLerpTime * PositionLerpTimeMultiplier;
        private readonly List<TileCoordinate> _path;
        private readonly Sprite _sprite;
        private readonly GameMap _map;
        private readonly List<IComponent> _components = new List<IComponent>();

        private GameCoordinate RenderOffset => new GameCoordinate(MapTile.Width / 2 - _sprite.Width / 2,
                                                                  -_sprite.Height + MapTile.Height / 2)
                                               + ((GameCoordinate) PreviousPosition - Position) * PositionLerpFraction;

        public GameEntity(IEntityIdAssigner idAssigner, string name, TileCoordinate position, Sprite sprite,
                      GameMap map, List<TileCoordinate> path) {
            Name = name;
            Position = PreviousPosition = _targetPosition = position;
            _positionLerpTime = 0;
            _map = map;
            _sprite = sprite;
            _path = path;
            Id = idAssigner.GetNextId(this);

            ServiceLocator.LogService.Log(LogType.Information, $"Entity {name} spawned at {Position}");
        }

        public void AddComponent(IComponent component) => _components.Add(component);
        public void AddComponents(params IComponent[] components) => components.ToList().ForEach(AddComponent);
        public T GetComponent<T>() where T : class, IComponent => _components.OfType<T>().FirstOrDefault();

        public void Update(float delta) {
            _components.ForEach(x => x.Update(delta));

            if (_targetPosition.Equals(Position) && _path.Count > 0) {
                _targetPosition = _path[_pathIndex++];

                if (_pathIndex >= _path.Count) _pathIndex = 0;
            }

            if (!_targetPosition.Equals(Position)) {
                if (_positionLerpTime <= 0) {
                    List<TileCoordinate> path = _map.GetPath(Position, _targetPosition);
                    PreviousPosition = Position;
                    Position = path[0];
                    _positionLerpTime = PositionLerpTime;
                } else {
                    _positionLerpTime -= delta;
                }
            }
        }

        public void Render(IRenderSurface renderSurface) {
            _components.ForEach(x => x.Render(renderSurface));
            renderSurface.Render(_sprite, Position + RenderOffset);
        }
    }
}
