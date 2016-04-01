using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShaRPG.Util;

namespace ShaRPG.Camera {
    class GameCamera : ICamera {
        public Vector2I Center { get; set; } = new Vector2I(0, 0);
    }
}
