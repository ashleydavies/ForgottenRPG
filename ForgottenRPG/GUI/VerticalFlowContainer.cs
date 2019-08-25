using System.Collections.Generic;
using System.Linq;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;
using SFML.System;
using ForgottenRPG.Util;

namespace ForgottenRPG.GUI {
    public class VerticalFlowContainer : AbstractGuiComponent, IGuiComponentContainer {
        public override int Height => _components.Aggregate(0, (h, component) => h + component.Height);
        public override int Width => Parent?.Width ?? 0;
        private readonly List<IGuiComponent> _components = new List<IGuiComponent>();

        public override void Render(RenderTarget renderSurface) {
            int h = 0;

            foreach (IGuiComponent component in _components) {
                renderSurface.WithOffset(new Vector2f(0, h), () => component.Render(renderSurface));
                h += component.Height;
            }
        }

        public void AddComponent(IGuiComponent component) {
            _components.Add(component);
            component.Parent = this;
        }

        public void RemoveComponent(IGuiComponent component) {
            _components.Remove(component);
            component.Parent = null;
        }

        public bool HasComponent(IGuiComponent component) {
            return _components.Contains(component);
        }

        public ScreenCoordinate ChildScreenPosition(IGuiComponent child) {
            int h = 0;
            foreach (var component in _components) {
                if (component == child) return ScreenPosition + new ScreenCoordinate(0, h);
                h += component.Height;
            }
            throw new GuiException($"{child} is not a child of vertical flow container");
        }

        public void Clicked(ScreenCoordinate coordinates) {
            base.Clicked(coordinates);
            _components.ForEach(c => {
                if (c.IsMouseOver(coordinates)) c.Clicked(coordinates);
            });
        }

        public override void Reflow() {
            _components.ForEach(c => c.Reflow());
        }

        public void Clear() {
            _components.Clear();
        }
    }
}
