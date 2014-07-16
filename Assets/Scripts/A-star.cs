using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class Astar : MonoBehaviour {
    class Node: IComparable {
        Vector2Int position; //tilemap position of node
        private const float ORTHOGANAL_COST = 1;
        private const float DIAGANOL_COST = 1.5f;
        private Node parent;

        public Node(Vector2Int position, Node parent) {
            this.position = position;
            this.parent = parent;
        }
        //Using Diaganol (Chebyshev) distance - grid allows 8 directions of movement
        //Calculates the heuristic of the node
        private float heuristic(Vector2Int goal){
            float dx = Mathf.Abs(position.x - goal.x);
            float dy = Mathf.Abs(position.y - goal.y);
            return ORTHOGANAL_COST * (dx + dy) + (DIAGANOL_COST - 2 * ORTHOGANAL_COST) * Mathf.Min(dx, dy);
        } 
    }

    public List<Node> Astar(Vector2Int start, Vector2Int goal){
        List<Node> finalPath = new List<Node>();//path from start to goal
        BinaryMinHeap<Node> openList = new BinaryMinHeap<Node>();//nodes to be examined
        HashSet<Node> closedList = new HashSet<Node>();

        Node startNode = new Node(start, null);
        openList.insert(startNode);
        

        return finalPath;
        /*
    OPEN = priority queue containing START
CLOSED = empty set
while lowest rank in OPEN is not the GOAL:
  current = remove lowest rank item from OPEN
  add current to CLOSED
  for neighbors of current:
    cost = g(current) + movementcost(current, neighbor)
    if neighbor in OPEN and cost less than g(neighbor):
      remove neighbor from OPEN, because new path is better
    if neighbor in CLOSED and cost less than g(neighbor): **
      remove neighbor from CLOSED
    if neighbor not in OPEN and neighbor not in CLOSED:
      set g(neighbor) to cost
      add neighbor to OPEN
      set priority queue rank to g(neighbor) + h(neighbor)
      set neighbor's parent to current

reconstruct reverse path from goal to start
by following parent pointers
         */
    }
}
