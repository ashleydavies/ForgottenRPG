using System;
using SFML.Graphics;
using SFML.System;
using ShaRPG.Entity.Components.Messages;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;
using Color = SFML.Graphics.Color;

namespace ShaRPG.Entity.Components {
    public class HealthComponent : AbstractComponent, IMessageHandler<DamageMessage> {
        private readonly ICleanupEntities _caretaker;

        private float Health {
            get => _health;
            set {
                if (Dead) throw new EntityException(_entity, "Dead entity HP should not change");

                float oldVal = _health;
                _health = Math.Clamp(value, 0, _maxHealth);
                if (_health < oldVal) {
                    ServiceLocator.LogService.Log(LogType.Info, $"{_entity.Name} now has {_health} health");
                }

                if (Dead) {
                    ServiceLocator.LogService.Log(LogType.Info, $"{_entity.Name} has died");
                    SendMessage(new DiedMessage());
                    _caretaker.EntityDied(_entity);
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
            if (_entity.FightMode) return;

            if (Health < _maxHealth) Health += delta;
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

        public void Message(DamageMessage message) {
            Health -= message.Amount;
        }
    }
}
