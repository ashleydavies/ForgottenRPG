using System.IO;

namespace ShaRPG {
    public class Config {
        public static readonly string EntityDataDirectory = Path.Combine("resources", "data", "xml", "entity");
        public static readonly string MapDataDirectory = Path.Combine("resources", "data", "xml", "map");
        public static readonly string PlayerName = "player";
    }
}
