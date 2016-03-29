using System;

namespace ShaRPG.Entity.Components
{
    public class HealthComponent : IComponent
    {
        public int Health { get; set; }
        public int MaxHealth { get; set; }

        public event Action Death;

        public HealthComponent(int maxHealth)
        {
            Health = maxHealth;
            MaxHealth = maxHealth;
        }

        public void Update()
        {

        }

        public void Message(IComponentMessage componentMessage)
        {

        }

        public void TakeDamage(int damage)
        {
            Health -= damage;

            if (Health <= 0)
            {
                OnDeath();
            }
        }

        protected virtual void OnDeath()
        {
            Death?.Invoke();
        }
    }
}