﻿using UnityEngine;
using System.Collections;

public enum ScientistState {
    WANDERING,
    FLEEING,
    STUNNED
}

public class ScientistEnemy : BaseEnemy {
    public const float STUN_COOLDOWN = 1.0f;

    public ScientistState startState = ScientistState.WANDERING;

    public float wanderSpeed = 2.5f;
    public float fleeSpeed = 3.5f;

    private ScientistState _currentState;

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

    private bool tryEnterFleeState() {
        if (getNearestVisibleSlime() != null) {
            _currentState = ScientistState.FLEEING;
            return true;
        }
        return false;
    }

    private void fleeState() {
        runAwayFromSlime(fleeSpeed);
    }
}
