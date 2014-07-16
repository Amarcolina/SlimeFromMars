using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class Astar : MonoBehaviour {
    class Node : IComparable {
        Vector2Int position; //tilemap position of node
        private const float ORTHOGANAL_COST = 1;
        private const float DIAGANOL_COST = 1.5f;
        private Node parent;
        private float totalCostFromStart;

        public Node(Vector2Int position, Node parent) {
            this.position = position;
            this.parent = parent;
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
        public void setParent(Node parent){
            this.parent = parent;
        }

        //Using Diaganol (Chebyshev) distance - grid allows 8 directions of movement
        //Calculates the heuristic of the node
        public float heuristic(Vector2Int goal) {
            float dx = Mathf.Abs(position.x - goal.x);
            float dy = Mathf.Abs(position.y - goal.y);
            return ORTHOGANAL_COST * (dx + dy) + (DIAGANOL_COST - 2 * ORTHOGANAL_COST) * Mathf.Min(dx, dy);
        }
    }

    public List<Node> Astar(Vector2Int start, Vector2Int goal){

        List<Node> finalPath = new List<Node>();//path from start to goal
        BinaryMinHeap<Node> openList = new BinaryMinHeap<Node>();//nodes to be examined
        HashSet<Node> closedList = new HashSet<Node>();
        Tilemap tileMap = null;
        Dictionary<Vector2Int, Node> nodePostionMap = new Dictionary<Vector2Int,Node>();
        Node startNode = new Node(start, null);
        nodePostionMap.Add(startNode.getPosition(), startNode);

        openList.insert(startNode);
        while (openList.peekAtElement(0).getPosition() != goal) { //while lowest rank in openList is not the goal node
            Node current = openList.extractElement(0);//remove lowest rank node from openList
            closedList.Add(current);//add current to closedList
            foreach(Vector2Int neighborPosition in tileMap.getNeighboringPositions(current.getPosition())){//for neighbors of current:

                //checks for element in dictionary, then adds if non-existent
                Node neighborNode = null;
                if(!nodePostionMap.TryGetValue(neighborPosition, out neighborNode)){
                    neighborNode = new Node(neighborPosition, current);
                nodePostionMap.Add(neighborNode.getPosition(), neighborNode);
                }

                float costFromStartToNeighbor = (current.getTotalCostFromStart() + movementCost(current, neighborNode)); 
                if(openList.contains(neighborNode) && costFromStartToNeighbor < neighborNode.getTotalCostFromStart()){//if neighbor in OPEN and cost less than g(neighbor):
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
            //reconstruct reverse path from goal to start by following parent pointers
            finalPath.Add(current);
            while (current.getParent() != null) {
                current = current.getParent();
                finalPath.Insert(0, current);
            }
        }
        return finalPath;
    }

    public float movementCost(Node current, Node neighbor){
        float dx = current.getPosition().x - neighbor.getPosition().x;
        float dy = current.getPosition().y - neighbor.getPosition().y;  
        return(Mathf.Sqrt(dx*dx + dy*dy));
    }
}