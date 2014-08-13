using UnityEngine;
using System.Collections;

public class SoldierEnemy : BaseEnemy {
    public float wanderSpeed = 2.5f;
    public float fleeSpeed = 3.5f;
    public float startledSpeed = 4.5f;

    [MinValue (0)]
    public int bullets = 20;
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
        } else {
            if (getNearestVisibleSlime() == null) {
                tryEnterState(EnemyState.WANDERING);
                return;
            }

            if (Vector3.Distance(transform.position, getNearestVisibleSlime().transform.position) > fireRange) {
                moveTowardsPointAstar(getNearestVisibleSlime().transform.position);
            } else {
                _shotCooldownLeft = timePerShot;
                _enemyAnimation.EnemyShoot(getNearestVisibleSlime().transform.position.x > transform.position.x ? 1.0f : -1.0f);  
            }
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
    protected WeaponCache _currentCacheDestination = null;
    protected Path _pathToWeaponCache = null;
    protected bool _isCalculatingWeaponCachePath = false;

    protected override bool canEnterFleeState() {
        return bullets == 0;
    }

    protected override void onEnterFleeState() {
        _pathToWeaponCache = null;
        _currentCacheDestination = null;
        StartCoroutine(calculateWeaponCachePath(60));
    }

    protected override void fleeState(){
        if(!_isCalculatingWeaponCachePath){
            WeaponCache newCache = _cacheSearcher.searchForClosest(transform.position);
            if (newCache != _currentCacheDestination) {
                StartCoroutine(calculateWeaponCachePath(0));
            }
        }

        if (_pathToWeaponCache != null) {
            if (followPath(_pathToWeaponCache)) {
                bullets += 1;
                _investigateLocation = _lastSlimeViewed.transform.position;
                if (!tryEnterState(EnemyState.INVESTIGATE)) {
                    tryEnterState(EnemyState.WANDERING);
                }
            }
        } else {
            runAwayFromSlime();
        }
    }
    
    private IEnumerator calculateWeaponCachePath(int preSearchAmount) {
        _isCalculatingWeaponCachePath = true;

        //Search for the closest for a an amount of time before starting Astar
        for (int i = 0; i < preSearchAmount; i++) {
            _currentCacheDestination = _cacheSearcher.searchForClosest(transform.position);
            yield return null;
        }

        Path newPath = new Path();
        AstarSettings settings = new AstarSettings();
        settings.maxNodesToCheck = 1;

        yield return StartCoroutine(Astar.findPathCoroutine(newPath, transform.position, _currentCacheDestination.transform.position, settings));

        _pathToWeaponCache = newPath.Count == 0 ? null : newPath;
        _isCalculatingWeaponCachePath = false;
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
    protected Path _investigatePath = null;
    protected bool _isCalculatingInvestigatePath = false;
    protected Vector3 _investigateLocation;

    protected override bool canEnterInvestigateState() {
        return bullets != 0;
    }

    protected override void onEnterInvestigateState() {
        _investigatePath = null;
        _isCalculatingInvestigatePath = false;
        StartCoroutine(calculateInvestigatePath());
    }

    protected override void investigateState() {
        if (_isCalculatingInvestigatePath) {
            moveTowardsPointAstar(_lastSlimeViewed.transform.position);
        } else {
            if (_investigatePath == null) {
                tryEnterState(EnemyState.WANDERING);
            } else {
                if (followPath(_investigatePath)) {
                    tryEnterState(EnemyState.WANDERING);
                }
                tryEnterState(EnemyState.ATTACKING);
            }
        }
    }

    private IEnumerator calculateInvestigatePath() {
        _isCalculatingInvestigatePath = true;

        Path newPath = new Path();
        AstarSettings settings = new AstarSettings();
        settings.maxNodesToCheck = 1;

        yield return StartCoroutine(Astar.findPathCoroutine(newPath, transform.position, _investigateLocation, settings));

        _investigatePath = newPath.Count == 0 ? null : newPath;
        _investigatePath.truncateBegining();

        _isCalculatingInvestigatePath = false;
    }
}
