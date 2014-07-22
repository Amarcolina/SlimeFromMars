using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseEnemy : MonoBehaviour{
    protected Tilemap _tilemap = null;

    public virtual void Awake(){
        _tilemap = Tilemap.getInstance();
    }

    //Checks to see if the enemytileobject has a slime component on its tile
    public bool isOnSlimeTile(){
        GameObject tileGameObject = _tilemap.getTileGameObject(transform.position);
        return tileGameObject.GetComponent<Slime>() != null;
    }

    public bool moveTowardsPoint(Vector2Int target, float speed) {
        Vector2 destination = Tilemap.getWorldLocation(target);
        transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        return new Vector2(transform.position.x, transform.position.y) == destination;
    }

    

    /*These methods got commented out because I will still need to implement them in the upcoming days
    * Mostly junk because I still dont have a good understanding of the tile system.

    //Wander movement - follows the specified path
    public void Wander (Path path)
    {
    updatePosition (path);

    }

    //Chase the player based on the path from A*
    public void Chase (Path path)
    {

    //Convert the Vector2 positions of the gameobject into Vector2Int
    Vector2Int startLocation = Tilemap.getTilemapLocation (enemy.transform.position);
    Vector2Int goalLocation = Tilemap.getTilemapLocation (player.transform.position);

    //This path is null here, not sure why
    chase = Astar.findPath (startLocation, goalLocation);
    if (chase == null) {
    Debug.Log("Empty path");
    }
    }


    //Updates the position when walking in a path
    public void updatePosition (Path path)
    {
    Vector3 enemyPos = enemy.transform.position;
    //Vector3 playerPos = player.transform.position;

    //check to see if we reached waypoint
    Vector3 offset = currwaypoint - enemyPos;
    float d = Vector3.SqrMagnitude (offset);
    if (d < 0.0001f) {
    //set next way point with the next node
    nextwaypoint = new Vector3 ((float)path.getNext ().x, (float)path.getNext ().y, 0);
    currwaypoint = nextwaypoint;
    }

    //move player according to the next node in the path
    enemy.transform.position = Vector3.MoveTowards (enemy.transform.position, currwaypoint, speed * Time.deltaTime);
    } */
}