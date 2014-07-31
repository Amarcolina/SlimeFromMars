using UnityEngine;
using System.Collections;

public enum ScientistState {
    WANDERING,
    FLEEING,
    HIDING
}

public class ScientistEnemy : BaseEnemy {
    public ScientistState startState = ScientistState.WANDERING;

    public float wanderSpeed = 2.5f;
    public float fleeSpeed = 3.5f;

    private ScientistState _currentState;

    private float _timeStartedFleeing = 0;

    public override void Awake() {
        base.Awake();
        _currentState = startState;
    }

    void Update() {
        if (isStunned()) {
            return;
        }

        switch (_currentState) {
            case ScientistState.WANDERING:
                wanderState();
                break;
            case ScientistState.FLEEING:
                fleeState();
                break;
            case ScientistState.HIDING:
                hideState();
                break;
            default:
                Debug.LogWarning("Cannot handle state " + _currentState);
                break;
        }
    }

    private void enterWanderState() {
        recalculateMovementPatternPath();
        _currentState = ScientistState.WANDERING;
    }

    private void wanderState() {
        followMovementPattern(wanderSpeed);
        tryEnterFleeState();
    }

    private bool tryEnterFleeState(int searchDistance = 20) {
        if (getNearestVisibleSlime(searchDistance) != null) {
            _currentState = ScientistState.FLEEING;
            _timeStartedFleeing = Time.time;
            return true;
        }
        return false;
    }

    private void fleeState() {
        if (Time.time - getLastTimeViewedSlime() > 15.0f) {
            movementPattern = null;
            float closestDistance = float.MaxValue;
            foreach (MovementPattern pattern in MovementPattern.getAllMovementPatterns()) {
                if (pattern.isRecursive()) {
                    continue;
                }
                float distance = Vector3.Distance(transform.position, pattern.transform.position);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    movementPattern = pattern;
                }
            }
            enterWanderState();
        }

        bool didFindHidingSpot = runAwayFromSlime(fleeSpeed);

        if (didFindHidingSpot && Time.time - _timeStartedFleeing > 45.0f) {
            enterHideState();
        }
    }

    private void enterHideState() {
        _currentState = ScientistState.HIDING;
    }

    private void hideState() {
        tryEnterFleeState(1);
    }
}
