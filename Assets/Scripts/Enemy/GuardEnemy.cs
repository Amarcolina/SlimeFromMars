using UnityEngine;
using System.Collections;

public enum GuardState {
    WANDERING,
    ATTACKING
}

public class GuardEnemy : BaseEnemy {
    public GuardState startState = GuardState.WANDERING;

    public float wanderSpeed = 2.5f;
    public float fleeSpeed = 3.5f;

    [MinValue(0)]
    public float timePerShot = 1.5f;
    [MinValue(0)]
    public float fireRange = 8.0f;

    public GameObject shotPrefab = null;
    public GameObject flamethrowerEffect = null;

    private bool _onShootCooldown = false;
    private GuardState _currentState;

    public override void Awake() {
        base.Awake();
        _currentState = startState;
    }

    void Update() {
        if (isStunned()) {
            return;
        }

        switch (_currentState) {
            case GuardState.WANDERING:
                wanderState();
                break;
            case GuardState.ATTACKING:
                attackState();
                break;
            default:
                Debug.LogWarning("Cannot handle state " + _currentState);
                break;
        }
    }

    private void enterWanderState() {
        recalculateMovementPatternPath();
        _currentState = GuardState.WANDERING;
    }

    private void wanderState() {
        followMovementPattern(wanderSpeed);
        tryEnterAttackState();
    }

    private bool tryEnterAttackState() {
        if (getNearestVisibleSlime() != null) {
            _currentState = GuardState.ATTACKING;
            _onShootCooldown = false;
            return true;
        }
        return false;
    }

    private void attackState() {
        if (_onShootCooldown) {
            _enemyAnimation.EnemyStopped();
            return;
        }

        if (getNearestVisibleSlime() == null) {
            enterWanderState();
            return;
        }

        if (Vector3.Distance(transform.position, getNearestVisibleSlime().transform.position) > fireRange) {
            moveTowardsPoint(getNearestVisibleSlime().transform.position);
        } else {
            _onShootCooldown = true;
            _enemyAnimation.EnemyShoot(getNearestVisibleSlime().transform.position.x > transform.position.x ? 1.0f : -1.0f);
        }
    }

    public void OnEnemyFire() {
        Vector3 direction = getNearestVisibleSlime().transform.position - transform.position;
        float fireAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion particleRotation = Quaternion.Euler(0, 0, fireAngle);
        GameObject effect = Instantiate(flamethrowerEffect, transform.position, particleRotation) as GameObject;
        Destroy(effect, 10.0f);

        for (int i = 0; i < 20; i++) {
            float angleInsideOfCone = fireAngle + Random.Range(-20, 20);
            Quaternion shotRotation = Quaternion.Euler(0, 0, angleInsideOfCone);
            Instantiate(shotPrefab, transform.position, shotRotation);
        }
    }
}
