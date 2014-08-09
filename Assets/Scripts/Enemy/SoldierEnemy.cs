﻿using UnityEngine;
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
    protected override bool canEnterFleeState() {
        return getNearestVisibleSlime() != null && bullets == 0;
    }

    protected override void fleeState(){
        runAwayFromSlime(fleeSpeed);
    }

    //Startled
    private float _startledEndTime = 0.0f;

    protected override void onEnterStartledState() {
        _startledEndTime = Time.time + 2.0f;
    }

    protected override void startledState() {
        if (Time.time > _startledEndTime) {
            tryEnterState(EnemyState.WANDERING);
        }

        runAwayFromSlime(startledSpeed);
    }
}
