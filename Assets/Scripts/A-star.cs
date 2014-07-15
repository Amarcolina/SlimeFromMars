using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Astar : MonoBehaviour {
    //Diagnol (Chebyshev) distance - grid allows 8 directions of movement
    float start, goal, D;
    //if cost is D
    private float heuristic(node){
    float dx = abs(node.x - goal.x);
    float dy = abs(node.y - goal.y);
    return D * max(dx, dy);
    }
    //if cost !D
    private float heuristic(node){
    float dx = abs(node.x - goal.x);
    float dy = abs(node.y - goal.y);
    return D * (dx + dy) + (D2 - 2 * D) * min(dx, dy);
    }

    public List<Node> A-star(){
         private BinaryHeap openList = new BinaryHeap();

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
    }
   
}
