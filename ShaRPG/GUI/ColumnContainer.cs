using System;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GUI {
    public class ColumnContainer : AbstractGuiComponent, IGuiComponentContainer {
        public enum Side {
            Left,
            Right
        }

        public override int Height => Math.Max(_left?.Height ?? 0, _right?.Height ?? 0);
        public override int Width => Parent?.Width ?? 0;
        private readonly ColumnContainerColumn _left;
        private readonly ColumnContainerColumn _right;
        private readonly Side _fixedColumn;
        private readonly int _fixedWidth;
        private int LeftWidth => _fixedColumn == Side.Left ? _fixedWidth : Width - _fixedWidth;
        private int RightWidth => _fixedColumn == Side.Right ? _fixedWidth : Width - _fixedWidth;
        private int ColumnWidth(Side side) => side == Side.Left ? LeftWidth : RightWidth;

        public ColumnContainer(Side fixedColumn, int fixedWidth) {
            _fixedColumn = fixedColumn;
            _fixedWidth = fixedWidth;
            _left = new ColumnContainerColumn(this, Side.Left);
            _right = new ColumnContainerColumn(this, Side.Right);
        }

        public override void Render(IRenderSurface renderSurface) {
            _left.Render(renderSurface);

            using (new RenderSurfaceOffset(renderSurface, new Vector2I(LeftWidth, 0))) {
                _right.Render(renderSurface);
            }
        }

        public override void Reflow() {
            _left.Reflow();
            _right.Reflow();
        }

        public void AddComponent(IGuiComponent component) {
            if (component is ColumnContainerColumn) return;
            throw new GuiException("Cannot add components directly to column container");
        }

        public void RemoveComponent(IGuiComponent component) {
            throw new GuiException("Cannot remove components directly from column container");
        }

        public bool HasComponent(IGuiComponent component) {
            return (_left?.HasComponent(component) ?? false) || (_right?.HasComponent(component) ?? false);
        }

        public ScreenCoordinate ChildScreenPosition(IGuiComponent component) {
            if (component == _left) return ScreenPosition;

            return ScreenPosition + new ScreenCoordinate(LeftWidth, 0);
        }

        public void SetLeftComponent(IGuiComponent component) {
            _left.AddComponent(component);
        }

        public void SetRightComponent(IGuiComponent component) {
            _right.AddComponent(component);
        }

        private class ColumnContainerColumn : AbstractGuiComponent, IGuiComponentContainer {
            public override int Height => _component.Height;
            public override int Width => _container.ColumnWidth(_side);
            private readonly Side _side;
            private readonly ColumnContainer _container;
            private IGuiComponent _component;

            public ColumnContainerColumn(ColumnContainer container, Side side) {
                Parent = _container = container;
                _side = side;
            }

            public override void Render(IRenderSurface renderSurface) {
                _component?.Render(renderSurface);
            }

            public override void Reflow() {
                _component?.Reflow();
            }

            public void AddComponent(IGuiComponent component) {
                if (_component != null && component != _component) _component.Parent = null;

                _component = component;
                _component.Parent = this;
            }

            public void RemoveComponent(IGuiComponent component) {
                if (component != _component) return;

                _component.Parent = null;
                _component = null;
            }

            public bool HasComponent(IGuiComponent component) {
                return _component == component;
            }

            public ScreenCoordinate ChildScreenPosition(IGuiComponent component) {
                return ScreenPosition;
            }
        }
    }
}
