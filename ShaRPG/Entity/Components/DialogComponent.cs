namespace ShaRPG.Entity.Components {
    public class DialogComponent : AbstractComponent {
        private readonly EntityDialog.Dialog _dialog;
        
        public DialogComponent(GameEntity entity, EntityDialog.Dialog dialog) : base(entity) {
            _dialog = dialog;
        }

        public override void Update(float delta) {

        }
    }
}
