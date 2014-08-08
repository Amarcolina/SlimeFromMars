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

    private bool _onShootCooldown = false;

    private AudioClip flameThrowerSFX;

    public override void Awake()
    {
        base.Awake();
        _currentState = startState;

        flameThrowerSFX = Resources.Load<AudioClip>("Sounds/SFX/guard_flamethrower");
    }

    void Start()
    {
        gameObject.AddComponent<SoundEffect>();
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
        _onShootCooldown = false;
    }

    protected override void attackState() {
        if (_onShootCooldown) {
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
            if (gameObject.GetComponent<SoundEffect>() != null)
            {
            }
            if (getNearestVisibleSlime(20, true) != null) {
                _onShootCooldown = true;
                _enemyAnimation.EnemyShoot(getNearestVisibleSlime().transform.position.x > shotOrigin.position.x ? 1.0f : -1.0f);
                gameObject.GetComponent<SoundEffect>().PlaySound(flameThrowerSFX);
                
            }
        }
    }

    public void OnEnemyFire() {
        StartCoroutine(fireWaitCoroutine());

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

    private IEnumerator fireWaitCoroutine() {
        yield return new WaitForSeconds(timePerShot);
        _onShootCooldown = false;
    }
}
