using ShaRPG;
using ShaRPG.Camera;
using ShaRPG.EntityDialog;
using ShaRPG.GameState;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

public class DialogState : AbstractGameState {
    private readonly Dialog _dialog;

    public DialogState(Game game, Dialog dialog, ICamera camera) : base(game, camera) {
        _dialog = dialog;
    }

    public override void Update(float delta) {
        
    }

    public override void Render(IRenderSurface renderSurface) {
        
    }

    public override void Clicked(ScreenCoordinate coordinates) {
        
    }

    public override void MouseWheelMoved(int delta) {
        
    }
}
