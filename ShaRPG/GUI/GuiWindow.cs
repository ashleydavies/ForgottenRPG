using System;
using System.Collections.Generic;
using System.Threading;
using SFML.Graphics;
using SFML.System;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;
using Sprite = SFML.Graphics.Sprite;

namespace ShaRPG.GUI {
    public class GuiWindow : IGuiComponentContainer {
        public IGuiComponentContainer Parent {
            get => null;
            set => throw new NotImplementedException();
        }

        // TODO: Sort out the coordinate mess we have going on here...
        public ScreenCoordinate ScreenPosition {
            get => new ScreenCoordinate(Position);
            set => _center = new Vector2i(value.X, value.Y) + _size / 2;
        }

        public int Width => _size.X;
        public int Height => _size.Y;

        private const int BorderTSize = 12;
        private const int BgTSize = 60;
        private Sprite _backgroundSprite;
        private Vector2i Position => _center - _size / 2;
        private Vector2i _center;
        private readonly Vector2i _size;
        private readonly List<IGuiComponent> _components = new List<IGuiComponent>();
        private RenderTexture _renderSurface;

        public GuiWindow(ITextureStore textureStore, Vector2i center, Vector2i size) {
            if (size.X % BgTSize != 0 || size.Y % BgTSize != 0)
                throw new GuiException("Bad size for GUI window");

            _center = center;
            _size = size;
            RenderBackground(textureStore);
        }

        public void Render(RenderTarget renderSurface) {
            renderSurface.WithOffset((Vector2f) Position,
                                     () => {
                                         renderSurface.WithOffset(new Vector2f(-BorderTSize, -BorderTSize),
                                                                  () => { renderSurface.Draw(_backgroundSprite); });
                                         _components.ForEach(component => component.Render(renderSurface));
                                     });
        }

        public void Reflow() {
            _components.ForEach(c => c.Reflow());
        }

        public void AddComponent(IGuiComponent component) {
            _components.Add(component);
            component.Parent = this;
        }

        public void RemoveComponent(IGuiComponent component) {
            _components.Remove(component);
            if (component.Parent == this) component.Parent = null;
        }

        public bool HasComponent(IGuiComponent component) {
            return _components.Contains(component);
        }

        public ScreenCoordinate ChildScreenPosition(IGuiComponent component) {
            return new ScreenCoordinate(Position);
        }

        public bool IsMouseOver(ScreenCoordinate coordinates) {
            return coordinates.Overlaps(new ScreenCoordinate(Position), Width, Height);
        }

        public void Clicked(ScreenCoordinate coordinates) {
            _components.ForEach(c => {
                if (c.IsMouseOver(coordinates)) c.Clicked(coordinates);
            });
        }

        private void RenderBackground(ITextureStore textureStore) {
            Sprite cornerTl = textureStore.GetNewSprite("ui_corner_tl");
            Sprite cornerTr = textureStore.GetNewSprite("ui_corner_tr");
            Sprite cornerBl = textureStore.GetNewSprite("ui_corner_bl");
            Sprite cornerBr = textureStore.GetNewSprite("ui_corner_br");
            Sprite sideT = textureStore.GetNewSprite("ui_side_t");
            Sprite sideB = textureStore.GetNewSprite("ui_side_b");
            Sprite sideL = textureStore.GetNewSprite("ui_side_l");
            Sprite sideR = textureStore.GetNewSprite("ui_side_r");
            Sprite background = textureStore.GetNewSprite("ui_background");

            cornerTr.Position = new Vector2f(Width + BorderTSize, 0);
            cornerBl.Position = new Vector2f(0, Height + BorderTSize);
            cornerBr.Position = new Vector2f(Width + BorderTSize, Height + BorderTSize);
            sideR.Position = new Vector2f(Width + BorderTSize, 0);
            sideB.Position = new Vector2f(0, Height + BorderTSize);
            background.Position = new Vector2f(BorderTSize, BorderTSize);
            sideT.Texture.Repeated = true;
            sideB.Texture.Repeated = true;
            sideL.Texture.Repeated = true;
            sideR.Texture.Repeated = true;
            background.Texture.Repeated = true;
            sideT.TextureRect = new IntRect(0, 0, Width + BorderTSize * 2, BorderTSize);
            sideB.TextureRect = new IntRect(0, 0, Width + BorderTSize * 2, BorderTSize);
            sideL.TextureRect = new IntRect(0, 0, BorderTSize, Height + BorderTSize * 2);
            sideR.TextureRect = new IntRect(0, 0, BorderTSize, Height + BorderTSize * 2);
            background.TextureRect = new IntRect(0, 0, Width, Height);

            _renderSurface = new RenderTexture((uint) (Width + BorderTSize * 2),
                                               (uint) (Height + BorderTSize * 2));

            _renderSurface.Draw(sideT);
            _renderSurface.Draw(sideB);
            _renderSurface.Draw(sideL);
            _renderSurface.Draw(sideR);
            _renderSurface.Draw(cornerTl);
            _renderSurface.Draw(cornerTr);
            _renderSurface.Draw(cornerBl);
            _renderSurface.Draw(cornerBr);
            _renderSurface.Draw(background);
            _renderSurface.Display();

            _backgroundSprite = new Sprite(_renderSurface.Texture);
        }
    }
}
