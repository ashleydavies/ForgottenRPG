using System;

namespace ShaRPG.Entity.Components {
    /// <summary>
    /// This component handles all logic related to maintaining this entity during combat mode
    /// Mainly, it maintains the current and max AP of the entity and allows other components to query the current AP
    ///   and make changes to it (e.g. the move component decreases it each step).
    /// </summary>
    public class CombatManagementComponent : AbstractComponent, IMessageHandler<CombatStartMessage>,
                                             IMessageHandler<TurnEndedMessage>, IMessageHandler<MoveMessage> {
        public int MaxAp { get; }
        public int Ap { get; private set; }

        public CombatManagementComponent(GameEntity entity, int ap) : base(entity) {
            MaxAp = Ap = ap;
        }

        public void Message(CombatStartMessage message) {
            ResetAp();
        }

        public void Message(TurnEndedMessage message) {
            ResetAp();
        }

        public void Message(MoveMessage message) {
            Ap--;
        }

        private void ResetAp() {
            Ap = MaxAp;
        }

        public override void Update(float delta) { }
    }
}
