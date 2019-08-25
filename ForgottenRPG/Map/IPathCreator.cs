using System.Collections.Generic;
using ForgottenRPG.Util.Coordinate;

namespace ForgottenRPG.Map {
    public interface IPathCreator {
        List<TileCoordinate> GetPath(TileCoordinate start, TileCoordinate finish);
    }
}
