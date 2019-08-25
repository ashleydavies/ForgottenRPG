using ForgottenRPG.Entity.Components.Messages;

namespace ForgottenRPG.Entity.Components {
    public interface IMessageHandler<T> where T: IComponentMessage {
        void Message(T message);
    }
}
