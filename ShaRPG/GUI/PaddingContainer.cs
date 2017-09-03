using SFML.Graphics;
using SFML.System;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

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

        public override void Render(RenderTarget renderSurface) {
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

        public ScreenCoordinate ChildScreenPosition(IGuiComponent component) {
            return ScreenPosition + new ScreenCoordinate(Padding, Padding);
        }

        public override void Clicked(ScreenCoordinate coordinates) {
            base.Clicked(coordinates);
            if (_paddedComponent.IsMouseOver(coordinates)) _paddedComponent.Clicked(coordinates);
        }

        private class PaddedContainer : IGuiComponentContainer {
            public IGuiComponentContainer Parent { get; set; }
            public int Width => Parent.Width - Padding * 2;
            public ScreenCoordinate ScreenPosition => Parent.ChildScreenPosition(this);
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

            public void Render(RenderTarget renderSurface) {
                renderSurface.WithOffset(new Vector2f(Padding, Padding), () => _component?.Render(renderSurface));
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

            public ScreenCoordinate ChildScreenPosition(IGuiComponent component) {
                return ScreenPosition;
            }

            public bool IsMouseOver(ScreenCoordinate coordinates) {
                return coordinates.Overlaps(ScreenPosition, Width, Height);
            }

            public void Clicked(ScreenCoordinate coordinates) {
                if (_component?.IsMouseOver(coordinates) ?? false) _component?.Clicked(coordinates);
            }
        }
    }
}
