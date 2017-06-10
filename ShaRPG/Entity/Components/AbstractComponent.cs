using ShaRPG.Util;

namespace ShaRPG.Entity.Components {
    public abstract class AbstractComponent : IComponent {
        protected GameEntity _entity;

        protected AbstractComponent(GameEntity entity) {
            _entity = entity;
        }

        public void Render(IRenderSurface renderSurface) {
            return;
        }

        public abstract void Update(float delta);
        public abstract void Message(IComponentMessage componentMessage);
    }
}
