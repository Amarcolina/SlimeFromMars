using UnityEngine;
using System.Collections;

public enum GuardState {
    WANDERING,
    ATTACKING
}

public class GuardEnemy : BaseEnemy {
    public GuardState startState = GuardState.WANDERING;

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
    private GuardState _currentState;

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
            _shotCooldownLeft = 0.0f;
            return true;
        }
        return false;
    }

    private void attackState() {
        if (_shotCooldownLeft >= 0.0f) {
            _shotCooldownLeft -= Time.deltaTime;
            _enemyAnimation.EnemyStopped();
            return;
        }

        if (getNearestVisibleSlime() == null) {
            enterWanderState();
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
                _shotCooldownLeft = timePerShot;
                _enemyAnimation.EnemyShoot(getNearestVisibleSlime().transform.position.x > shotOrigin.position.x ? 1.0f : -1.0f);
                gameObject.GetComponent<SoundEffect>().PlaySound(flameThrowerSFX);
                
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
