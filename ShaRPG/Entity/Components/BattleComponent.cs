namespace ShaRPG.Entity.Components {
    public class BattleComponent : AbstractComponent {
        private readonly int _maxBp;
        
        public BattleComponent(GameEntity entity, int maxBp = 7) : base(entity) {
            _maxBp = maxBp;
            Dependency<HealthComponent>();
        }
        
        public override void Update(float delta) {
            throw new System.NotImplementedException();
        }
    }
}
