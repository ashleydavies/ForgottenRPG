namespace ForgottenRPG.Entity.Components.Messages {
    public class AttackMessage : IComponentMessage {
        public readonly GameEntity ToAttack;

        public AttackMessage(GameEntity toAttack) {
            ToAttack = toAttack;
        }
    }
}
