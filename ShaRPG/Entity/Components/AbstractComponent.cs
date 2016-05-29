using ShaRPG.Util;

namespace ShaRPG.Entity.Components {
    public abstract class AbstractComponent : IComponent {
        protected Entity _entity;

        public AbstractComponent(Entity entity) {
            _entity = entity;
        }

        public void Render(IRenderSurface renderSurface) {
            return;
        }

        public abstract void Update();
        public abstract void Message(IComponentMessage componentMessage);
    }
}
