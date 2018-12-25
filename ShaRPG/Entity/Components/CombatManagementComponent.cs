using System;

namespace ShaRPG.Entity.Components {
    /// <summary>
    /// This component handles all logic related to maintaining this entity during combat mode
    /// Mainly, it maintains the current and max AP of the entity and allows other components to query the current AP
    ///   and make changes to it (e.g. the move component decreases it each step).
    /// </summary>
    public class CombatManagementComponent : AbstractComponent,
                                             IMessageHandler<TurnStartedMessage>, IMessageHandler<MovedMessage> {
        public int MaxAp { get; }
        public int Ap { get; private set; }

        public CombatManagementComponent(GameEntity entity, int ap) : base(entity) {
            MaxAp = ap;
            Ap = 0;
        }

        public void Message(MovedMessage message) {
            Ap--;
        }
        
        // At the start of the turn, we get maximum AP
        public void Message(TurnStartedMessage message) {
            Ap = MaxAp;
        }
        
        public override void Update(float delta) { }
    }
}
