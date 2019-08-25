using ShaRPG.Entity.Components.Messages;

namespace ShaRPG.Entity.Components {
    /// <summary>
    /// This component handles all logic related to maintaining this entity during combat mode.
    /// Mainly, it maintains the current and max AP of the entity and allows other components to query the current AP
    /// and make changes to it (e.g. the move component decreases it each step).
    /// </summary>
    public class CombatComponent : AbstractComponent,
                                   IMessageHandler<TurnStartedMessage>, IMessageHandler<MovedMessage>,
                                   IMessageHandler<SkipTurnMessage>, IMessageHandler<DiedMessage> {
        public int MaxAp { get; }
        public int Ap { get; private set; }

        private readonly FactionManager _factionManager;

        public CombatComponent(GameEntity entity, int ap, FactionManager factionManager,
                               string faction) : base(entity) {
            MaxAp = ap;
            Ap = 0;
            _factionManager = factionManager;
            _factionManager.RegisterEntity(entity, faction);
        }

        public override void Update(float delta) { }

        public void Message(MovedMessage message) {
            Ap--;
        }

        // At the start of the turn, we get maximum AP
        public void Message(TurnStartedMessage message) {
            Ap = MaxAp;
        }

        // If we want to skip a turn, just set AP to zero and it will get naturally handled in the next update
        public void Message(SkipTurnMessage message) {
            Ap = 0;
        }

        public void Message(DiedMessage message) {
            _factionManager.DeregisterEntity(_entity);
        }
    }
}
