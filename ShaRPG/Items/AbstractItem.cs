using SFML.Graphics;

namespace ShaRPG.Items {
    public class AbstractItem : IItem {
        public int Id { get; }
        public string Name { get; }
        public string Codename { get; }
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
