using System;
using SFML.Graphics;
using SFML.System;

namespace ShaRPG.Util {
    public static class RenderTargetExtensions {
        public static void WithOffset(this RenderTarget renderTarget, Vector2f offset, Action action) {
            View old = new View(renderTarget.GetView());
            View newView = new View(renderTarget.GetView());
            newView.Move(-offset);
            renderTarget.SetView(newView);
            action();
            renderTarget.SetView(old);
        }
        
        public static void WithView(this RenderTarget renderTarget, View view, Action action) {
            View old = new View(renderTarget.GetView());
            renderTarget.SetView(view);
            action();
            renderTarget.SetView(old);
        }
    }
}
