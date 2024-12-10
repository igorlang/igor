using System.Collections.Generic;

namespace Igor
{
    public enum Direction
    {
        ClientToServer,
        ServerToClient,
    }

    public static class Directions
    {
        public static Direction Opposite(this Direction direction) => direction == Direction.ClientToServer ? Direction.ServerToClient : Direction.ClientToServer;

        public static readonly IReadOnlyList<Direction> Values = new[] { Direction.ClientToServer, Direction.ServerToClient };
    }
}
