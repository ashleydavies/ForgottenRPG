using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using ShaRPG.Entity.Components;
using ShaRPG.GameState;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity {
    internal class EntityManager : IEntityIdAssigner, IClickObserver {
        public GameEntity Player => _playerId >= 0
                                        ? _entities[_playerId]
                                        : throw new EntityException("Unable to determine player - ID not set");

        private readonly StateGame _gameState;
        private readonly Dictionary<int, GameEntity> _entities;
        private int _nextId;
        private int _playerId;
        private bool _fightMode;
        private Queue<GameEntity> _fightQueue;

        public EntityManager(StateGame gameState) {
            _gameState = gameState;
            _nextId = 0;
            _playerId = -1;
            _fightMode = false;
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
            _fightMode = !_fightMode;

            if (_fightMode) {
                SetupFightMode();
            }
        }

        private void SetupFightMode() {
            // Set up the fight queue; the player always goes at the front
            _fightQueue = new Queue<GameEntity>();
            _fightQueue.Enqueue(_entities[_playerId]);
            
            foreach (var kvp in _entities) {
                int id = kvp.Key;
                GameEntity entity = kvp.Value;
                
                entity.SendMessage<CombatStartMessage>(new CombatStartMessage());
                
                if (id == _playerId) continue;
                
                _fightQueue.Enqueue(entity);
            }
        }
    }
}
