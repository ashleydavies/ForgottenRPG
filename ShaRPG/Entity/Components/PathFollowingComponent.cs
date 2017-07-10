using System.Collections.Generic;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity.Components {
    public class PathFollowingComponent : AbstractComponent {
        private readonly List<TileCoordinate> _path;
        private int _pathIndex;

        public PathFollowingComponent(GameEntity entity, List<TileCoordinate> path) : base(entity) {
            Dependency<MovementComponent>();
            _path = path;
            _pathIndex = 0;
            
            if (_path.Count == 0) throw new EntityException(entity, "Attempt to initialise empty path component");
        }

        public override void Update(float delta) {
            if (_path.Count > 0 && _path[_pathIndex].Equals(_entity.Position)) {
                IssueMovement(_path[_pathIndex++]);
                _pathIndex %= _path.Count;
            }

            if (!_path[_pathIndex].Equals(_entity.Position)) {
                IssueMovement(_path[_pathIndex]);
            }
        }

        private TileCoordinate IssueMovement(TileCoordinate targetPosition) {
            return _entity.GetComponent<MovementComponent>().TargetPosition = targetPosition;
        }

        public override void Message(IComponentMessage componentMessage) {
            throw new System.NotImplementedException();
        }
    }
}
