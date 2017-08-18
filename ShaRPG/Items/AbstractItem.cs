namespace ShaRPG.Items {
    public class AbstractItem : IItem {
        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string Graphic { get; }

        public AbstractItem(int id, string name, string description, string graphic) {
            Id = id;
            Name = name;
            Description = description;
            Graphic = graphic;
        }
    }
}
