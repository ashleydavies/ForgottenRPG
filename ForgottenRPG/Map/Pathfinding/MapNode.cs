using ForgottenRPG.Util.Coordinate;

namespace ForgottenRPG.Map.Pathfinding {
    public class MapNode {
        public GameMap Map { get; set; }
        public MapNode[] Neighbors;
        public MapNode Parent;
        public TileCoordinate Position;
        public int F, G, H, Cost;
        public bool Collideable => Map.Collideable(Position);

        public int HeuristicTo(MapNode other) => Position.ManhattanDistance(other.Position);
    }
}
