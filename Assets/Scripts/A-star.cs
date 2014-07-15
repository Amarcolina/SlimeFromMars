using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class Astar : MonoBehaviour {
    class Node: IComparable {
        Vector2Int position; //tilemap position of node
        private const float ORTHOGANAL_COST = 1;
        private const float DIAGANOL_COST = 1.5f;

        //Using Diaganol (Chebyshev) distance - grid allows 8 directions of movement
        //Calculates the heuristic of the node
        private float heuristic(Vector2Int goal){
            float dx = Mathf.Abs(position.x - goal.x);
            float dy = Mathf.Abs(position.y - goal.y);
            return ORTHOGANAL_COST * (dx + dy) + (DIAGANOL_COST - 2 * ORTHOGANAL_COST) * Mathf.Min(dx, dy);
        } 
        
        /*//if cost is D
        private float heuristic(Node node){
        float dx = Mathf.Abs(node.x - goal.x);
        float dy = Mathf.Abs(node.y - goal.y);
        return D * Mathf.Max(dx, dy);
        }*/
    }
    

   

    public List<Node> Astar(){
        private BinaryMinHeap<Node> openList = new BinaryMinHeap<Node>();
        private List<Node> path = new List<Node>();
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
    return path;
    }
   
}
