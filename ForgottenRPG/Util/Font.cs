namespace ForgottenRPG.Util {
    public class Font {
        internal SFML.Graphics.Font UnderlyingFont;

        public Font(string filename) {
            UnderlyingFont = new SFML.Graphics.Font(filename);
        }
    }
}
