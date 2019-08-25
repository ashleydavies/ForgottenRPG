using System;
using System.Collections.Generic;
using System.Linq;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;
using SFML.System;
using ForgottenRPG.Util;

namespace ForgottenRPG.GUI {
    public class FixedContainer : AbstractGuiComponent, IGuiComponentContainer {
        public override int Height => _components.Aggregate(0, (i, c) => Math.Max(i, c.Height));
        public override int Width => _components.Aggregate(0, (i, c) => Math.Max(i, c.Width));
        public int Top { get; set; }
        public int Left { get; set; }
        private readonly List<IGuiComponent> _components = new List<IGuiComponent>();

        public FixedContainer() { }

        public FixedContainer(int top, int left, IGuiComponent component) {
            Top = top;
            Left = left;
            AddComponent(component);
        }

        public override void Render(RenderTarget renderSurface) {
            renderSurface.WithOffset(new Vector2f(Top, Left),
                                     () => _components.ForEach(component => component.Render(renderSurface)));
        }

        public override void Reflow() { }

        public void AddComponent(IGuiComponent component) {
            _components.Add(component);
            component.Parent = this;
        }

        public void RemoveComponent(IGuiComponent component) {
            if (!_components.Contains(component)) throw new GuiException("Invalid attempt to remove component");

            _components.Remove(component);
        }

        public bool HasComponent(IGuiComponent component) {
            return _components.Contains(component);
        }

        public ScreenCoordinate ChildScreenPosition(IGuiComponent component) {
            return Parent.ChildScreenPosition(this) + new ScreenCoordinate(Top, Left);
        }

        public override bool IsMouseOver(ScreenCoordinate coordinates) {
            return coordinates.Overlaps(ScreenPosition + new ScreenCoordinate(Top, Left), Width, Height);
        }

        public override void Clicked(ScreenCoordinate coordinates) {
            foreach (IGuiComponent component in _components.Where(x => x.IsMouseOver(coordinates))) {
                component.Clicked(coordinates);
            }
        }
    }
}
