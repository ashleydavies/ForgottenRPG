using System;
using System.Collections.Generic;
using ShaRPG.Util;

namespace ShaRPG.Entity {
    internal class EntityManager : IEntityIdAssigner {
        public GameEntity Player => _playerId >= 0
                                    ? _entities[_playerId]
                                    : throw new EntityException("Unable to determine player - ID not set");

        private readonly Dictionary<int, GameEntity> _entities;
        private int _nextId;
        private int _playerId;

        public EntityManager() {
            _nextId = 0;
            _playerId = -1;
            _entities = new Dictionary<int, GameEntity>();
        }

        public int GetNextId(GameEntity e) {
            if (string.Equals(e.Name, Config.PlayerName, StringComparison.CurrentCultureIgnoreCase)) {
                if (_playerId != -1) throw new EntityException("Multiple players spawned?");
                _playerId = _nextId;
            }
            _entities[_nextId] = e;
            return _nextId++;
        }

        public GameEntity GetEntity(int id) {
            return _entities[id];
        }

        public void Update(float delta) {
            foreach (GameEntity e in _entities.Values) {
                e.Update(delta);
            }
        }

        public void Render(IRenderSurface renderSurface) {
            foreach (GameEntity e in _entities.Values) {
                e.Render(renderSurface);
            }
        }
    }
}
