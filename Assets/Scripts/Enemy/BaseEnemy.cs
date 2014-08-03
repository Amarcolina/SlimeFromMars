using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseEnemy : MonoBehaviour, IDamageable, IStunnable, IGrabbable{
    public MovementPattern movementPattern;
    public GameObject corpsePrefab = null;
    public float health = 1.0f;

    protected int _waypointIndex = 0;
    protected Path _currentWaypointPath = null;
    protected float _timeUntilNextWaypoint = 0.0f;
    protected Tilemap _tilemap = null;

    protected EnemyAnimation _enemyAnimation;

    protected Slime _currentSlimeToFleeFrom = null;
    protected Path _fleePath = null;

    protected float _canBeStunnedAgainTime = 0.0f;
    protected float _stunEndTime = 0.0f;

    public virtual void Awake(){
        _tilemap = Tilemap.getInstance();
        _enemyAnimation = GetComponent<EnemyAnimation>();
    }

    protected virtual void OnEnemyFire() {
    }

    private int _previousDamageFrame = 0;
    public virtual void damage(float damage) {
        if (health > 0 && Time.frameCount != _previousDamageFrame) {
            _previousDamageFrame = Time.frameCount;
            health -= damage;
            _enemyAnimation.EnemyHit();
            if (health <= 0.0f) {
                StartCoroutine(deathCoroutine());
            }
        }
    }

    private IEnumerator deathCoroutine() {
        yield return new WaitForSeconds(0.5f);
        if (corpsePrefab) {
            Instantiate(corpsePrefab, transform.position, Quaternion.identity);
        }
        Destroy(this);
    }

    public void stun(float duration) {
        if (Time.time > _canBeStunnedAgainTime) {
            _canBeStunnedAgainTime = Time.time + duration + getStunCooldown();
            _stunEndTime = Time.time + duration;
        }
    }

    protected virtual float getStunCooldown() {
        return 1.0f;
    }

    protected bool isStunned() {
        return Time.time <= _stunEndTime;
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
            _enemyAnimation.EnemyStopped();
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
        Vector2 newPosition = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        if (newPosition == (Vector2)transform.position) {
            _enemyAnimation.EnemyStopped();
        } else {
            _enemyAnimation.EnemyMoving(newPosition.x > transform.position.x ? 1.0f : -1.0f);
        }
        transform.position = newPosition;
        return newPosition == destination;
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

    /* This method returns the nearest slime to the enemy, or null is none
     * were found inside of the given search radius.  This only searches
     * allon the 4 main axes and the 4 diagonals, so it will not return
     * correctly in every case
     */
    private Slime _nearestSlime = null;
    private float _nextUpdateNearestSlimeTime = -1;
    private float _lastTimeViewedSlime = 0.0f;
    protected Slime getNearestVisibleSlime(int maxTileDistance = 20, bool forceUpdate = false) {
        if (Time.time < _nextUpdateNearestSlimeTime && !forceUpdate) {
            if (_nearestSlime != null) {
                _lastTimeViewedSlime = Time.time;
            }
            return _nearestSlime;
        }

        _nearestSlime = null;
        float closestDistance = float.MaxValue;
        for (float angle = 0.0f; angle < 360.0f; angle += 22.5f) {
            TileRayHit hit = TilemapUtilities.castTileRay(transform.position, Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right, maxTileDistance, tileRayHitSlime);
            if (hit.didHit) {
                GameObject hitObj = _tilemap.getTileGameObject(hit.hitPosition);
                if (hitObj != null) {
                    Slime slime = hitObj.GetComponent<Slime>();
                    if (slime != null) {
                        float dist = (slime.transform.position - transform.position).sqrMagnitude;
                        if (dist < closestDistance) {
                            _nearestSlime = slime;
                            closestDistance = dist;
                        }
                    }
                }
            }
        }

        if (_nearestSlime == null) {
            _nextUpdateNearestSlimeTime = Time.time + 0.5f;
        } else {
            _nextUpdateNearestSlimeTime = Time.time + 1.7f;
            _lastTimeViewedSlime = Time.time;
        }

        return _nearestSlime;
    }

    protected float getLastTimeViewedSlime() {
        getNearestVisibleSlime();
        return _lastTimeViewedSlime;
    }


    public static bool tileRayHitSlime(GameObject tileObj) {
        if (tileObj == null || !tileObj.GetComponent<Tile>().isTransparent) {
            return true;
        }
        if (tileObj.GetComponent<Slime>() != null) {
            return true;
        }
        return false;
    }

    private Vector2Int _runAwayDirection = new Vector2Int(1, 0);
    private float _nextUpdateRunAwayDirectionTime = -1;
    private int _rotationDirection = 1;
    private float _timeCanRunAwayAgain = 0.0f;

    /*
     * 
     */
    protected bool runAwayFromSlime(float speed = 2.5f) {
        if (Time.time < _timeCanRunAwayAgain) {
            return false;
        }

        if (Time.time > _nextUpdateRunAwayDirectionTime) {
            Slime s = getNearestVisibleSlime();
            if (s != null) {
                Vector2 delta = (transform.position - s.transform.position).normalized;
                _runAwayDirection = Vector2Int.zero;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {
                    _runAwayDirection.x = delta.x > 0.0f ? 1 : -1;
                } else {
                    _runAwayDirection.y = delta.y > 0.0f ? 1 : -1;
                }
                _nextUpdateRunAwayDirectionTime = Time.time + Random.Range(0.5f, 1.5f);
            }
        }

        Vector2Int origin = transform.position;

        if (_tilemap.isWalkable(origin + _runAwayDirection)) {
            Debug.DrawLine(transform.position, origin + _runAwayDirection, Color.green);
            moveTowardsPoint(origin + _runAwayDirection, speed);
        } else {
            Vector2Int rotatedDirection = new Vector2Int(_rotationDirection * _runAwayDirection.y, -_rotationDirection*_runAwayDirection.x);
            if (_tilemap.isWalkable(origin + rotatedDirection)) {
                Debug.DrawLine(transform.position, origin + rotatedDirection, Color.yellow);
                moveTowardsPoint(origin + rotatedDirection, speed);
            } else {
                _runAwayDirection = rotatedDirection;
                if (Random.value > 0.5) {
                    _rotationDirection = -_rotationDirection;
                }
                if (getNearestVisibleSlime() == null) {
                    _timeCanRunAwayAgain = Time.time + Random.Range(1.0f, 2.0f);
                    return true;
                }
            }
        }

        return false;
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