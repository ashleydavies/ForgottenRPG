using System.IO;
using ShaRPG.Util;

namespace ShaRPG {
    public class Config {
        public static readonly string EntityDataDirectory = Path.Combine("resources", "data", "xml", "entity");
        public static readonly string MapDataDirectory = Path.Combine("resources", "data", "xml", "map");
        public static readonly string FontDirectory = Path.Combine("resources", "font");
        public static readonly string PlayerName = "player";
        
        public static readonly Font GuiFont = new Font(Path.Combine(FontDirectory, "LiberationSans-Regular.ttf"));
    }
}
