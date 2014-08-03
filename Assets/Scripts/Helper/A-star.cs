using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public delegate float AStarMovementCost(Vector2Int position, Vector2Int neighbor);
public delegate float AStarNodeHeuristic(Vector2Int position, Vector2Int goal);
public delegate bool AStarIsPathWalkable(Vector2Int position);
public delegate bool AStarEarlySuccessFunction(Vector2Int position);
public delegate bool AStarEarlyFailureFunction(Vector2Int position);

public class AstarSettings {
    public AStarMovementCost movementCostFunction = Astar.defaultMovementCost;
    public AStarNodeHeuristic heuristicFunction = Astar.defaultHeuristic;
    public AStarIsPathWalkable isWalkableFunction = Tile.isWalkableFunction;
    public AStarEarlySuccessFunction earlySuccessFunction = null;
    public AStarEarlyFailureFunction earlyFailureFunction = null;
    public AStarIsPathWalkable isNeighborWalkableFunction = Tile.isWalkableFunction;
    public int maxNodesToCheck = -1;

    public AstarSettings() { }

    public AstarSettings(AstarSettings settings) {
        this.movementCostFunction = settings.movementCostFunction;
        this.heuristicFunction = settings.heuristicFunction;
        this.isWalkableFunction = settings.isWalkableFunction;
        this.earlySuccessFunction = settings.earlySuccessFunction;
        this.earlyFailureFunction = settings.earlyFailureFunction;
        this.isNeighborWalkableFunction = settings.isNeighborWalkableFunction;
        this.maxNodesToCheck = settings.maxNodesToCheck;
    }
}

public class Astar : MonoBehaviour {
    private static AstarSettings defaultSettings = new AstarSettings();

    public class Node : IComparable {
        Vector2Int position; //tilemap position of node
        private Node parent;
        private float totalCostFromStart, heuristic;

        public Node(Vector2Int position, Vector2Int goal, Node parent, float heuristic) {
            this.position = position;
            this.parent = parent;
            this.heuristic = heuristic;
        }

        public float calculateFCost() {
            return (totalCostFromStart + heuristic);
        }
        public float getTotalCostFromStart() {
            return totalCostFromStart;
        }
        public void setTotalCostFromStart(float totalCostFromStart) {
            this.totalCostFromStart = totalCostFromStart;

        }
        public Vector2Int getPosition() {
            return position;
        }
        public Node getParent() {
            return parent;
        }
        public void setParent(Node parent) {
            this.parent = parent;
        }

        //Using Diaganol (Chebyshev) distance - grid allows 8 directions of movement
        //Calculates the heuristic of the node
        public int CompareTo(object obj) {
            if (obj == null) {
                return 1;
            }

            Node node = obj as Node;
            if (node != null) {
                return this.calculateFCost().CompareTo(node.calculateFCost());
            } else {
                throw new ArgumentException("Object is not a Temperature");
            }
        }
    }

    public static Path findPath(Vector2Int start, Vector2Int goal, AstarSettings settings = null){
        if (settings == null) {
            settings = defaultSettings;
        }

        Path path = new Path();
        findPathInternal(path, start, goal, settings);

        return path.Count == 0 ? null : path;
    }

    public static IEnumerator findPathCoroutine(Path path, Vector2Int start, Vector2Int goal, AstarSettings settings = null) {
        if (settings == null) {
            settings = defaultSettings;
        }

        yield return findPathInternal(path, start, goal, settings);
    }

    private static IEnumerator findPathInternal(Path path, Vector2Int start, Vector2Int goal, AstarSettings settings) {
        if (start == null || goal == null) {
            yield break;
        }

        if (!settings.isWalkableFunction(goal)) {
            yield break;
        }

        MinHashHeap<Node> openList = new MinHashHeap<Node>();//nodes to be examined
        HashSet<Node> closedList = new HashSet<Node>();
        Dictionary<Vector2Int, Node> nodePostionMap = new Dictionary<Vector2Int, Node>();
        Node startNode = new Node(start, goal, null, settings.heuristicFunction(start, goal));
        Node goalNode = null;
        nodePostionMap.Add(startNode.getPosition(), startNode);

        openList.insert(startNode);
        int nodesChecked = 0;

        Node current = null;
        while(true){
            current = openList.extractElement(0);//remove lowest rank node from openList

            //We break the while loop if we have found the goal node
            if (current.getPosition() == goal) {
                break;
            }

            //We break the while loop if the earlySuccessFunction is met
            if (settings.earlySuccessFunction != null && settings.earlySuccessFunction(current.getPosition())) {
                break;
            }

            //We terminate the function and return no path found if the earlyFailureFunction is met
            if (settings.earlyFailureFunction != null && settings.earlyFailureFunction(current.getPosition())) {
                yield break;
            }

            closedList.Add(current);//add current to closedList

            //for neighbors of current:
            for(int neighborOffsetIndex = 0; neighborOffsetIndex < TilemapUtilities.neighborFullArray.Length; neighborOffsetIndex++) {
                Vector2Int neighborPosition = current.getPosition() + TilemapUtilities.neighborFullArray[neighborOffsetIndex];
                if (!TilemapUtilities.areTilesNeighbors(current.getPosition(), neighborPosition, true, settings.isNeighborWalkableFunction)) {
                    continue;
                }

                if (!settings.isWalkableFunction(neighborPosition)) {
                    continue;
                }

                //checks for element in dictionary, then adds if non-existent
                Node neighborNode = null;
                if (!nodePostionMap.TryGetValue(neighborPosition, out neighborNode)) {
                    neighborNode = new Node(neighborPosition, goal, current, settings.heuristicFunction(neighborPosition, goal));
                    nodePostionMap.Add(neighborNode.getPosition(), neighborNode);
                }

                float costFromStartToNeighbor = (current.getTotalCostFromStart() + settings.movementCostFunction(current.getPosition(), neighborNode.getPosition()));
                if (openList.contains(neighborNode) && costFromStartToNeighbor < neighborNode.getTotalCostFromStart()) {//if neighbor in OPEN and cost less than g(neighbor):
                    openList.extractElement(neighborNode);
                }
                if (closedList.Contains(neighborNode) && costFromStartToNeighbor < neighborNode.getTotalCostFromStart()) { //if neighbor in CLOSED and cost less than g(neighbor): **
                    closedList.Remove(neighborNode);// remove neighbor from CLOSED
                }
                if (!openList.contains(neighborNode) && !closedList.Contains(neighborNode)) {
                    neighborNode.setTotalCostFromStart(costFromStartToNeighbor);
                    openList.insert(neighborNode);
                    neighborNode.setParent(current);
                }
            }

            //If the heap is zero, that means we have not found the goal and we have run out
            //of places to check.  In this case we have failed to find a path and we must
            //return null
            if (openList.getHeapSize() == 0) {
                yield break;
            }

            //if maxNodesToCheck is 0 or less, that is interpreted as unlimited number of nodes
            //if maxNodesToCheck is nonzero, we terminate with no path found 
            nodesChecked++;
            if (settings.maxNodesToCheck > 0 && nodesChecked >= settings.maxNodesToCheck) {
                yield return null;
            }
        }

        //Path finalPath = new Path();//path from start to goal
        goalNode = current;
        //reconstruct reverse path from goal to start by following parent pointers
        path.addNodeToStart(goalNode.getPosition());
        while (goalNode.getParent() != null) {
            goalNode = goalNode.getParent();
            path.addNodeToStart(goalNode.getPosition());
        }
    }

    //the cost of moving directly from one node to another
    public static float defaultMovementCost(Vector2Int current, Vector2Int neighbor) {
        float dx = current.x - neighbor.x;
        float dy = current.y - neighbor.y;
        return (Mathf.Sqrt(dx * dx + dy * dy));
    }

    private const float ORTHOGANAL_COST = 1;
    private const float DIAGANOL_COST = 1.5f;
    public static float defaultHeuristic(Vector2Int node, Vector2Int goal) {
        float dx = Mathf.Abs(node.x - goal.x);
        float dy = Mathf.Abs(node.y - goal.y);
        return ORTHOGANAL_COST * (dx + dy) + (DIAGANOL_COST - 2 * ORTHOGANAL_COST) * Mathf.Min(dx, dy);
    }
}