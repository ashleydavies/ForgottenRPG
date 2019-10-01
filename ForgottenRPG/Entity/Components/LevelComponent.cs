namespace ForgottenRPG.Entity.Components {
    public class LevelComponent : AbstractComponent {
        public int Level => 1;
        
        public LevelComponent(GameEntity entity) : base(entity) { }
        public override void Update(float delta) { }
    }
}
