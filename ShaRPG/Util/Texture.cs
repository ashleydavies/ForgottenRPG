using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaRPG.Util {
    public class Texture {
        public SFML.Graphics.Texture UnderlyingTexture { get; set; }

        public Texture(string path)
        {
            UnderlyingTexture = new SFML.Graphics.Texture(path);
        }
    }
}
