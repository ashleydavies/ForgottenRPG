using ForgottenRPG.Entity.Components.Messages;

namespace ForgottenRPG.Entity.Components {
    public class DialogComponent : AbstractComponent, IMessageHandler<PlayerInteractMessage> {
        private readonly Dialog.Dialog _dialog;
        
        public DialogComponent(GameEntity entity, Dialog.Dialog dialog) : base(entity) {
            _dialog = dialog;
        }

        public override void Update(float delta) {
        }

        public void Message(PlayerInteractMessage message) {
            _dialog.StartDialog();
        }
    }
}
