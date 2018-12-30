using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using ShaRPG.Entity.Components;
using ShaRPG.GameState;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity {
    public class EntityManager : IClickObserver {
        public bool FightMode { get; private set; }
        public GameEntity Player => _playerId >= 0
                                        ? _entities[_playerId]
                                        : throw new EntityException("Unable to determine player - ID not set");

        private readonly StateGame _gameState;
        private readonly Dictionary<int, GameEntity> _entities;
        private int _nextId;
        private int _playerId;
        private Queue<GameEntity> _combatQueue;

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
        }

        public void Render(RenderTarget renderSurface) {
            foreach (GameEntity e in _entities.Values) {
                e.Render(renderSurface);
            }
        }

        public bool IsMouseOver(ScreenCoordinate coordinates) {
            return _entities.Values.Aggregate(
                false,
                (over, e) => over | e.MouseOver(_gameState.TranslateCoordinates(coordinates))
            );
        }

        public void Clicked(ScreenCoordinate coordinates) {
            GameCoordinate translateCoordinates = _gameState.TranslateCoordinates(coordinates);
            
            foreach (GameEntity e in _entities.Values) {
                if (e.MouseOver(translateCoordinates)) {
                    e.SendMessage(new MouseClickMessage(translateCoordinates));
                    
                    // Only one entity can be clicked - return once we found it
                    return;
                }
            }
        }

        public void ToggleFightMode() {
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
    }
}
