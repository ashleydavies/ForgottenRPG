using ShaRPG;
using ShaRPG.Camera;
using ShaRPG.EntityDialog;
using ShaRPG.GameState;
using ShaRPG.GUI;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

public class DialogState : AbstractGameState {
    private readonly Dialog _dialog;
    private readonly GuiWindow _dialogGuiWindow;

    public DialogState(Game game, Dialog dialog, Vector2I windowSize, ICamera camera,
                       ISpriteStoreService spriteStore) : base(game, camera) {
        _dialog = dialog;
        _dialogGuiWindow = new GuiWindow(spriteStore, windowSize / 2, new Vector2I(60 * 9 + 24, 60 * 12 + 24));

        ColumnContainer container = new ColumnContainer(ColumnContainer.Side.Left, 80);
        container.SetLeftComponent(
            new PaddingContainer(10, new SpriteContainer(spriteStore.GetSprite(dialog.Character), Alignment.Center)));
        VerticalFlowContainer textContainer = new VerticalFlowContainer();
        container.SetRightComponent(new PaddingContainer(5, textContainer));

        textContainer.AddComponent(new TextContainer(dialog.Name, 24));
        textContainer.AddComponent(new PaddingContainer(10, null));
        textContainer.AddComponent(new TextContainer(dialog.Prompt, 16) {
            Indent = 4,
            LineSpacing = 2
        });
        textContainer.AddComponent(new PaddingContainer(5, null));

        {
            int replyIndex = 1;
            dialog.Replies.ForEach(reply => {
                textContainer.AddComponent(new PaddingContainer(3, null));
                textContainer.AddComponent(new TextContainer($"{replyIndex++}. {reply}", 16));
            });
        }

        _dialogGuiWindow.AddComponent(container);
    }

    public override void Update(float delta) { }

    public override void Render(IRenderSurface renderSurface) {
        _dialogGuiWindow.Render(renderSurface);
    }

    public override void Clicked(ScreenCoordinate coordinates) { }

    public override void MouseWheelMoved(int delta) { }
}
