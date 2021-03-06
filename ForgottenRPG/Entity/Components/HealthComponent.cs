using System;
using ForgottenRPG.Entity.Components.Messages;
using ForgottenRPG.Service;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;
using SFML.System;
using ForgottenRPG.Util;
using Color = SFML.Graphics.Color;

namespace ForgottenRPG.Entity.Components {
    public class HealthComponent : AbstractComponent, IMessageHandler<DamageMessage> {
        private readonly ICleanupEntities _caretaker;

        private float Health {
            get => _health;
            set {
                if (Dead) throw new EntityException(Entity, "Dead entity HP should not change");

                float oldVal = _health;
                _health = Math.Clamp(value, 0, _maxHealth);
                if (_health < oldVal) {
                    ServiceLocator.LogService.Log(LogType.Info, $"{Entity.Name} now has {_health} health");
                }

                if (Dead) {
                    ServiceLocator.LogService.Log(LogType.Info, $"{Entity.Name} has died");
                    SendMessage(new DiedMessage());
                    _caretaker.EntityDied(Entity);
                }
            }
        }

        public bool Dead => Health <= 0;
        private float _health;
        private readonly int _maxHealth;
        private readonly RectangleShape barBackground = new RectangleShape();
        private readonly RectangleShape barForeground = new RectangleShape();

        public HealthComponent(GameEntity entity, int maxHealth, ICleanupEntities caretaker) : base(entity) {
            _health = _maxHealth = maxHealth;
            _caretaker = caretaker;

            barBackground.FillColor = Color.Red;
            barBackground.Size = new Vector2f(64, 10);
            barForeground.FillColor = Color.Yellow;
            barForeground.Size = new Vector2f(64, 10);
        }

        public override void Update(float delta) {
            if (Entity.FightMode) return;

            if (Health < _maxHealth) Health += delta;
        }

        public override void Render(RenderTarget renderSurface) {
            // Only render health bars during fight mode
            if (!Entity.FightMode) return;

            barForeground.Size = new Vector2f(_health / _maxHealth * 64, 10);

            renderSurface.WithOffset(Entity.RenderPosition - new GameCoordinate(16, 24), () => {
                renderSurface.Draw(barBackground);
                renderSurface.Draw(barForeground);
            });
        }

        public void Message(DamageMessage message) {
            Health -= message.Amount;
        }
    }
}
