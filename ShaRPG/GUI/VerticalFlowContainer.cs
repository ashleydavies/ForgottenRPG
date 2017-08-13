using System;
using System.Collections.Generic;
using System.Linq;
using ShaRPG.Util;

namespace ShaRPG.GUI {
    public class VerticalFlowContainer : AbstractGuiComponent, IGuiComponentContainer {
        public override int Height => _components.Aggregate(0, (h, component) => h + component.Height);
        public override int Width => Parent?.Width ?? 0;
        private readonly List<IGuiComponent> _components = new List<IGuiComponent>();

        public override void Render(IRenderSurface renderSurface) {
            int h = 0;

            foreach (IGuiComponent component in _components) {
                using (new RenderSurfaceOffset(renderSurface, new Vector2I(0, h))) {
                    component.Render(renderSurface);
                }
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

        public override void Reflow() {
            _components.ForEach(c => c.Reflow());
        }
    }
}
