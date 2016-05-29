#region

using System.Collections.Generic;
using System.Linq;
using ShaRPG.Entity.Components;
using ShaRPG.Util;

#endregion

namespace ShaRPG.Entity {
    public class Entity {
        private readonly List<IComponent> _components = new List<IComponent>();

        public Entity(IEntityIdAssigner idAssigner) {
            Id = idAssigner.GetNextId(this);
        }

        public int Id { get; }

        public void Update() {
            _components.ForEach(x => x.Update());
        }

        public void Render(IRenderSurface renderSurface) {
            _components.ForEach(x => x.Render(renderSurface));
        }

        public void AddComponent(IComponent component) => _components.Add(component);
        public void AddComponents(params IComponent[] components) => components.ToList().ForEach(AddComponent);
        public T GetComponent<T>() where T : class, IComponent => _components.OfType<T>().FirstOrDefault();
    }
}
