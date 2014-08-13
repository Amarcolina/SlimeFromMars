using UnityEngine;
using System.Collections;

public class SoldierEnemy : BaseEnemy {
    public const int MAX_BULLETS = 8;

    public float wanderSpeed = 2.5f;
    public float fleeSpeed = 3.5f;
    public float startledSpeed = 4.5f;

    [Range (0, MAX_BULLETS)]
    public int bullets = MAX_BULLETS;
    [MinValue (0)]
    public float timePerShot = 1.5f;
    [MinValue (0)]
    public float fireRange = 8.0f;

    public GameObject muzzleFlashEffect;
    public Transform shotSpawn;

    private float _shotCooldownLeft = 0.0f;
    protected ProximitySearcher<WeaponCache> _cacheSearcher;
    protected static WeaponCache[] _weaponCacheArray = null;

    private AudioClip bulletSFX;

    public override void Awake() {
        base.Awake();

        if (_weaponCacheArray == null) {
            _weaponCacheArray = FindObjectsOfType<WeaponCache>();
        }

        _cacheSearcher = new ProximitySearcher<WeaponCache>(_weaponCacheArray, 60.0f);

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
 	    _shotCooldownLeft = 0.0f;
    }

    protected override void attackState(){
        if (_shotCooldownLeft > 0.0f) {
            _shotCooldownLeft -= Time.deltaTime;
            _enemyAnimation.EnemyStopped();
            return;
        }

        if (bullets == 0) {
            if (!tryEnterState(EnemyState.FLEEING)) {
                tryEnterState(EnemyState.WANDERING);
            }
            return;
        }

        if (getNearestVisibleSlime() == null) {
            tryEnterState(EnemyState.WANDERING);
            return;
        }

        _investigateLocation = getNearestVisibleSlime().transform.position;

        if (Vector3.Distance(transform.position, getNearestVisibleSlime().transform.position) > fireRange) {
            moveTowardsPointAstar(getNearestVisibleSlime().transform.position);
        } else {
            _shotCooldownLeft = timePerShot;
            _enemyAnimation.EnemyShoot(getNearestVisibleSlime().transform.position.x > transform.position.x ? 1.0f : -1.0f);  
        }
    }

    public void OnEnemyFire() {
        StartCoroutine(damageSlimeCoroutine());
    }

    protected IEnumerator damageSlimeCoroutine() {
        yield return null;
        _soundManager.PlaySound(gameObject.transform, bulletSFX);
        if (getNearestVisibleSlime(20, true) != null) {
            Instantiate(muzzleFlashEffect, shotSpawn.position, Quaternion.identity);
            Instantiate(muzzleFlashEffect, getNearestVisibleSlime().transform.position + new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, 0.4f), 0), Quaternion.identity);
            getNearestVisibleSlime().damageSlime(1.5f);
            getNearestVisibleSlime(20, true);
            bullets--;
        }
    }

    //Flee state
    protected float _startSearchingForCacheTime = 0;

    protected override bool canEnterFleeState() {
        return bullets == 0;
    }

    protected override void onExitFleeState() {
        _startSearchingForCacheTime = Time.time + 5.0f;
    }

    protected override void fleeState(){
        WeaponCache cache = _cacheSearcher.searchForClosest(transform.position);

        bool shouldSearchForCache = Time.time > _startSearchingForCacheTime && cache != null;

        if (shouldSearchForCache) {
            bool doneWithPath = pathTowardsLocation(cache.pathingWaypoint.position);

            if (doneWithPath) {
                if (isAtDestination(cache.pathingWaypoint.position)) {
                    bullets += cache.takeBullets(MAX_BULLETS);
                    if (!tryEnterState(EnemyState.INVESTIGATE)) {
                        tryEnterState(EnemyState.WANDERING);
                    }
                } else {
                    tryEnterState(EnemyState.WANDERING);
                }
            }
        } else {
            runAwayFromSlime();
        }
    }

    //Startled
    private float _startledEndTime = 0.0f;

    protected override void onEnterStartledState() {
        _startledEndTime = Time.time + 4.0f;
    }

    protected override void startledState() {
        if (Time.time > _startledEndTime) {
            tryEnterState(EnemyState.WANDERING);
        }

        runAwayFromSlime(startledSpeed);
    }

    //Investigate
    protected Vector3 _investigateLocation;

    protected override bool canEnterInvestigateState() {
        return bullets != 0;
    }

    protected override void investigateState() {
        tryEnterState(EnemyState.ATTACKING);

        bool donePathing = pathTowardsLocation(_investigateLocation);

        if (donePathing) {
            tryEnterState(EnemyState.WANDERING);
        }
    }
}
