using System;
using System.Collections.Generic;
using System.Linq;
using ForgottenRPG.Service;
using SFML.Graphics;
using SFML.System;

namespace ForgottenRPG.GUI {
    public class DebugGUI {
        private GuiWindow _window;
        private TextContainer _log;

        public DebugGUI(ITextureStore store) {
            _window = new GuiWindow(store, new Vector2i(320, 120), new Vector2i(600, 180));

            _log = new TextContainer("Debug Log", 16);
            _window.AddComponent(new PaddingContainer(10, _log));
        }

        public void AddLogText(string text) {
            _log.Contents = String.Join("\n", 
                                        _log.Contents
                                            .Split("\n")
                                            .TakeLast(8)
                                            .Concat(new List<string> { text }));
        }

        public void Render(RenderTarget renderSurface) {
            _window.Render(renderSurface);
        }
    }
}
