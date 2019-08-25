using System;
using System.Collections.Generic;
using System.Linq;
using ForgottenRPG.Service;
using SFML.Graphics;
using SFML.System;

namespace ForgottenRPG.GUI {
    public class DebugGui {
        private GuiWindow _window;
        private TextContainer _log;

        public DebugGui(ITextureStore store) {
            _window = new GuiWindow(store, new Vector2i(350, 200), new Vector2i(660, 300));

            _log = new TextContainer("Debug Log", 22);
            _window.AddComponent(new PaddingContainer(10, _log));
        }

        public void AddLogText(string text) {
            _log.Contents = String.Join("\n", 
                                        _log.Contents
                                            .Split("\n")
                                            .TakeLast(10)
                                            .Concat(new List<string> { text }));
        }

        public void Render(RenderTarget renderSurface) {
            _window.Render(renderSurface);
        }
    }
}
