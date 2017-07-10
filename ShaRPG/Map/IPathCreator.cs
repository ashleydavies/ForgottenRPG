using System.Collections.Generic;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Map {
    public interface IPathCreator {
        List<TileCoordinate> GetPath(TileCoordinate start, TileCoordinate finish);
    }
}
