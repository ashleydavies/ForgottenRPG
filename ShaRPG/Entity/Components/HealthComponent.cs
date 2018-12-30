using System;
using System.Drawing;
using SFML.Graphics;
using SFML.System;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;
using Color = SFML.Graphics.Color;

namespace ShaRPG.Entity.Components {
    public class HealthComponent : AbstractComponent {
        private float Health {
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
        private readonly RectangleShape barBackground = new RectangleShape();
        private readonly RectangleShape barForeground = new RectangleShape();

        public HealthComponent(GameEntity entity, int maxHealth) : base(entity) {
            _health = _maxHealth = maxHealth;
            
            barBackground.FillColor = Color.Red;
            barBackground.Size = new Vector2f(64, 10);
            barForeground.FillColor = Color.Yellow;
            barForeground.Size = new Vector2f(64, 10);
        }

        public override void Update(float delta) {
            Health += delta;
        }
        
        public override void Render(RenderTarget renderSurface) {
            // Only render health bars during fight mode
            if (!_entity.FightMode) return;

            barForeground.Size = new Vector2f(_health / _maxHealth * 64, 10);
            
            renderSurface.WithOffset(_entity.RenderPosition - new GameCoordinate(16, 24), () => {
                renderSurface.Draw(barBackground);
                renderSurface.Draw(barForeground);
            });
        }
    }
}
