namespace Plutonium.Agent.Framework.AStar;

public class Pathfinding
{
    /// <summary>
    ///     Different ways to calculate path distance.
    /// </summary>
    public enum DistanceType
    {
        /// <summary>
        ///     The "ordinary" straight-line distance between two points.
        /// </summary>
        Euclidean,

        /// <summary>
        ///     Distance without diagonals, only horizontal and/or vertical path lines.
        /// </summary>
        Manhattan
    }

    /// <summary>
    ///     Find a path between two points.
    /// </summary>
    /// <param name="grid">Grid to search.</param>
    /// <param name="startPos">Starting position.</param>
    /// <param name="targetPos">Ending position.</param>
    /// <param name="distance">The type of distance, Euclidean or Manhattan.</param>
    /// <param name="ignorePrices">If true, will ignore tile price (how much it "cost" to walk on).</param>
    /// <returns>List of points that represent the path to walk.</returns>
    public static List<Point> FindPath(Grid grid, Point startPos, Point targetPos,
        DistanceType distance = DistanceType.Euclidean, bool ignorePrices = false)
    {
        // find path
        var nodes_path = _ImpFindPath(grid, startPos, targetPos, distance, ignorePrices);

        // convert to a list of points and return
        var ret = new List<Point>();
        if (nodes_path != null)
            foreach (var node in nodes_path)
                ret.Add(new Point(node.gridX, node.gridY));
        return ret;
    }

    /// <summary>
    ///     Internal function that implements the path-finding algorithm.
    /// </summary>
    /// <param name="grid">Grid to search.</param>
    /// <param name="startPos">Starting position.</param>
    /// <param name="targetPos">Ending position.</param>
    /// <param name="distance">The type of distance, Euclidean or Manhattan.</param>
    /// <param name="ignorePrices">If true, will ignore tile price (how much it "cost" to walk on).</param>
    /// <returns>List of grid nodes that represent the path to walk.</returns>
    private static List<Node> _ImpFindPath(Grid grid, Point startPos, Point targetPos,
        DistanceType distance = DistanceType.Euclidean, bool ignorePrices = false)
    {
        var startNode = grid.nodes[startPos.x, startPos.y];
        var targetNode = grid.nodes[targetPos.x, targetPos.y];

        var openSet = new List<Node>();
        var closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            var currentNode = openSet[0];
            for (var i = 1; i < openSet.Count; i++)
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    currentNode = openSet[i];

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode) return RetracePath(grid, startNode, targetNode);

            foreach (Node neighbour in grid.GetNeighbours(currentNode, distance))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour)) continue;

                var newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) *
                    (ignorePrices ? 1 : (int)(10.0f * neighbour.price));
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Retrace path between two points.
    /// </summary>
    /// <param name="grid">Grid to work on.</param>
    /// <param name="startNode">Starting node.</param>
    /// <param name="endNode">Ending (target) node.</param>
    /// <returns>Retraced path between nodes.</returns>
    private static List<Node> RetracePath(Grid grid, Node startNode, Node endNode)
    {
        var path = new List<Node>();
        var currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    ///     Get distance between two nodes.
    /// </summary>
    /// <param name="nodeA">First node.</param>
    /// <param name="nodeB">Second node.</param>
    /// <returns>Distance between nodes.</returns>
    private static int GetDistance(Node nodeA, Node nodeB)
    {
        var dstX = Math.Abs(nodeA.gridX - nodeB.gridX);
        var dstY = Math.Abs(nodeA.gridY - nodeB.gridY);
        return dstX > dstY ? 14 * dstY + 10 * (dstX - dstY) : 14 * dstX + 10 * (dstY - dstX);
    }
}