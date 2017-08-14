
using System;
using ShaRPG.Util;

namespace ShaRPG.GUI {
    public abstract class AbstractGuiComponent : IGuiComponent {
        private IGuiComponentContainer _parent;
        public IGuiComponentContainer Parent {
            get => _parent;
            set {
                if (_parent == value) return;

                _parent?.RemoveComponent(this);
                _parent = value;
                if (_parent != null && !_parent.HasComponent(this)) _parent?.AddComponent(this);
                Reflow();
            }
        }
        public abstract int Height { get; }
        public abstract int Width { get; }
        public abstract void Render(IRenderSurface renderSurface);
        public abstract void Reflow();
        public void ReflowAll() {
            IGuiComponent component = this;
            while (component.Parent != null) component = component.Parent;
            component.Reflow();
        }
    }
}
