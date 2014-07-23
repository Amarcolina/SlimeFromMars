using UnityEngine;
using System.Collections;

public enum ScientistState {
    WANDERING,
    WAITING,
    FLEEING
}

public class ScientistEnemy : BaseEnemy {
    public ScientistState startState = ScientistState.WANDERING;

    public float wanderSpeed = 2.5f;
    public float fleeSpeed = 3.5f;
    public float waitTime = 1.0f;

    public Transform[] waypoints = null;
    //public WaypointEndAction patrolEndAction = WaypointEndAction.REVERSE;

    private ScientistState _currentState;
    private int _currentWaypoint = 0;
    private bool _forwardToNextWaypoint = true;
    private Path _currentPath = null;
    private float _timeLeftToWait = 0.0f;

    public override void Awake() {
        base.Awake();
        _currentState = startState;
    }

    void Update() {
        if (isOnSlimeTile()) {
            Destroy(this.gameObject);
            return;
        }

        switch (_currentState) {
            case ScientistState.WANDERING:
                wanderState();
                break;
            case ScientistState.WAITING:
                waitState();
                break;
            case ScientistState.FLEEING:
                fleeState();
                break;
        }
    }

    private bool enterWanderState(Transform waypoint) {
        _currentPath = Astar.findPath(Tilemap.getTilemapLocation(transform.position), Tilemap.getTilemapLocation(waypoint.position));
        if (_currentPath == null) {
            return false;
        }
        _currentState = ScientistState.WANDERING;
        return true;
    }

    private void wanderState() {
        if (moveTowardsPoint(_currentPath.getCurrent(), wanderSpeed)) {
            if (!_currentPath.hasNext()) {
                enterWaitState();
            }
            _currentPath.getNext();
        }
    }

    private bool enterWaitState() {
        _timeLeftToWait = waitTime;
        _currentState = ScientistState.WAITING;
        return true;
    }

    private void waitState() {
        _timeLeftToWait -= Time.deltaTime;
        if (_timeLeftToWait <= 0.0) {

            /*
            if (patrolEndAction == WaypointEndAction.REVERSE) {
                if (_currentWaypoint == waypoints.Length - 1 || _currentWaypoint == 0) {
                    _forwardToNextWaypoint = !_forwardToNextWaypoint;
                }
                _currentWaypoint += _forwardToNextWaypoint ? 1 : -1;
            }else if(patrolEndAction == WaypointEndAction.LOOP){
                _currentWaypoint = (_currentWaypoint + 1) % waypoints.Length;
            }
             * */

            enterWanderState(waypoints[_currentWaypoint]);
        }
    }

    private bool enterFleeState() {
        return false;
    }

    private void fleeState() {

    }

    public void OnDrawGizmosSelected() {
        Gizmos.color = new Color(0.5f, 0.5f, 1.0f, 0.4f);
        foreach (Transform waypoint in waypoints) {
            Gizmos.DrawSphere(waypoint.position, 0.5f);
        }

        Gizmos.color = Color.blue;
        for (int i = 0; i < _currentPath.Count - 1; i++) {
            Gizmos.DrawLine(Tilemap.getWorldLocation(_currentPath[i]), Tilemap.getWorldLocation(_currentPath[i + 1]));
        }
    }
}
