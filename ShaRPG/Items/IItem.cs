namespace ShaRPG.Items {
    public interface IItem {
        int Id { get; }
        string Name { get; }
        string Description { get; }
        string Graphic { get; }
    }
}
