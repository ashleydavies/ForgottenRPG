using ForgottenRPG.Entity.Components.Messages;
using ForgottenRPG.Service;

namespace ForgottenRPG.Entity.Components {
    public class AttackComponent : AbstractComponent, IMessageHandler<TurnStartedMessage>,
                                   IMessageHandler<AttackMessage> {
        private GameEntity _target;


        public AttackComponent(GameEntity entity) : base(entity) {
            Dependency<CombatComponent>();
        }

        public override void Update(float delta) {
            // TODO: Should components just not update if ActionBlocked..?
            if (_target == null || Entity.ActionBlocked()) return;

            if (Entity.IsAdjacent(_target)) {
                ServiceLocator.LogService.Log(LogType.Info, $"{Entity.Name} attacked {_target.Name}");
                _target.SendMessage(new DamageMessage(100, Entity));
                SendMessage(new SkipTurnMessage());
            } else {
                SendMessage(new DestinationMessage(_target.Position));
            }
        }

        public void Message(TurnStartedMessage message) {
            _target = null;
        }

        public void Message(AttackMessage message) {
            _target = message.ToAttack;
        }
    }
}
