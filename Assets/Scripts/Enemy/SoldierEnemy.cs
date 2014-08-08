using UnityEngine;
using System.Collections;

public class SoldierEnemy : BaseEnemy {
    public float wanderSpeed = 2.5f;
    public float fleeSpeed = 3.5f;

    [MinValue (0)]
    public int bullets = 20;
    [MinValue (0)]
    public float timePerShot = 1.5f;
    [MinValue (0)]
    public float fireRange = 8.0f;

    public GameObject muzzleFlashEffect;
    public Transform shotSpawn;

    private bool _onShootCooldown = false;

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

        handleStateMachine();
    }

    //Wander state
    protected override void onEnterWanderState(){
        recalculateMovementPatternPath();
    }

    protected override void wanderState() {
        followMovementPattern(wanderSpeed);
        tryEnterState(EnemyState.ATTACKING);
        tryEnterState(EnemyState.FLEEING);
    }

    //Attack state
    protected override bool canEnterAttackState() {
        return getNearestVisibleSlime() != null && bullets != 0;
    }

    protected override void onEnterAttackState(){
 	    _onShootCooldown = false;
    }

    protected override void attackState(){
        if (_onShootCooldown) {
            _enemyAnimation.EnemyStopped();
            return;
        }

        if (bullets == 0) {
            if (!tryEnterState(EnemyState.FLEEING)) {
                tryEnterState(EnemyState.WANDERING);
            }
        } else {
            if (getNearestVisibleSlime() == null) {
                tryEnterState(EnemyState.WANDERING);
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

    public void OnEnemyFire() {
        StartCoroutine(damageSlimeCoroutine());
    }

    protected IEnumerator damageSlimeCoroutine() {
        yield return null;
        if (getNearestVisibleSlime(20, true) != null) {
            Instantiate(muzzleFlashEffect, shotSpawn.position, Quaternion.identity);
            Instantiate(muzzleFlashEffect, getNearestVisibleSlime().transform.position + new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, 0.4f), 0), Quaternion.identity);
            getNearestVisibleSlime().damageSlime(1.5f);
            getNearestVisibleSlime(20, true);
            bullets--;
        }
        yield return new WaitForSeconds(timePerShot);
        _onShootCooldown = false;
    }

    //Flee state
    protected override bool canEnterFleeState() {
        return getNearestVisibleSlime() != null && bullets == 0;
    }

    protected override void fleeState(){
        runAwayFromSlime(fleeSpeed);
    }
}
