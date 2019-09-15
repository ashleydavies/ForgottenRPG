namespace ForgottenRPG.Entity.Components.Messages {
    public class DamageMessage : IComponentMessage {
        public readonly GameEntity Source;
        public readonly int Amount;
        
        public DamageMessage(int amount, GameEntity source) {
            Amount = amount;
            Source = source;
        }
    }
}
