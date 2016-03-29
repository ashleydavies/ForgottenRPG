using System.Collections.Generic;
using System.Linq;
using ShaRPG.Entity.Components;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity
{
    public abstract class Entity
    {
        private readonly List<IComponent> _components = new List<IComponent>();

        public abstract void Update();

        public void AddComponent(IComponent component) => _components.Add(component);
        public void AddComponents(params IComponent[] components) => components.ToList().ForEach(AddComponent);
        public T GetComponent<T>() where T : class, IComponent => _components.OfType<T>().FirstOrDefault();
    }
}