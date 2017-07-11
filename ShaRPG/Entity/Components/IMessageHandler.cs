namespace ShaRPG.Entity.Components {
    public interface IMessageHandler<T> where T: IComponentMessage {
        void Message(T message);
    }
}
