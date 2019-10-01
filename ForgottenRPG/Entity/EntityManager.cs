using System;
using System.Collections.Generic;
using System.Linq;
using ForgottenRPG.Entity.Components.Messages;
using ForgottenRPG.GameState;
using ForgottenRPG.Util;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;

namespace ForgottenRPG.Entity {
    public class EntityManager : IClickObserver, ICleanupEntities {
        public bool FightMode { get; private set; }

        public GameEntity Player => _playerId >= 0
                                        ? _entities[_playerId]
                                        : throw new EntityException("Unable to determine player - ID not set");

        public IEnumerable<GameEntity> Entities => _entities.Values.ToList();

        private readonly StateGame _gameState;
        private readonly Dictionary<int, GameEntity> _entities;
        private int _nextId;
        private int _playerId;
        private Queue<GameEntity> _combatQueue;
        private List<GameEntity> _deadList = new List<GameEntity>();

        public EntityManager(StateGame gameState) {
            _gameState = gameState;
            _nextId = 0;
            _playerId = -1;
            FightMode = false;
            _entities = new Dictionary<int, GameEntity>();
        }

        public int GetNextId(GameEntity e) {
            if (string.Equals(e.Name, Config.PlayerName, StringComparison.CurrentCultureIgnoreCase)) {
                if (_playerId != -1) throw new EntityException("Multiple players spawned?");

                _playerId = _nextId;
            }

            _entities[_nextId] = e;
            return _nextId++;
        }

        public GameEntity GetEntity(int id) {
            return _entities[id];
        }

        public void Update(float delta) {
            // The queue should never be empty as it should at least contain the player
            if (FightMode && _combatQueue.Peek().ActionBlocked()) {
                var gameEntity = _combatQueue.Dequeue();

                gameEntity.SendMessage(new TurnEndedMessage());
                _combatQueue.Enqueue(gameEntity);
                _combatQueue.Peek().SendMessage(new TurnStartedMessage());
            }

            foreach (GameEntity e in _entities.Values) {
                e.Update(delta);
            }

            foreach (GameEntity e in _deadList) {
                _entities.Remove(e.Id);
            }
            _deadList.Clear();
        }

        public void Render(RenderTarget renderSurface) {
            foreach (GameEntity e in _entities.Values) {
                e.Render(renderSurface);
            }
        }

        public bool IsMouseOver(ScreenCoordinate coordinates) {
            return _entities.Values.Any(
                e => e.MouseOver(_gameState.TranslateCoordinates(coordinates))
            );
        }

        public void Clicked(ScreenCoordinate coordinates) {
            GameCoordinate translateCoordinates = _gameState.TranslateCoordinates(coordinates);

            foreach (GameEntity e in _entities.Values) {
                if (e.MouseOver(translateCoordinates)) {
                    // TODO: Don't automatically attack allies on click
                    if (FightMode) {
                        Player.SendMessage(new AttackMessage(e));
                    } else {
                        e.SendMessage(new PlayerInteractMessage(translateCoordinates));
                    }

                    // Only one entity can be clicked - return once we found it
                    return;
                }
            }
        }

        // We can only toggle fight mode if the player is in control
        public void TryToggleFightMode() {
            if (FightMode && _combatQueue.Peek() != Player) return;

            FightMode = !FightMode;

            if (FightMode) {
                SetupCombatMode();
            }
        }

        private void SetupCombatMode() {
            // Set up the fight queue; the player always goes at the front
            _combatQueue = new Queue<GameEntity>();
            _combatQueue.Enqueue(_entities[_playerId]);

            foreach (var (id, entity) in _entities) {
                entity.SendMessage(new CombatStartMessage());

                if (id == _playerId) continue;

                _combatQueue.Enqueue(entity);
            }

            // Start the turn for the first entity in the queue
            _combatQueue.Peek().SendMessage(new TurnStartedMessage());
        }

        public void TrySkipTurn() {
            if (!FightMode || _combatQueue.Peek() != Player) return;
            Player.SendMessage(new SkipTurnMessage());
        }

        public void EntityDied(GameEntity entity) {
            _deadList.Add(entity);
            
            if (FightMode) {
                // Rebuild combat queue without the newly dead entity
                _combatQueue = new Queue<GameEntity>(_combatQueue.Where(e => e != entity));
            }
        }
    }

    public interface ICleanupEntities {
        void EntityDied(GameEntity entity);
    }
}
