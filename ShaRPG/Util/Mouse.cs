using SFML.System;
using SFML.Window;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Util {
    public class Mouse {
        public static Window SFMLWindow { get; set; }

        public static ScreenCoordinate Position {
            get {
                Vector2i mousePosition = SFML.Window.Mouse.GetPosition(SFMLWindow);
                return new ScreenCoordinate(mousePosition.X, mousePosition.Y);
            }
        }
    }
}
