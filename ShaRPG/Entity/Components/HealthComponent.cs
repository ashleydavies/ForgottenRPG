#region

using System;

#endregion

namespace ShaRPG.Entity.Components {
    public class HealthComponent : AbstractComponent {
        public HealthComponent(Entity entity, int maxHealth) : base(entity) {
            Health = maxHealth;
            MaxHealth = maxHealth;
        }

        private int Health { get; set; }
        private int MaxHealth { get; set; }
        public bool Dead => Health <= 0;
        public event Action Death;

        public void TakeDamage(int damage) {
            if (Dead) {
                throw new ComponentException("TakeDamage failed: Entity already dead");
            }

            Health -= damage;

            if (Dead) {
                Death?.Invoke();
            }
        }

        public override void Update() {}

        public override void Message(IComponentMessage componentMessage) {}
    }
}
