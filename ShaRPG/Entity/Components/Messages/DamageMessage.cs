namespace ShaRPG.Entity.Components.Messages {
    public class DamageMessage : IComponentMessage {
        public readonly int Amount;
        
        public DamageMessage(int amount) {
            Amount = amount;
        }
    }
}
