using System;

namespace ForgottenRPG.Util {
    public enum Direction {
        North,
        East,
        South,
        West
    }

    public static class DirectionUtils {
        public static Direction Opposite(this Direction dir) {
            return dir switch {
                Direction.North => Direction.South,
                Direction.East  => Direction.West,
                Direction.South => Direction.North,
                Direction.West  => Direction.East,
                _               => throw new ArgumentOutOfRangeException(nameof(dir), dir, "unknown direction")
            };
        }

        public static Direction FromString(string dir) {
            return dir.ToLower() switch {
                "north" => Direction.North,
                "east"  => Direction.East,
                "south" => Direction.South,
                "west"  => Direction.West,
                _       => throw new ArgumentException("Unknown CardinalDirection: " + dir)
            };
        }
    }
}
