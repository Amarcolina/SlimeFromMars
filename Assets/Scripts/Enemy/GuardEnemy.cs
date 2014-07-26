using UnityEngine;
using System.Collections;

public class GuardEnemy : BaseEnemy {

    public enum GuardState
    {
        WANDERING,
        ATTACKING
    }

    [MinValue(0)]
    public int bullets = 20;
    [MinValue(0)]
    public float timePerShot = 4f;
    [MinValue(0)]
    public float fireRange = 8.0f;

    private Slime _nearestSlime = null;

    public GuardState startState = GuardState.WANDERING;

    private float _timeUntilNextShot = 0.0f;
    public float wanderSpeed = 2.5f;

    private GuardState _currentState;
    private int onSlimeDuration;
    private GameObject projectile;

    public override void Awake()
    {
        base.Awake();
        _currentState = startState;
        projectile.AddComponent<Rigidbody>();

    }

    void Update()
    {
        if (isStunned())
        {
            return;
        }
        if (isOnSlimeTile())
        {
            onSlimeDuration++;
            return;
        }
        if (onSlimeDuration > 5)
        {
            Destroy(this.gameObject);
        }

        switch (_currentState)
        {
            case GuardState.WANDERING:
                wanderState();
                break;
            case GuardState.ATTACKING:
                attackState();
                break;
        }
    }

    private void attackState()
    {

            if (_nearestSlime == null)
            {
                enterWanderState();
                return;
            }

            if (Vector3.Distance(transform.position, _nearestSlime.transform.position) > fireRange)
            {
                moveTowardsPoint(_nearestSlime.transform.position);
                _timeUntilNextShot = timePerShot;
            }
            else
            {
                _timeUntilNextShot -= Time.deltaTime;
                if (_timeUntilNextShot <= 0.0f)
                {
                    _timeUntilNextShot += timePerShot;
                   _nearestSlime.damageSlime(3f);
                    bullets--;
                }
            }
        
    }

    private void coneAttack()
    {
        GameObject gameobject = _tilemap.getTileGameObject(transform.position);
        float angle =  Vector3.Angle(_nearestSlime.transform.position, transform.position);
        
    }

    private void enterWanderState()
    {
        recalculateMovementPatternPath();
        _currentState = GuardState.WANDERING;
    }

    private void wanderState()
    {
        followMovementPattern(wanderSpeed);
        tryEnterAttackState();
    }
    private bool tryEnterAttackState()
    {
        if (_nearestSlime != null && bullets != 0)
        {
            _currentState = GuardState.ATTACKING;
            return true;
        }
        return false;
    }
}
