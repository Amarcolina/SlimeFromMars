using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseEnemy : MonoBehaviour{
    public MovementPattern movementPattern;

    protected int _waypointIndex = 0;
    protected Path _currentWaypointPath = null;
    protected float _timeUntilNextWaypoint = 0.0f;
    protected Tilemap _tilemap = null;

    protected Slime _currentSlimeToFleeFrom = null;
    protected Path _fleePath = null;

    public virtual void Awake(){
        _tilemap = Tilemap.getInstance();
    }

    //Checks to see if the enemytileobject has a slime component on its tile
    public bool isOnSlimeTile(){
        GameObject tileGameObject = _tilemap.getTileGameObject(transform.position);
        return tileGameObject.GetComponent<Slime>() != null;
    }

    /* Calling this function every frame will result in the enemy following
     * their set movement path.  This handles everything from pathing using
     * Astar to automatically re-pathing if a path gets blocked.
     * 
     * If you are resuming a movement pattern after moving off course,
     * it is recomended to call recalculateMovementPatternPath() so that
     * the path is recalculated to be from your current position, rather than
     * trying to use the old stale path
     */
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

    /* Recalculates the current path being used to follow the 
     * enemies movement pattern.  Use this if you want to update the
     * path (if the path has become stale or if you want to ensure that
     * the shortest path is followed after the environmnt has changed)
     */
    protected void recalculateMovementPatternPath(Waypoint waypoint = null) {
        if (waypoint == null) {
            waypoint = movementPattern[_waypointIndex];
        }

        _currentWaypointPath = Astar.findPath(transform.position, waypoint.transform.position);
    }

    /* Calling this method every frame will move the enemy towards a given destination
     * at a specific speed.  This method will return true once the enemy has reached
     * that destination
     */
    protected bool moveTowardsPoint(Vector2 destination, float speed = 2.5f) {
        transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        return new Vector2(transform.position.x, transform.position.y) == destination;
    }

    /* Calling this method every frame will move the enemy allong a given path
     * at a specific speed.  This method will return true once the enemy has
     * reached the end of the path.
     */
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

    private Vector2Int[] _directionList = {new Vector2Int( 1,  1),
                                           new Vector2Int( 0,  1),
                                           new Vector2Int(-1,  1),
                                           new Vector2Int(-1,  0),
                                           new Vector2Int(-1, -1),
                                           new Vector2Int( 0, -1),
                                           new Vector2Int( 1, -1),
                                           new Vector2Int( 1,  0)};

    /* This method returns all of the slime tiles visible to this enemy
     * within a given tile radius.  This only searches along the 4 main
     * axes and the 4 diagonals, so it will not return correctly in every 
     * case, and will only ever return up to a maximum of 8 tiles as a
     * consequense
     */
    protected List<Slime> getVisibleSlimeTiles(int maxTileDistance = 20) {
        List<Slime> slimeTiles = new List<Slime>();
        foreach(Vector2Int direction in _directionList){
            Slime slime = getNearestSlimeInDirection(direction, maxTileDistance);
            if (slime != null) {
                slimeTiles.Add(slime);
            }
        }
        return slimeTiles;
    }

    /* This method returns the nearest slime to the enemy, or null is none
     * were found inside of the given search radius.  This only searches
     * allon the 4 main axes and the 4 diagonals, so it will not return
     * correctly in every case
     */
    protected Slime getNearestVisibleSlime(int maxTileDistance = 20) {
        Slime nearestSlime = null;
        float closestDistance = float.MaxValue;
        foreach (Vector2Int direction in _directionList) {
            Slime slime = getNearestSlimeInDirection(direction, maxTileDistance);
            if (slime != null) {
                float dist = (slime.transform.position - transform.position).sqrMagnitude;
                if(dist < closestDistance){
                    nearestSlime = slime;
                    closestDistance = dist;
                }
            }
        }
        return nearestSlime;
    }

    protected bool runAwayFromSlime(float speed = 2.5f) {
        Slime nearestSlime = getNearestVisibleSlime();

        if (_currentSlimeToFleeFrom != null) {
            if (nearestSlime == null || Vector3.Distance(transform.position, nearestSlime.transform.position) > Vector3.Distance(transform.position, _currentSlimeToFleeFrom.transform.position)) {
                nearestSlime = _currentSlimeToFleeFrom;
            }
        }

        if (nearestSlime == null) {
            return false;
        }

        _currentSlimeToFleeFrom = nearestSlime;

        float minCost = _fleePath == null ? float.MaxValue : -Vector3.Distance(nearestSlime.transform.position, _fleePath.getEnd()) - 4.0f;

        for (float checkDistance = 2; checkDistance < 20; checkDistance += 5.0f) {
            for (float angle = 0; angle < 360.0f; angle += 45.0f) {
                Vector3 checkPos = transform.position + Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right * checkDistance;
                Path path = Astar.findPath(transform.position, checkPos);
                if (path != null) {
                    float cost = -Vector3.Distance(nearestSlime.transform.position, checkPos);
                    if (cost < minCost) {
                        minCost = cost - 2.0f;
                        if (path.hasNext()) {
                            path.getNext();
                        }
                        _fleePath = path;
                    }
                }
            }
        }

        if (minCost == float.MaxValue) {
            return false;
        }

        return followPath(_fleePath, speed);
    }

    /* This method can be used to "cast" a ray along a direction until it hits a Slime
     * block, or until a maximum distance is reached.  This method works off of simple
     * integer incrementing, and so passing in directions that have components larger than
     * 1 in magnitude is not recomended, as it could cause walls or objects to be skipped
     * over
     */
    protected Slime getNearestSlimeInDirection(Vector2Int direction, int maxTileDistance = 20) {
        if (Mathf.Abs(direction.x) > 1 || Mathf.Abs(direction.y) > 1) {
            Debug.LogWarning("The direction " + direction + " might have unexpected results");
        }
        Vector2Int ray = Tilemap.getTilemapLocation(transform.position);
        for (int i = 0; i < maxTileDistance; i++) {
            Tile tile = _tilemap.getTile(ray);
            if(tile == null){
                return null;
            }

            if(!tile.isWalkable){
                return null;
            }

            Slime slime = tile.GetComponent<Slime>();
            if (slime != null  &&  slime.isSolid()) {
                return slime;
            }

            ray += direction;
        }
        return null;
    }

    public void OnDrawGizmos() {
        if (_fleePath != null) {
            Gizmos.color = Color.blue;
            for (int i = 0; i < _fleePath.Count - 1; i++) {
                Gizmos.DrawLine(_fleePath[i], _fleePath[i + 1]);
            }
        }
    }
}