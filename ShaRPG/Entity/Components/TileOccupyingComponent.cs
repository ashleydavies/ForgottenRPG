using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity.Components {
    class TileOccupyingComponent : AbstractComponent {
        public TileOccupyingComponent(Entity entity) : base(entity) {}

        private TileCoordinate _coordinate;
        private TileCoordinate _aim;

        public void MoveTo(TileCoordinate coordinate) {
            _aim = coordinate;
        }

        public override void Update() {
            _entity.GetComponent<PositionComponent>()?.SetPosition(_coordinate);
        }

        public override void Message(IComponentMessage componentMessage) {

        }
    }
}
