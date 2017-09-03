using System;
using SFML.Graphics;
using ShaRPG.Util.Coordinate;

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
        public ScreenCoordinate ScreenPosition => _parent.ChildScreenPosition(this);
        public abstract int Height { get; }
        public abstract int Width { get; }
        public abstract void Render(RenderTarget renderSurface);
        public abstract void Reflow();
        public event Action<ScreenCoordinate> OnClicked;
        
        public void ReflowAll() {
            IGuiComponent component = this;
            while (component.Parent != null) component = component.Parent;
            component.Reflow();
        }

        public bool IsMouseOver(ScreenCoordinate coordinates) {
            return coordinates.Overlaps(ScreenPosition, Width, Height);
        }

        public virtual void Clicked(ScreenCoordinate coordinates) {
            OnClicked?.Invoke(coordinates);
        }
    }
}
