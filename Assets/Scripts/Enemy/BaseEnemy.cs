using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseEnemy : MonoBehaviour{
    public MovementPattern movementPattern;

    protected int _waypointIndex = 0;
    protected Path _currentWaypointPath = null;
    protected float _timeUntilNextWaypoint = 0.0f;
    protected Tilemap _tilemap = null;

    public virtual void Awake(){
        _tilemap = Tilemap.getInstance();
    }

    //Checks to see if the enemytileobject has a slime component on its tile
    public bool isOnSlimeTile(){
        GameObject tileGameObject = _tilemap.getTileGameObject(transform.position);
        return tileGameObject.GetComponent<Slime>() != null;
    }

    protected Waypoint followMovementPattern(float speed = 2.5f) {
        Waypoint currentWaypoint = movementPattern[_waypointIndex];

        //If we are currently waiting at a waypoint, decrease that timer
        if (_timeUntilNextWaypoint >= 0) {
            _timeUntilNextWaypoint -= Time.deltaTime;
        } else {
            //If we currently don't have a path, find one
            if (_currentWaypointPath == null) {
                recalculateMovementPatternPath(currentWaypoint);
            }

            //Move towards the current waypoint
            if (_currentWaypointPath != null){
                if (followPath(_currentWaypointPath, speed)) {
                    //Once we get there we wait for the given wait time before continuing
                    _timeUntilNextWaypoint = currentWaypoint.getWaitTime();

                    //Set the current path to null since we are at the end of it
                    _currentWaypointPath = null;
                    _waypointIndex++;
                }
            }
        }
        return currentWaypoint;
    }

    protected void recalculateMovementPatternPath(Waypoint waypoint = null) {
        if (waypoint == null) {
            waypoint = movementPattern[_waypointIndex];
        }

        _currentWaypointPath = Astar.findPath(Tilemap.getTilemapLocation(transform.position), 
                                              Tilemap.getTilemapLocation(waypoint.transform.position));
    }

    protected bool moveTowardsPoint(Vector2 destination, float speed = 2.5f) {
        transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        return new Vector2(transform.position.x, transform.position.y) == destination;
    }

    protected bool moveTowardsPoint(Vector2Int target, float speed = 2.5f) {
        return moveTowardsPoint(Tilemap.getWorldLocation(target), speed);
    }

    protected bool followPath(Path path, float speed = 2.5f) {
        Vector2Int node = path.getCurrent();
        if (moveTowardsPoint(node, speed)) {
            if (!path.hasNext()) {
                return true;
            }
            path.getNext();
        }
        return false;
    }
}