namespace ShaRPG.Entity.Components {
    public class DialogComponent : AbstractComponent {
        public DialogComponent(GameEntity entity) : base(entity) { }

        public override void Update(float delta) {
            throw new System.NotImplementedException();
        }

        public override void Message(IComponentMessage componentMessage) {
            throw new System.NotImplementedException();
        }
    }
}
