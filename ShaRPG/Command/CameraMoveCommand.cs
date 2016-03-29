using ShaRPG.Camera;

namespace ShaRPG.Command
{
    public class CameraMoveCommand : ICommand
    {
        private readonly ICamera _camera;
        private readonly int _offsetX;
        private readonly int _offsetY;

        public CameraMoveCommand(ICamera camera, int offsetX, int offsetY)
        {
            this._camera = camera;
            this._offsetX = offsetX;
            this._offsetY = offsetY;
        }

        public void Execute()
        {
            _camera.Move(_offsetX, _offsetY);
        }
    }
}