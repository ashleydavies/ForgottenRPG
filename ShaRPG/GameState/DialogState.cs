using SFML.Graphics;
using SFML.System;
using ShaRPG.Entity.Dialog;
using ShaRPG.GUI;
using ShaRPG.Service;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GameState
{
    public class DialogState : AbstractGameState {
        private readonly Dialog _dialog;
        private readonly GuiWindow _dialogGuiWindow;
        private readonly VerticalFlowContainer _textContainer;
        private bool _reloadRequired = true;
        private static readonly Vector2i GuiWindowSize = new Vector2i(60 * 9, 60 * 12);

        public DialogState(Game game, Dialog dialog, Vector2f windowSize, ITextureStore textureStore)
            : base(game) {
            _dialog = dialog;
            _dialog.OnEnd += EndState;
            _dialogGuiWindow = new GuiWindow(textureStore, (Vector2i) (windowSize / 2), GuiWindowSize);

            ColumnContainer container = new ColumnContainer(ColumnContainer.Side.Left, 80);
            container.SetLeftComponent(
                new PaddingContainer(10, new SpriteContainer(textureStore.GetNewSprite(dialog.Graphic), Alignment.Center)));
            _textContainer = new VerticalFlowContainer();
            container.SetRightComponent(new PaddingContainer(5, _textContainer));

            _dialogGuiWindow.AddComponent(container);
        }

        public override void Update(float delta) {
            if (_reloadRequired) InitialiseText();
            _reloadRequired = false;
        }

        public override void Render(RenderTarget renderSurface) {
            _dialogGuiWindow.Render(renderSurface);
        }

        public override void Clicked(ScreenCoordinate coordinates) {
            if (_dialogGuiWindow.IsMouseOver(coordinates)) _dialogGuiWindow.Clicked(coordinates);
        }

        public override void MouseWheelMoved(float delta) { }

        private void InitialiseText() {
            _textContainer.Clear();
            _textContainer.AddComponent(new TextContainer(_dialog.Name, 24));
            _textContainer.AddComponent(new PaddingContainer(10, null));
            _textContainer.AddComponent(new TextContainer(_dialog.Prompt, 16) {
                Indent = 4,
                LineSpacing = 2
            });
            _textContainer.AddComponent(new PaddingContainer(5, null));

            {
                int replyIndex = 0;
                _dialog.Replies.ForEach(reply => {
                    _textContainer.AddComponent(new PaddingContainer(3, null));
                    TextContainer replyTextContainer = new TextContainer($"{replyIndex + 1}. {reply}", 16) {
                        Color = Color.Blue
                    };
                    int index = replyIndex;
                    replyTextContainer.OnClicked += coordinate => {
                        _dialog.ReplyActioned(index);
                        _reloadRequired = true;
                    };
                    _textContainer.AddComponent(replyTextContainer);
                    replyIndex++;
                });
            }
        }
    }
}
