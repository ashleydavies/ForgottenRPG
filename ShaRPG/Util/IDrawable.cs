using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Util {
    public interface IDrawable
    {
        void Draw(RenderWindow window, GameCoordinate position);
        void Update(float delta);
    }
}
