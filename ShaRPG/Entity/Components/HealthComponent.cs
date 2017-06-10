using System;
using ShaRPG.Service;

namespace ShaRPG.Entity.Components {
    public class HealthComponent : AbstractComponent {
        public float Health {
            get => _health;
            set {
                if (Dead) throw new EntityException(_entity, "Dead entity HP should not change");

                _health = Math.Min(value, _maxHealth);

                if (Dead) Death?.Invoke();
            }
        }
        public event Action Death;
        public bool Dead => Health <= 0;
        private float _health;
        private readonly int _maxHealth;

        public HealthComponent(GameEntity entity, int maxHealth) : base(entity) {
            _health = _maxHealth = maxHealth;
        }

        public override void Update(float delta) {
            Health += delta * 1;
        }

        public override void Message(IComponentMessage componentMessage) {
            throw new NotImplementedException();
        }
    }
}
