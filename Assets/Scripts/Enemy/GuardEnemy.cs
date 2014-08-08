using UnityEngine;
using System.Collections;

public class GuardEnemy : BaseEnemy {
    public float wanderSpeed = 2.5f;
    public float attackSpeed = 3.5f;

    [MinValue(0)]
    public float timePerShot = 1.5f;
    [MinValue(0)]
    public float fireRange = 8.0f;

    public GameObject shotPrefab = null;
    public GameObject flamethrowerEffect = null;
    public Transform shotOrigin;

    private float _shotCooldownLeft = 0.0f;

    private AudioClip flameThrowerSFX;

    public override void Awake() {
        base.Awake();
        _currentState = startState;

        sound = SoundManager.getInstance();
        flameThrowerSFX = Resources.Load<AudioClip>("Sounds/SFX/guard_flamethrower");
    }

    void Update() {
        if (isStunned()) {
            return;
        }

        handleStateMachine();
    }

    //Wander state
    protected override void onEnterWanderState() {
        recalculateMovementPatternPath();
    }

    protected override void wanderState() {
        followMovementPattern(wanderSpeed);
        tryEnterState(EnemyState.ATTACKING);
        tryEnterState(EnemyState.FLEEING);
    }

    //Attack state
    protected override bool canEnterAttackState() {
        return getNearestVisibleSlime() != null;
    }

    protected override void onEnterAttackState() {
        _shotCooldownLeft = 0.0f;
    }

    protected override void attackState() {
        if (_shotCooldownLeft >= 0.0f) {
            _shotCooldownLeft -= Time.deltaTime;
            _enemyAnimation.EnemyStopped();
            return;
        }

        if (getNearestVisibleSlime() == null) {
            tryEnterState(EnemyState.WANDERING);
            return;
        }

        if (Vector3.Distance(shotOrigin.position, getNearestVisibleSlime().transform.position) > fireRange ||
            Mathf.Abs(transform.position.y - getNearestVisibleSlime().transform.position.y) > 0.1f) {
                moveTowardsPoint(getNearestVisibleSlime().transform.position, attackSpeed);
        } else {
            if (getNearestVisibleSlime(20, true) != null) {
                _shotCooldownLeft = timePerShot;
                _enemyAnimation.EnemyShoot(getNearestVisibleSlime().transform.position.x > shotOrigin.position.x ? 1.0f : -1.0f);
                sound.PlaySound(gameObject.transform, flameThrowerSFX);
            }
        }
    }

    public void OnEnemyFire() {
        Vector3 direction = transform.localScale.x > 0.0f ? Vector3.right : Vector3.left;
        float fireAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion particleRotation = Quaternion.Euler(0, 0, fireAngle);
        GameObject effect = Instantiate(flamethrowerEffect, shotOrigin.position, particleRotation) as GameObject;
        Destroy(effect, 10.0f);

        for (int i = 0; i < 20; i++) {
            float angleInsideOfCone = fireAngle + Random.Range(-30, 30);
            Quaternion shotRotation = Quaternion.Euler(0, 0, angleInsideOfCone);
            Instantiate(shotPrefab, shotOrigin.position, shotRotation);
        }

        Instantiate(shotPrefab, transform.position, particleRotation);
    }
}
