using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ForgottenRPG.GameState.Xml;
using ForgottenRPG.GUI;
using ForgottenRPG.Service;
using ForgottenRPG.Util;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;
using SFML.System;

namespace ForgottenRPG.GameState {
    public class WorldMapState : AbstractGameState {
        private readonly IGuiComponentContainer _gui = new FixedContainer();
        private readonly Dictionary<string, MapDetails> _maps = new();
        private readonly List<RectangleShape> _connectionRects = [];
        private const int IconWidth = 32;
        private const int IconHeight = 32;

        private record MapDetails(string Id, IGuiComponent Icon, Vector2I Position) {
            public readonly Dictionary<Direction, MapDetails> Connections = new();

            public void AddConnection(Direction dir, MapDetails details) {
                if (Connections.TryGetValue(dir, out var value)) {
                    throw new ArgumentException(
                        $"Cannot add connection to {details.Id} for map {Id}; it is already connected to {value.Id} in the same direction");
                }

                Connections.Add(dir, details);
            }
        }

        // TODO: Ingest a loaded map list instead of loading it inside ctor
        public WorldMapState(GameUtils utils, Game game, string xmlDirectory) : base(game) {
            _gui.AddComponent(new SpriteContainer(utils.textureStore.GetNewSprite("worldmap")));

            using var fs = File.OpenRead(Path.Combine(xmlDirectory, "Map", "mapList.xml"));

            var serializer = new XmlSerializer(typeof(MapOverview));
            if (serializer.Deserialize(fs) is not MapOverview mapOverview) {
                throw new Exception("Unable to deserialize MapOverview");
            }

            foreach (var map in mapOverview.Maps.MapList) {
                if (_maps.ContainsKey(map.Id)) {
                    throw new Exception($"Map with id {map.Id} exists twice in the map file");
                }

                var icon   = utils.textureStore.GetNewSprite($"map_icon_{map.Icon}");
                var sprite = new FixedContainer(map.Mx, map.My, new SpriteContainer(icon));
                _gui.AddComponent(sprite);
                sprite.OnClicked += _ => {
                    var windowSize = new Vector2f(utils.window.Size.X, utils.window.Size.Y);
                    SetState(new StateGame(Game, utils, map.Id, windowSize));
                    ServiceLocator.LogService.Log(LogType.Info, $"Clicked on {map.Id}");
                };
                _maps[map.Id] = new MapDetails(map.Id, sprite, new Vector2I(map.Mx, map.My));
            }

            foreach (var connectionElement in mapOverview.Connections.ConnectionList) {
                if (!_maps.TryGetValue(connectionElement.From, out var from)) {
                    throw new InvalidDataException($"Connection 'from' id '{connectionElement.From}' not found.");
                }

                if (!_maps.TryGetValue(connectionElement.To, out var to)) {
                    throw new InvalidDataException($"Connection 'to' id '{connectionElement.To}' not found.");
                }

                var direction = DirectionUtils.FromString(
                    connectionElement.Start ?? throw new InvalidDataException("Connection missing 'start' direction.")
                );

                var endDirection = connectionElement.End is { Length: > 0 }
                    ? DirectionUtils.FromString(connectionElement.End)
                    : direction.Opposite();

                from.AddConnection(direction, to);
                to.AddConnection(endDirection, from);

                var start = PointPosition(from.Position, direction);
                var end   = PointPosition(to.Position, endDirection);

                var shape = new RectangleShape {
                    Size = new Vector2f(0, (float)start.Distance(end)),
                    Position = new Vector2f(start.X, start.Y),
                    Rotation = 270 + float.RadiansToDegrees((float)Math.Atan2(end.Y - start.Y, end.X - start.X)),
                    OutlineColor = Color.Black,
                    OutlineThickness = 1,
                };

                _connectionRects.Add(shape);
            }
        }


        public override void Update(float delta) { }

        public override void Render(RenderTarget renderSurface) {
            _gui.Render(renderSurface);
            foreach (var rect in _connectionRects) {
                renderSurface.Draw(rect);
            }
        }

        private Vector2I PointPosition(Vector2I iconPosition, Direction direction) {
            return iconPosition + direction switch {
                Direction.North => new Vector2I(IconWidth / 2, 0),
                Direction.East  => new Vector2I(IconWidth, IconHeight / 2),
                Direction.South => new Vector2I(IconWidth / 2, IconHeight),
                Direction.West  => new Vector2I(0, IconHeight / 2),
                _               => throw new ArgumentOutOfRangeException()
            };
        }

        public override void Clicked(ScreenCoordinate coordinates) {
            _gui.Clicked(coordinates);
        }

        public override void MouseWheelMoved(float delta) { }
    }

    namespace Xml {
        [XmlRoot(ElementName = "Map")]
        public class Map {
            [XmlAttribute(AttributeName = "id")] public string Id { get; set; }
            [XmlAttribute(AttributeName = "name")] public string Name { get; set; }
            [XmlAttribute(AttributeName = "icon")] public string Icon { get; set; }
            [XmlAttribute(AttributeName = "mX")] public int Mx { get; set; }
            [XmlAttribute(AttributeName = "mY")] public int My { get; set; }
        }

        [XmlRoot(ElementName = "Maps")]
        public class Maps {
            [XmlElement(ElementName = "Map")] public List<Map> MapList { get; set; }
        }

        [XmlRoot(ElementName = "Connection")]
        public class Connection {
            [XmlAttribute(AttributeName = "from")] public string From { get; set; }
            [XmlAttribute(AttributeName = "to")] public string To { get; set; }

            [XmlAttribute(AttributeName = "start")]
            public string Start { get; set; }

            [XmlAttribute(AttributeName = "end")] public string End { get; set; }
        }

        [XmlRoot(ElementName = "Connections")]
        public class Connections {
            [XmlElement(ElementName = "Connection")]
            public List<Connection> ConnectionList { get; set; }
        }

        [XmlRoot(ElementName = "MapOverview")]
        public class MapOverview {
            [XmlElement(ElementName = "Maps")] public Maps Maps { get; set; }

            [XmlElement(ElementName = "Connections")]
            public Connections Connections { get; set; }
        }
    }
}
