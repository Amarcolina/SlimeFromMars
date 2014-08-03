using UnityEngine;
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

    private bool _onShootCooldown = false;
    private SoldierState _currentState;

    private AudioClip bulletSFX;

    public override void Awake() {
        base.Awake();
        _currentState = startState;


        bulletSFX = Resources.Load<AudioClip>("Sounds/SFX/soldier_bullet");
    }

    void Update() {
        if (isStunned()) {
            return;
        }

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

        if (bullets == 0) {
            if (tryEnterFleeState()) {
                enterWanderState();
            }
        } else {
            if (getNearestVisibleSlime() == null) {
                enterWanderState();
                return;
            }

            
            if (Vector3.Distance(transform.position, getNearestVisibleSlime().transform.position) > fireRange) {
                moveTowardsPoint(getNearestVisibleSlime().transform.position);
            } else {
                _onShootCooldown = true;
                _enemyAnimation.EnemyShoot(getNearestVisibleSlime().transform.position.x > transform.position.x ? 1.0f : -1.0f);
                    gameObject.AddComponent<SoundEffect>().sfx = bulletSFX;
            }
        }
    }

    protected override void OnEnemyFire() {
        base.OnEnemyFire();
        StartCoroutine(damageSlimeCoroutine());
    }

    protected IEnumerator damageSlimeCoroutine() {
        yield return null;
        if (getNearestVisibleSlime(20, true) != null) {
            getNearestVisibleSlime().damageSlime(1.5f);
            getNearestVisibleSlime(20, true);
            bullets--;
        }
        yield return new WaitForSeconds(timePerShot);
        _onShootCooldown = false;
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
