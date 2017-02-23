using System;

namespace ShaRPG.Entity {
    public class EntityException : Exception {
        public EntityException(string message) : base(message) { }
        public EntityException(Entity e, string message)
            : base($"Entity exception in {e.Name}({e.Id}): {message}") { }
    }
}
