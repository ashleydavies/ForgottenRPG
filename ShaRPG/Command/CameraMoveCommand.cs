#region

using ShaRPG.Camera;
using ShaRPG.Util;

#endregion

namespace ShaRPG.Command {
    public class CameraMoveCommand : ICommand {
        private readonly ICamera _camera;
        private readonly Vector2F _offset;

        public CameraMoveCommand(ICamera camera, Vector2F offset) {
            _camera = camera;
            _offset = offset;
        }

        public void Execute(float delta) {
            _camera.Center += _offset * delta;
        }
    }
}
