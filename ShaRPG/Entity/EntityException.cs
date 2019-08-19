using System;
using System.Linq;

namespace ShaRPG.Entity {
    public class EntityException : Exception {
        public EntityException(string message) : base(message) { }
        public EntityException(GameEntity e, string message)
            : base($"Entity exception in {e.Name}({e.Id}){{{string.Join(',', e.GetComponents().Select(x => x.GetType().Name))}}}: {message}") { }
    }
}
