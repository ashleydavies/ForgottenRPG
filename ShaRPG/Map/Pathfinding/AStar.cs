using System.Collections.Generic;
using System.Linq;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Map.Pathfinding {
    public class AStar {
        public static List<TileCoordinate> GetPath(MapNode[,] nodes, TileCoordinate beginPos, TileCoordinate endPos) {
            MapNode begin = nodes[beginPos.X, beginPos.Y];
            MapNode end = nodes[endPos.X, endPos.Y];

            begin.G = 0;
            begin.H = begin.F = begin.HeuristicTo(end);
            begin.Parent = null;

            HashSet<MapNode> open = new HashSet<MapNode>();
            HashSet<MapNode> closed = new HashSet<MapNode>();

            open.Add(begin);

            while (true) {
                MapNode current = open.ToList().OrderBy(x => x.F).FirstOrDefault();

                if (current == null) return null;

                if (current == end) break;

                open.Remove(current);
                closed.Add(current);

                foreach (MapNode neighbor in current.Neighbors) {
                    if (neighbor.Collideable) continue;

                    int g = current.G + neighbor.Cost;

                    if (g < neighbor.G) {
                        open.Remove(neighbor);
                        closed.Remove(neighbor);
                    }

                    if (open.Contains(neighbor) || closed.Contains(neighbor)) continue;

                    neighbor.G = g;
                    neighbor.H = neighbor.HeuristicTo(end);
                    neighbor.F = neighbor.G + neighbor.H;
                    neighbor.Parent = current;
                    open.Add(neighbor);
                }
            }

            List<TileCoordinate> path = new List<TileCoordinate>();

            for (MapNode current = end; current.Parent != null; current = current.Parent) path.Add(current.Position);

            path.Reverse();

            return path;
        }
    }
}
