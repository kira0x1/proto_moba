using System;
using System.Threading.Tasks;

namespace Kira.Util;

public class Graph
{
    public int GridRows { get; set; }
    public int GridCols { get; set; }
    public List<GraphNode> AllNodes { get; set; }
    public List<GraphNode> RealNodes { get; set; }

    private readonly char[] Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    // Node Dictionary key = Node, value = the previous node in the path
    private Dictionary<GraphNode, GraphNode> CameFrom { get; set; }
    private Dictionary<GraphNode, int> Weights { get; set; }

    public bool IsSearching { get; private set; }
    public GraphNode StartNode { get; }
    public GraphNode GoalNode { get; }
    public int SearchDelay { get; set; } = 80;

    public Graph(int rows = 5, int cols = 5)
    {
        AllNodes = new List<GraphNode>();
        Weights = new Dictionary<GraphNode, int>();

        GridRows = rows;
        GridCols = cols;

        int i = 0;
        int j = 0;

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                string letter = Letters[i % Letters.Length].ToString();

                if (i >= Letters.Length)
                {
                    letter += $"{j}";
                    j++;
                }

                var xRand = Random.Shared.Int(1, 3);
                var yRand = Random.Shared.Int(1, 3);

                var isRealNode = x % xRand == 0 && y % yRand == 0;
                var node = new GraphNode(pos.x, pos.y, letter, isRealNode, weight: 1);
                AllNodes.Add(node);
                Weights[node] = node.Weight;
                i++;
            }
        }

        // Set Wall's
        // Left Side
        AllNodes[(GridRows - 2) / 2 + GridCols].IsWall = true;
        AllNodes[(GridRows - 1) / 2 + GridCols].IsWall = true;
        AllNodes[(GridRows + 1) / 2 + GridCols].IsWall = true;
        AllNodes[GridRows - 1 + GridCols + 1].IsWall = true;
        AllNodes[GridRows + GridCols * 2].IsWall = true;
        AllNodes[GridRows].IsWall = true;

        // Right Side
        var wallIndexRight = GridRows * 2 + GridCols * 2;
        // AllNodes[wallIndexRight + GridRows + 2].IsWall = true;
        AllNodes[wallIndexRight + GridRows + 3].IsWall = true;
        AllNodes[wallIndexRight + 1].IsWall = true;

        // Set Goal node
        var goalIndex = (cols) * (rows + 1) / 4;
        AllNodes[goalIndex].IsGoal = true;
        GoalNode = AllNodes[goalIndex];

        // Set the center slot as occupied
        int startIndex = (cols + 2) * (rows + 1) / 2;
        AllNodes[startIndex].IsOccupied = true;
        StartNode = AllNodes[startIndex];
    }

    public void CancelSearch()
    {
        IsSearching = false;
    }

    public void ResetNodes()
    {
        foreach (GraphNode node in AllNodes)
        {
            node.IsCurrent = false;
            node.isFrontier = false;
            node.IsNeighbour = false;
            node.IsReached = false;
            node.IsHighlightedPath = false;
            node.DisplayCameFromDirection = false;
        }
    }

    /// <summary>
    /// Takes into account distance costs
    /// </summary>
    public async Task StartSearchWithDistance()
    {
        if (IsSearching) return;
        IsSearching = true;

        var start = AllNodes.Find(x => x.IsOccupied);
        var frontier = new PriorityQueue<GraphNode, int>();
        var costSoFar = new Dictionary<GraphNode, int>();
        CameFrom = new Dictionary<GraphNode, GraphNode>();

        frontier.Enqueue(start, 0);
        costSoFar[start] = 0;
        CameFrom.Add(start, null);

        GraphNode previousCurrent = null;
        List<GraphNode> prevNeighbours = new List<GraphNode>();

        while (frontier.Count > 0 && IsSearching)
        {
            if (!IsSearching) break;

            // Set new current node
            var current = frontier.Dequeue();
            current.IsCurrent = true;

            // Reset Previous Current
            if (previousCurrent != null)
            {
                previousCurrent.IsCurrent = false;
                previousCurrent.isFrontier = false;
                previousCurrent.IsReached = true;
            }

            // Reset Previous Neighbours
            foreach (GraphNode prevNeighbour in prevNeighbours)
            {
                prevNeighbour.IsReached = true;
                prevNeighbour.IsNeighbour = false;
                prevNeighbour.isFrontier = false;
            }

            previousCurrent = current;

            // Early Exit
            if (current == GoalNode)
            {
                break;
            }

            var neighbours = Neighbours(current);

            // Where the main path's actually set
            foreach (GraphNode nb in neighbours)
            {
                if (nb.IsWall) continue;
                nb.IsNeighbour = true;

                var newCost = costSoFar[current] + Cost(current, nb);

                if (!costSoFar.ContainsKey(nb) || newCost < costSoFar[nb])
                {
                    nb.isFrontier = true;
                    nb.CameFrom = current;
                    nb.DisplayCameFromDirection = true;

                    costSoFar[nb] = newCost;
                    frontier.Enqueue(nb, newCost);
                    CameFrom.Add(nb, current);
                }
            }

            prevNeighbours = neighbours;
            await Task.Delay(SearchDelay);
        }

        // Reset previous neighbours so our ui knows not to display them as neighbours
        foreach (GraphNode prevNeighbour in prevNeighbours)
        {
            prevNeighbour.IsNeighbour = false;
            prevNeighbour.isFrontier = false;
        }


        // Reset last "current" for our ui 
        if (previousCurrent != null)
        {
            previousCurrent.IsCurrent = false;
        }

        IsSearching = false;
    }

    public async Task StartSearch()
    {
        if (IsSearching) return;

        IsSearching = true;

        var start = AllNodes.Find(x => x.IsOccupied);
        var frontier = new Queue<GraphNode>();
        CameFrom = new Dictionary<GraphNode, GraphNode>();

        frontier.Enqueue(start);
        CameFrom.Add(start, null);

        GraphNode previousCurrent = null;
        List<GraphNode> prevNeighbours = new List<GraphNode>();

        while (frontier.Count > 0 && IsSearching)
        {
            if (!IsSearching) break;

            // Set new current node
            var current = frontier.Dequeue();
            current.IsCurrent = true;

            // Reset Previous Current
            if (previousCurrent != null)
            {
                previousCurrent.IsCurrent = false;
                previousCurrent.isFrontier = false;
                previousCurrent.IsReached = true;
            }

            // Reset Previous Neighbours
            foreach (GraphNode prevNeighbour in prevNeighbours)
            {
                prevNeighbour.IsReached = true;
                prevNeighbour.IsNeighbour = false;
                prevNeighbour.isFrontier = false;
            }

            previousCurrent = current;

            // Early Exit
            if (current == GoalNode)
            {
                break;
            }

            var neighbours = Neighbours(current);

            // Where the main path's actually set
            foreach (GraphNode nb in neighbours)
            {
                if (nb.IsWall) continue;
                nb.IsNeighbour = true;

                if (!CameFrom.ContainsKey(nb))
                {
                    // nb.IsReached = true;
                    nb.isFrontier = true;
                    nb.CameFrom = current;
                    nb.DisplayCameFromDirection = true;
                    frontier.Enqueue(nb);
                    CameFrom.Add(nb, current);
                }
            }

            prevNeighbours = neighbours;
            await Task.Delay(SearchDelay);
        }

        // Reset previous neighbours so our ui knows not to display them as neighbours
        foreach (GraphNode prevNeighbour in prevNeighbours)
        {
            prevNeighbour.IsNeighbour = false;
            prevNeighbour.isFrontier = false;
        }


        // Reset last "current" for our ui 
        if (previousCurrent != null)
        {
            previousCurrent.IsCurrent = false;
        }

        IsSearching = false;
    }

    private int Cost(GraphNode current, GraphNode next)
    {
        return Weights[next];
    }

    public async void FindPathFromGoal()
    {
        await FindPathFromNode(GoalNode);
    }

    public async Task FindPathFromNode(GraphNode node)
    {
        var current = node;
        var path = new Stack<GraphNode>();
        IsSearching = true;

        while (current != StartNode)
        {
            if (current != GoalNode)
            {
                current.IsHighlightedPath = true;
            }

            path.Push(current);
            current = CameFrom[current];
            await Task.Delay(170);
        }

        IsSearching = false;

        // path.Append(StartNode);
        // path.Reverse();
    }

    public List<GraphNode> Neighbours(GraphNode node)
    {
        Vector2Int[] dirs =
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1)
        };

        List<GraphNode> neighbors = new List<GraphNode>();

        foreach (var dir in dirs)
        {
            // neighbour position
            var npos = new Vector2Int(node.x + dir.x, node.y + dir.y);

            // neighbour node
            var nb = AllNodes.Find(x => x.Equals(npos));

            if (nb != null)
            {
                neighbors.Add(nb);
            }
        }

        return neighbors;
    }

    public GraphNode FindNode(Vector2Int pos)
    {
        GraphNode node = AllNodes.Find(n => n.Equals(pos));
        return node;
    }

    public GraphNode FindNode(int x, int y)
    {
        GraphNode node = AllNodes.Find(n => n.Equals(new Vector2Int(x, y)));
        return node;
    }
}