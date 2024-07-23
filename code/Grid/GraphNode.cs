namespace Kira.Util;

using System;

public class GraphNode : IEquatable<Vector2Int>
{
    public readonly string name;
    public readonly int x;
    public readonly int y;

    public bool DisplayCameFromDirection { get; set; }

    public Vector2Int Position { get; private set; }

    public bool IsOccupied { get; set; }

    // Is not just apart of the grid / for aesthetic purposes
    public bool IsRealNode { get; set; }

    // Just for displaying a neighbour during a search
    public bool IsNeighbour { get; set; }

    public bool IsReached { get; set; }

    public bool IsWall { get; set; }

    public bool isFrontier { get; set; }

    // is the node currently selected for searching
    public bool IsCurrent { get; set; }

    public bool IsGoal { get; set; }

    public GraphNode CameFrom { get; set; }

    public bool IsHighlightedPath { get; set; }

    // The movement cost
    public int Weight { get; set; }

    public GraphNode(int x, int y, string name = "", bool isRealNode = false, bool isWall = false, int weight = 1)
    {
        this.x = x;
        this.y = y;
        this.Position = new Vector2Int(x, y);
        this.name = name;
        this.IsRealNode = isRealNode;
        this.IsWall = isWall;
        this.Weight = weight;
    }

    public bool Equals(Vector2Int other)
    {
        return other.x == x && other.y == y;
    }
}