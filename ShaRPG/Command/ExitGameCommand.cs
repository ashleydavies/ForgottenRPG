namespace ShaRPG.Command {
    public class ExitGameCommand : ICommand {
        private readonly GameState.GameState game_;

        public ExitGameCommand(GameState.GameState game) {
            game_ = game;
        }

        public void Execute(float delta) {
            game_.ExitGame();
        }
    }
}
