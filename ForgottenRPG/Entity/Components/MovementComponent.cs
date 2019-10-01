using System;
using ForgottenRPG.Entity.Components.Messages;
using ForgottenRPG.Map;
using ForgottenRPG.Util.Coordinate;

namespace ForgottenRPG.Entity.Components {
    public class MovementComponent : AbstractComponent, IMessageHandler<DestinationMessage>,
                                     IMessageHandler<CombatStartMessage> {
        public GameCoordinate RenderOffset => (GameCoordinate) (_previousPosition - _entity.Position)
                                              * PositionLerpFraction;

        private const float TotalPositionLerpTime = 0.3f;
        private const float PositionLerpTimeMultiplier = 1 / TotalPositionLerpTime;
        private float _positionLerpTime;
        private float PositionLerpFraction => _positionLerpTime * PositionLerpTimeMultiplier;
        private MoveState _state = MoveState.Still;
        private TileCoordinate _targetPosition;
        private TileCoordinate _previousPosition;
        private readonly IPathCreator _pathCreator;

        private bool Animating() => _positionLerpTime > 0;
        private bool ReachedDestination() => _targetPosition.Equals(_entity.Position);

        public MovementComponent(GameEntity entity, IPathCreator pathCreator) : base(entity) {
            // An entity cannot move if it does not know how to handle its movement in combat mode (importantly, this
            //   does not imply that it can fight.)
            Dependency<CombatComponent>();
            _pathCreator = pathCreator;
            _targetPosition = _previousPosition = entity.Position;
        }

        public override void Update(float delta) {
            if (_state == MoveState.Animating) {
                _positionLerpTime = Math.Max(0.0f, _positionLerpTime - delta);

                if (_positionLerpTime <= 0) {
                    SendMessage(new MovedMessage(_previousPosition, _entity.Position));
                    TryNextMove();
                }
            } else if (!ReachedDestination()) {
                TryNextMove();
            }
        }

        private void TryNextMove() {
            _positionLerpTime = 0.0f;
            if (!_entity.ActionBlocked() && !ReachedDestination()) {
                _previousPosition = _entity.Position;
                _entity.Position = _pathCreator.GetPath(_entity.Position, _targetPosition)?[0] ?? _entity.Position;
                _positionLerpTime = TotalPositionLerpTime;
                _state = MoveState.Animating;
            } else {
                _targetPosition = _entity.Position;
                _state = MoveState.Still;
            }
        }

        public void Message(DestinationMessage message) {
            _targetPosition = message.DesiredPosition;
        }

        public void Message(CombatStartMessage message) {
            // End movement lerping
            if (!_targetPosition.Equals(_entity.Position)) {
                //_entity.Position = _pathCreator.GetPath(_entity.Position, _targetPosition)?[0] ?? _entity.Position;
                _targetPosition = _entity.Position;
                _positionLerpTime = 0.0f;
            }
        }

        private enum MoveState {
            Still,
            Animating
        }
    }
}
