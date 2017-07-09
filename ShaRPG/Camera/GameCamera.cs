using SFML.System;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Camera {
    internal class GameCamera : ICamera {
        public Vector2I Size { get; set; }
        public Vector2F Scale { get; set; } = new Vector2F(1, 1);
        public Vector2F Center { get; set; } = new Vector2F(0, 0);
        public Vector2I TopLeftPosition => ((Vector2I) Center) - (Size / 2);

        public GameCamera(Vector2I size) {
            Size = size;
        }
        
        public GameCoordinate TranslateScreenCoordinate(ScreenCoordinate screenCoordinate) {
            return new GameCoordinate(screenCoordinate.X + TopLeftPosition.X, screenCoordinate.Y + TopLeftPosition.Y);
        }
    }
}
