namespace ForgottenRPG.Util {
    public class Texture {
        public Texture(string path) {
            UnderlyingTexture = new SFML.Graphics.Texture(path);
        }

        public SFML.Graphics.Texture UnderlyingTexture { get; set; }
    }
}
