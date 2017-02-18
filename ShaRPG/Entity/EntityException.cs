using System;

namespace ShaRPG.Entity {
    public class EntityException : Exception {
        public EntityException(Entity e, string message)
            : base($"Entity exception in {e.Name}({e.Id}): {message}") { }
    }
}
