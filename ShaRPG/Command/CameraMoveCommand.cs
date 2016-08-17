#region

using ShaRPG.Camera;
using ShaRPG.Util;

#endregion

namespace ShaRPG.Command {
    public class CameraMoveCommand : ICommand {
        private readonly ICamera _camera;
        private readonly Vector2I _offset;

        public CameraMoveCommand(ICamera camera, Vector2I offset) {
            _camera = camera;
            _offset = offset;
        }

        public void Execute() {
            _camera.Center += _offset;
        }
    }
}
