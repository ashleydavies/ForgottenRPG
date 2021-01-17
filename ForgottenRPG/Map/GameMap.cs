using System;
using System.Collections.Generic;
using System.Linq;
using ForgottenRPG.Entity;
using ForgottenRPG.GameState;
using ForgottenRPG.Items;
using ForgottenRPG.Map.Pathfinding;
using ForgottenRPG.Service;
using ForgottenRPG.Util;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;

namespace ForgottenRPG.Map {
    public class GameMap : IClickObserver, IPathCreator, IPositionalItemStorage {
        private readonly StateGame _game;
        private readonly int[,] _tiles;
        private readonly MapNode[,] _pathfindingNodes;
        private readonly List<GameMapEntitySpawnDetails> _spawnPositions;
        private readonly List<(ItemStack itemStack, GameCoordinate position)> _items;
        private readonly MapTileStore _tileStore;
        private readonly Random _random = new Random();
        public readonly Vector2I Size;

        public MapTile GetTile(TileCoordinate coordinate) => _tileStore.GetTile(TileId(coordinate));
        public int TileId(TileCoordinate coordinate) => _tiles[coordinate.X, coordinate.Y];
        public bool Collideable(TileCoordinate position) => GetTile(position).Collideable;

        public bool TileAtPosition(TileCoordinate position) =>
            position.X >= 0 && position.Y >= 0 && position.X < _tiles.GetLength(0) && position.Y < _tiles.GetLength(1);

        public List<TileCoordinate> GetPath(TileCoordinate start, TileCoordinate finish)
            => AStar.GetPath(_pathfindingNodes, start, finish);

        public bool IsMouseOver(ScreenCoordinate screenCoordinates)
            => TileAtPosition(_game.TranslateCoordinates(screenCoordinates));

        public GameMap(StateGame game, int[,] tiles, Vector2I size, MapTileStore tileStore,
                       List<GameMapEntitySpawnDetails> spawnPositions, List<(ItemStack, GameCoordinate)> items) {
            _game = game;
            _tiles = tiles;
            _pathfindingNodes = new MapNode[size.X, size.Y];
            _tileStore = tileStore;
            _spawnPositions = spawnPositions;
            _items = items;
            Size = size;

            InitialisePathfindingNodes();
            //InitialiseCompositeTiles();
        }

        public void SpawnEntities(EntityLoader entityLoader) {
            _spawnPositions.ForEach(spawnDetails => {
                entityLoader.LoadEntity(spawnDetails.EntityName, this, spawnDetails.Position, spawnDetails.Path, _game);
            });
        }

        public void Render(RenderTarget renderSurface) {
            EachTile((x, y) => GetTile(new TileCoordinate(x, y)).Draw(renderSurface, new TileCoordinate(x, y)));
            _items.ForEach(itemInstance => {
                renderSurface.Draw(new Sprite(itemInstance.itemStack.Item.Texture) {
                    Position = itemInstance.position
                });
            });
        }

        public void Update(float delta) {
            _tileStore.Update(delta);
        }

        public void Clicked(ScreenCoordinate coordinates) {
            ServiceLocator.LogService.Log(
                LogType.Info,
                $"Clicked map at {(TileCoordinate) _game.TranslateCoordinates(coordinates)}"
            );
            _game.MovePlayer(_game.TranslateCoordinates(coordinates));
        }

        private void InitialisePathfindingNodes() {
            EachTile((x, y) => {
                _pathfindingNodes[x, y] = new MapNode {
                    Map = this,
                    Position = new TileCoordinate(x, y)
                };
            });

            EachTile((x, y) => {
                List<MapNode> neighbors = new List<MapNode>();

                for (int x1 = Math.Max(x - 1, 0); x1 <= Math.Min(Size.X - 1, x + 1); x1++) {
                    for (int y1 = Math.Max(y - 1, 0); y1 <= Math.Min(Size.Y - 1, y + 1); y1++) {
                        if (Math.Abs(x - x1) == 1 && Math.Abs(y - y1) == 1) continue;

                        neighbors.Add(_pathfindingNodes[x1, y1]);
                    }
                }

                _pathfindingNodes[x, y].Neighbors = neighbors.ToArray();
            });
        }

        private void InitialiseCompositeTiles() {
            throw new NotImplementedException();
        }

        private void EachTile(Action<int, int> f) {
            for (int x = 0; x < Size.X; x++) for (int y = 0; y < Size.Y; y++) f(x, y);
        }

        public List<ItemStack> GetItems(GameCoordinate position, int distance) {
            return (from t in _items
                    where (t.position + new GameCoordinate(ItemManager.SpriteSizeX, ItemManager.SpriteSizeY) / 2)
                          .EuclideanDistance(position) < distance
                    select t.itemStack).ToList();
        }

        public void DropItem(GameCoordinate position, ItemStack item) {
            // If there are nearby items, try to put this item somewhere unique
            TryDropItem(position, item);
        }

        // TODO: Identify a number of candidate positions and choose the best (closest) so we can increase density
        // Coming back just over a year later, this code is delightfully hacky given how well it actually works
        private void TryDropItem(GameCoordinate position, ItemStack item, int retries = 25) {
            if (retries == 0 || !_items.Any(i => i.position.EuclideanDistance(position) < 16)) {
                _items.Add((item, position));
                return;
            }

            TryDropItem(position + new GameCoordinate(_random.Next(-10, 10), _random.Next(-10, 10)),
                        item, retries - 1);
        }

        public void CollectItem(ItemStack itemStack) {
            _items.RemoveAll(t => ReferenceEquals(t.itemStack, itemStack));
        }
    }

    public readonly struct GameMapEntitySpawnDetails {
        public readonly TileCoordinate Position;
        public readonly string EntityName;
        public readonly List<TileCoordinate> Path;

        public GameMapEntitySpawnDetails(TileCoordinate position, string entityName, List<TileCoordinate> path) {
            Position = position;
            EntityName = entityName;
            Path = path;
        }
    }
}
