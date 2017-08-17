using ShaRPG.EntityDialog;
using ShaRPG.Service;

namespace ShaRPG.Entity.Components {
    public class DialogComponent : AbstractComponent, IMessageHandler<MouseClickMessage> {
        private readonly Dialog _dialog;
        
        public DialogComponent(GameEntity entity, Dialog dialog) : base(entity) {
            _dialog = dialog;
        }

        public override void Update(float delta) {
        }

        public void Message(MouseClickMessage message) {
            _dialog.StartDialog();
        }
    }
}
