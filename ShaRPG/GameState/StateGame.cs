﻿using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using ShaRPG.Entity;
using ShaRPG.Entity.Components;
using ShaRPG.EntityDialog;
using ShaRPG.Items;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GameState {
    public class StateGame : AbstractGameState, IOpenDialog {
        private GameEntity _player => _entityManager.Player;
        private readonly GameMap _map;
        private readonly MapLoader _mapLoader;
        private readonly EntityLoader _entityLoader;
        private readonly EntityManager _entityManager;
        private readonly ItemManager _itemManager;
        private readonly ClickManager _clickManager = new ClickManager();
        private readonly Dictionary<Keyboard.Key, (bool repeat, Action<float> action)> _keyMappings;
        private readonly Dictionary<Keyboard.Key, bool> _lastTime = new Dictionary<Keyboard.Key, bool>();
        private readonly FpsCounter _fpsCounter = new FpsCounter();
        private readonly ITextureStore _textureStore;
        private Vector2f _gameCenter;
        private Vector2f _renderOffset => _gameCenter - _windowSize / 2;
        private Vector2f _windowSize;
        private float _scale = 1;

        public StateGame(Game game, Vector2f size, ITextureStore textureStore, MapTileStore mapTileStore,
                         ItemManager itemManager) : base(game) {
            _windowSize = size;
            _textureStore = textureStore;
            _itemManager = itemManager;
            _entityManager = new EntityManager(this);
            _entityLoader = new EntityLoader(Config.EntityDataDirectory, _entityManager, textureStore);
            _mapLoader = new MapLoader(Config.MapDataDirectory, mapTileStore, _itemManager);
            _map = _mapLoader.LoadMap(0, this);
            _map.SpawnEntities(_entityLoader);
            _clickManager.Register(ClickPriority.Entity, _entityManager);
            _clickManager.Register(ClickPriority.Map, _map);

            _keyMappings = new Dictionary<Keyboard.Key, (bool, Action<float>)> {
                {
                    Keyboard.Key.Up,
                    (true, delta => _gameCenter = new Vector2f(_gameCenter.X, _gameCenter.Y - 300 * delta))
                }, {
                    Keyboard.Key.Down,
                    (true, delta => _gameCenter = new Vector2f(_gameCenter.X, _gameCenter.Y + 300 * delta))
                }, {
                    Keyboard.Key.Left,
                    (true, delta => _gameCenter = new Vector2f(_gameCenter.X - 300 * delta, _gameCenter.Y))
                }, {
                    Keyboard.Key.Right,
                    (true, delta => _gameCenter = new Vector2f(_gameCenter.X + 300 * delta, _gameCenter.Y))
                }, {
                    Keyboard.Key.F, (false, _ => _entityManager.TryToggleFightMode())
                }, {
                    Keyboard.Key.X, (false, _ => throw new EndGameException())
                }, {
                    Keyboard.Key.Tab, (false, _ => OpenInventory())
                }, {
                    Keyboard.Key.Space, (false, _ => _entityManager.TrySkipTurn())
                }
            };

            if (_player == null) throw new EntityException("No player was created during map loading time");

            _player.GetComponent<InventoryComponent>().Inventory
                   .PickupItem(new ItemStack(_itemManager.GetItem("iron_longsword"), 1));

            _gameCenter = (GameCoordinate) _player.Position;
        }

        public override void Update(float delta) {
            foreach (Keyboard.Key key in _keyMappings.Keys) {
                var pressed = Keyboard.IsKeyPressed(key);
                if (pressed && (_keyMappings[key].repeat || _lastTime.ContainsKey(key) && !_lastTime[key])) {
                    _keyMappings[key].action(delta);
                }

                _lastTime[key] = pressed;
            }

            _map.Update(delta);
            _entityManager.Update(delta);
            _fpsCounter.Update(delta);
        }

        public override void Render(RenderTarget renderSurface) {
            // TODO: Don't hard-code this
            if (renderSurface.Size.X > 2000) {
                //_scale = 1.5f;
            }

            renderSurface.WithView(new View(_gameCenter, _windowSize / _scale), () => {
                _map.Render(renderSurface);
                _entityManager.Render(renderSurface);
            });
            _fpsCounter.Render(renderSurface);
        }

        public override void Clicked(ScreenCoordinate coordinates) {
            _clickManager.Clicked(coordinates);
        }

        public override void MouseWheelMoved(int delta) {
            _scale += delta / 10f;
        }

        public void MovePlayer(TileCoordinate destination) {
            _player.SendMessage(new DestinationMessage(destination));
        }

        public void ExitGame() {
            throw new EndGameException();
        }

        public GameCoordinate TranslateCoordinates(ScreenCoordinate coordinates) {
            // Get the coordinates into the game scaling system
            coordinates -= new ScreenCoordinate(_windowSize / 2);
            coordinates /= _scale;
            coordinates += new ScreenCoordinate(_windowSize / 2);
            return coordinates.AsGameCoordinate(_renderOffset, _scale);
        }

        public void StartDialog(Dialog dialog) {
            ChangeState(new DialogState(Game, dialog, _windowSize, _textureStore));
        }

        private void OpenInventory() {
            ChangeState(new InventoryState(Game, _player.GetComponent<InventoryComponent>().Inventory, _map,
                                           _player.Position, _windowSize, _textureStore));
        }
    }
}
