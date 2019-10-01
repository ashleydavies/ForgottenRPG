using System;
using ForgottenRPG.Entity.Components.Messages;
using SFML.Graphics;

namespace ForgottenRPG.Entity.Components {
    public abstract class AbstractComponent : IComponent {
        protected readonly GameEntity Entity;

        protected AbstractComponent(GameEntity entity) {
            Entity = entity;
        }

        public virtual void Render(RenderTarget renderSurface) { }
        public abstract void Update(float delta);

        protected void Dependency<T>() where T : class, IComponent {
            if (Entity.GetComponent<T>() == null) {
                throw new EntityException(
                    Entity,
                    $"Component {GetType().Name} has an unresolved dependency on {typeof(T).Name}"
                );
            }
        }

        protected void SendMessage<T>(T message) where T : IComponentMessage {
            Entity.SendMessage(message);
        }
    }
}
