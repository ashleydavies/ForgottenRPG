#region

using System.Collections.Generic;

#endregion

namespace ShaRPG.Entity {
    internal class EntityManager : IEntityIdAssigner {
        private readonly Dictionary<int, Entity> _entities;
        private int _nextId;

        public EntityManager() {
            _nextId = 0;
            _entities = new Dictionary<int, Entity>();
        }

        public int GetNextId(Entity e) {
            _entities[_nextId] = e;
            return _nextId++;
        }

        public Entity GetEntity(int id) {
            return _entities[id];
        }

        public void Update() {
            foreach (var e in _entities.Values) {
                e.Update();
            }
        }
    }
}
