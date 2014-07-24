using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Astar : MonoBehaviour {
    public delegate float AStarMovementCost(Vector2Int position, Vector2Int neighbor);
    public delegate float AStarNodeHeuristic(Vector2Int position, Vector2Int goal);
    public delegate bool AStarIsPathWalkable(Vector2Int position);

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

    public static Path findPath(Vector2Int start, Vector2Int goal) {
        return findPath(start, goal, defaultHeuristic, defaultIsWalkable, defaultMovementCost);
    }

    public static Path findPath(Vector2Int start, Vector2Int goal, AStarNodeHeuristic heuristicFunction) {
        return findPath(start, goal, heuristicFunction, defaultIsWalkable, defaultMovementCost);
    }

    public static Path findPath(Vector2Int start, Vector2Int goal, AStarIsPathWalkable isWalkableFunction) {
        return findPath(start, goal, defaultHeuristic, isWalkableFunction, defaultMovementCost);
    }

    public static Path findPath(Vector2Int start, Vector2Int goal, AStarMovementCost movementCostFunction) {
        return findPath(start, goal, defaultHeuristic, defaultIsWalkable, movementCostFunction);
    }

    public static Path findPath(Vector2Int start, Vector2Int goal, 
                                AStarNodeHeuristic heuristicFunction, 
                                AStarIsPathWalkable isWalkableFunction, 
                                AStarMovementCost movementCostFunction) {  
        if (start == null || goal == null) {
            return null;
        }

        BinaryMinHeap<Node> openList = new BinaryMinHeap<Node>();//nodes to be examined
        HashSet<Node> closedList = new HashSet<Node>();
        Dictionary<Vector2Int, Node> nodePostionMap = new Dictionary<Vector2Int, Node>();
        Node startNode = new Node(start, goal, null, heuristicFunction(start, goal));
        Node goalNode = null;
        nodePostionMap.Add(startNode.getPosition(), startNode);

        openList.insert(startNode);
        while (openList.peekAtElement(0).getPosition() != goal) { //while lowest rank in openList is not the goal node
            Node current = openList.extractElement(0);//remove lowest rank node from openList
            closedList.Add(current);//add current to closedList

            foreach (Vector2Int neighborPosition in TilemapUtilities.getNeighboringPositions(current.getPosition(), true, true)) {//for neighbors of current:
                if (!isWalkableFunction(neighborPosition)) {
                    continue;
                }

                //checks for element in dictionary, then adds if non-existent
                Node neighborNode = null;
                if (!nodePostionMap.TryGetValue(neighborPosition, out neighborNode)) {
                    neighborNode = new Node(neighborPosition, goal, current, heuristicFunction(neighborPosition, goal));
                    nodePostionMap.Add(neighborNode.getPosition(), neighborNode);
                }

                float costFromStartToNeighbor = (current.getTotalCostFromStart() + movementCostFunction(current.getPosition(), neighborNode.getPosition()));
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
                return null;
            }
        }

        Path finalPath = new Path();//path from start to goal
        goalNode = openList.peekAtElement(0);
        //reconstruct reverse path from goal to start by following parent pointers
        finalPath.addNodeToStart(goalNode.getPosition());
        while (goalNode.getParent() != null) {
            goalNode = goalNode.getParent();
            finalPath.addNodeToStart(goalNode.getPosition());
        }

        return finalPath;
    }

    public static bool defaultIsWalkable(Vector2Int position) {
        return Tilemap.getInstance().isWalkable(position);
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