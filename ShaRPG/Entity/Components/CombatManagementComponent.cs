namespace ShaRPG.Entity.Components {
    /// <summary>
    /// This component handles all logic related to maintaining this entity during combat mode
    /// Mainly, it maintains the current and max AP of the entity and allows other components to query the current AP
    ///   and make changes to it (e.g. the move component decreases it each step).
    /// </summary>
    public class CombatManagementComponent : AbstractComponent, IMessageHandler<CombatStartMessage> {
        private int _maxAp;
        private int _ap;
        
        public CombatManagementComponent(GameEntity entity, int ap, int maxAp) : base(entity) {
            _maxAp = _ap = maxAp;
        }
        
        public void Message(CombatStartMessage message) {
            _ap = _maxAp;
        }

        public override void Update(float delta) { }
    }
}
