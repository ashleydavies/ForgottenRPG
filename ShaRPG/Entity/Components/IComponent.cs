namespace ShaRPG.Entity.Components
{
    public interface IComponent
    {
        void Update();
        void Message(IComponentMessage componentMessage);
    }
}