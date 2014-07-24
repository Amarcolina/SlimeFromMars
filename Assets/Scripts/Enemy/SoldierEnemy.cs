﻿using UnityEngine;
using System.Collections;

public enum SoldierState {
    WANDERING,
    ATTACKING,
    FLEEING
}

public class SoldierEnemy : BaseEnemy {
    public SoldierState startState = SoldierState.WANDERING;

    public float wanderSpeed = 2.5f;
    public float fleeSpeed = 3.5f;

    [MinValue (0)]
    public int bullets = 20;
    [MinValue (0)]
    public float timePerShot = 1.5f;
    [MinValue (0)]
    public float fireRange = 8.0f;

    private float _timeUntilNextShot = 0.0f;
    private SoldierState _currentState;

    public override void Awake() {
        base.Awake();
        _currentState = startState;
    }

    void Update() {
        switch (_currentState) {
            case SoldierState.WANDERING:
                wanderState();
                break;
            case SoldierState.ATTACKING:
                attackState();
                break;
            case SoldierState.FLEEING:
                fleeState();
                break;
            default:
                Debug.LogWarning("Cannot handle state " + _currentState);
                break;
        }
    }

    private void enterWanderState() {
        recalculateMovementPatternPath();
        _currentState = SoldierState.WANDERING;
    }

    private void wanderState() {
        followMovementPattern(wanderSpeed);
        tryEnterAttackState();
        tryEnterFleeState();
    }

    private bool tryEnterAttackState() {
        if (getNearestVisibleSlime() != null && bullets != 0) {
            _currentState = SoldierState.ATTACKING;
            return true;
        }
        return false;
    }

    private void attackState() {
        if (bullets == 0) {
            if (tryEnterFleeState()) {
                enterWanderState();
            }
        } else {
            Slime slime = getNearestVisibleSlime();
            if (slime == null) {
                enterWanderState();
                return;
            }


            if (Vector3.Distance(transform.position, slime.transform.position) > fireRange) {
                moveTowardsPoint(slime.transform.position);
                _timeUntilNextShot = timePerShot;
            } else {
                _timeUntilNextShot -= Time.deltaTime;
                if (_timeUntilNextShot <= 0.0f) {
                    _timeUntilNextShot += timePerShot;
                    slime.damageSlime(1.5f);
                    bullets--;
                }
            }
        }
    }

    private bool tryEnterFleeState() {
        if (getNearestVisibleSlime() != null && bullets == 0) {
            _currentState = SoldierState.FLEEING;
            return true;
        }
        return false;
    }

    private void fleeState() {
        runAwayFromSlime(fleeSpeed);
    }
}
