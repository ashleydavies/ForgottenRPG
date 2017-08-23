namespace ShaRPG.Items {
    public class AbstractItem : IItem {
        public int Id { get; }
        public string Name { get; }
        public string Codename { get; }
        public string Description { get; }

        public AbstractItem(int id, string name, string description, string codename) {
            Id = id;
            Name = name;
            Description = description;
            Codename = codename;
        }
    }
}
