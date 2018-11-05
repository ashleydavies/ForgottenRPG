using System.Globalization;
using SFML.Graphics;

namespace ShaRPG.Items {
    public class AbstractItem : IItem {
        private static TextInfo _textInfo = new CultureInfo("en-GB", false).TextInfo;
        public int Id { get; }
        public string Name { get; }
        public string Codename { get; }
        public string DisplayName => _textInfo.ToTitleCase(Codename.Replace('_', ' '));
        public string Description { get; }
        public Sprite Texture { get; }

        public AbstractItem(int id, string name, string description, string codename, Sprite sprite) {
            Id = id;
            Name = name;
            Description = description;
            Codename = codename;
            Texture = sprite;
        }
    }
}
