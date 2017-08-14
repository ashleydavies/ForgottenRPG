using System;
using ShaRPG.Util;

namespace ShaRPG.GUI {
    public class PaddingContainer : AbstractGuiComponent, IGuiComponentContainer {
        public int Padding {
            get => _paddedComponent.Padding;
            set => _paddedComponent.Padding = value;
        }
        public override int Height => _paddedComponent.Height + Padding * 2;
        public override int Width => Parent?.Width ?? 0;
        private readonly PaddedContainer _paddedComponent;

        public PaddingContainer(int padding) : this(padding, null) { }

        public PaddingContainer(int padding, IGuiComponent component) {
            _paddedComponent = new PaddedContainer(this, padding);
            if (component != null) AddComponent(component);
        }

        public override void Render(IRenderSurface renderSurface) {
            _paddedComponent.Render(renderSurface);
        }

        public override void Reflow() {
            _paddedComponent.Reflow();
        }
        
        public void AddComponent(IGuiComponent component) {
            _paddedComponent.AddComponent(component);
        }

        public void RemoveComponent(IGuiComponent component) {
            _paddedComponent.RemoveComponent(component);
        }

        public bool HasComponent(IGuiComponent component) {
            return _paddedComponent.HasComponent(component);
        }

        private class PaddedContainer : IGuiComponentContainer {
            public IGuiComponentContainer Parent { get; set; }
            public int Width => Parent.Width - Padding * 2;
            public int Height => _component?.Height ?? 0;
            public int Padding {
                get => _padding;
                set { _padding = value; Reflow(); }
            }
            private IGuiComponent _component;
            private int _padding;

            public PaddedContainer(PaddingContainer parent, int padding) {
                Parent = parent;
                Padding = padding;
            }

            public void Render(IRenderSurface renderSurface) {
                using (new RenderSurfaceOffset(renderSurface, new Vector2I(Padding, Padding))) {
                    _component.Render(renderSurface);
                }
            }

            public void Reflow() {
                _component?.Reflow();
            }

            public void AddComponent(IGuiComponent component) {
                if (_component != null && component != _component) _component.Parent = null;

                _component = component;
                _component.Parent = this;
            }

            public void RemoveComponent(IGuiComponent component) {
                if (component == _component) {
                    _component.Parent = null;
                    _component = null;
                }
            }

            public bool HasComponent(IGuiComponent component) {
                return _component == component;
            }
        }
    }
}
