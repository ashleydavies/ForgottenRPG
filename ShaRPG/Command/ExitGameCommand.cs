namespace ShaRPG.Command {
    public class ExitGameCommand : ICommand {
        private readonly GameState.StateGame _game;

        public ExitGameCommand(GameState.StateGame game) {
            _game = game;
        }

        public void Execute(float delta) {
            _game.ExitGame();
        }
    }
}
