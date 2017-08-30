using ShaRPG.GameState;

namespace ShaRPG.Command {
    public class OpenInventoryCommand : ICommand {
        private readonly StateGame _gameState;

        public OpenInventoryCommand(StateGame gameState) {
            _gameState = gameState;
        }
        
        public void Execute(float delta) {
            _gameState.OpenInventory();
        }
    }
}
