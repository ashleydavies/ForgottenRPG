#region

using SFML.Graphics;
using ShaRPG.Util.Coordinate;

#endregion

namespace ShaRPG.Util {
    public interface IDrawable {
        void Draw(RenderWindow window, GameCoordinate position);
        void Update(float delta);
    }
}
