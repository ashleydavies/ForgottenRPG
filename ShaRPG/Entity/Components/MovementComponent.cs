using ShaRPG.Map;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity.Components {
    public class MovementComponent : AbstractComponent, IMessageHandler<MoveMessage>, IMessageHandler<CombatStartMessage> {
        public GameCoordinate RenderOffset => (GameCoordinate) (_previousPosition - _entity.Position)
                                              * PositionLerpFraction;

        private const float TotalPositionLerpTime = 0.3f;
        private const float PositionLerpTimeMultiplier = 1 / TotalPositionLerpTime;
        private float _positionLerpTime;
        private float PositionLerpFraction => _positionLerpTime * PositionLerpTimeMultiplier;
        private TileCoordinate _targetPosition;
        private TileCoordinate _previousPosition;
        private readonly IPathCreator _pathCreator;


        public MovementComponent(GameEntity entity, IPathCreator pathCreator) : base(entity) {
            // An entity cannot move if it does not know how to handle its movement in combat mode (importantly, this
            //   does not imply that it can fight.)
            Dependency<CombatManagementComponent>();
            _pathCreator = pathCreator;
            _targetPosition = _previousPosition = entity.Position;
        }

        public override void Update(float delta) {
            if (!_targetPosition.Equals(_entity.Position) || _positionLerpTime > 0) {
                if (_positionLerpTime <= 0) {
                    _previousPosition = _entity.Position;
                    _entity.Position = _pathCreator.GetPath(_entity.Position, _targetPosition)?[0] ?? _entity.Position;
                    _positionLerpTime = TotalPositionLerpTime;
                } else {
                    _positionLerpTime -= delta;
                }
            }
            
            if (_positionLerpTime < 0) _positionLerpTime = 0.0f;
        }

        public void Message(MoveMessage message) {
            _targetPosition = message.DesiredPosition;
        }

        public void Message(CombatStartMessage message) {
            // End movement lerping
            if (!_targetPosition.Equals(_entity.Position)) {
                _entity.Position = _pathCreator.GetPath(_entity.Position, _targetPosition)?[0] ?? _entity.Position;
                _targetPosition = _entity.Position;
                _positionLerpTime = 0.0f;
            }
        }
    }
}
