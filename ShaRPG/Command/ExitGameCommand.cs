namespace ShaRPG.Command {
    public class ExitGameCommand : ICommand {
        private readonly GameState.GameState _game;

        public ExitGameCommand(GameState.GameState game) {
            _game = game;
        }

        public void Execute(float delta) {
            _game.ExitGame();
        }
    }
}
