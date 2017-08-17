namespace ShaRPG.Util {
    public class Color {
        private byte r;
        private byte g;
        private byte b;
        
        public static Color Black => new Color(0, 0, 0);
        public static Color White => new Color(255, 255, 255);
        public static Color Blue => new Color(0, 0, 255);
        
        public Color(byte r, byte g, byte b) {
            this.r = r;
            this.g = g;
            this.b = b;
        }
        
        public SFML.Graphics.Color UnderlyingColor => new SFML.Graphics.Color(r, g, b);
    }
}
