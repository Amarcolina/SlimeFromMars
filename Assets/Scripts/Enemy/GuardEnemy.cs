using UnityEngine;
using System.Collections;

public enum GuardState
{
    WANDERING,
    ATTACKING,
    FLEEING
}

public class GuardEnemy : BaseEnemy
{
    public GuardState startState = GuardState.WANDERING;

    public float wanderSpeed = 2.5f;
    public float fleeSpeed = 3.5f;

    [MinValue(0)]
    public int bullets = 20;
    [MinValue(0)]
    public float timePerShot = 1.5f;
    [MinValue(0)]
    public float fireRange = 8.0f;

    private float _timeUntilNextShot = 0.0f;
    private GuardState _currentState;

    //The flame shot prefab
    public GameObject shot;
    public int numFlames;

    private float timer;

    void Start()
    {
        shot.GetComponent<EnemyProjectile>();
    }

    void Update()
    {
        if (isStunned())
        {
            return;
        }
        
        switch (_currentState)
        {
            case GuardState.WANDERING:
                wanderState();
                break;
            case GuardState.ATTACKING:
                attackState();
                break;
            case GuardState.FLEEING:
                fleeState();
                break;
            default:
                Debug.LogWarning("Cannot handle state " + _currentState);
                break;
        }
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
        tryEnterFleeState();
    }

    private bool tryEnterAttackState()
    {
        if (getNearestVisibleSlime() != null && bullets != 0)
        {
            _currentState = GuardState.ATTACKING;
            return true;
        }
        return false;
    }

    private void attackState()
    {
        if (bullets == 0)
        {
            if (tryEnterFleeState())
            {
                enterWanderState();
            }
        }
        else
        {
            if (getNearestVisibleSlime() == null)
            {
                enterWanderState();
                return;
            }

            if (Vector3.Distance(transform.position, getNearestVisibleSlime().transform.position) > fireRange)
            {
                moveTowardsPoint(getNearestVisibleSlime().transform.position);
                _timeUntilNextShot = timePerShot;
            }
            else
            {
                useFlameThrower();
            }
        }
    }


    private void useFlameThrower()
    {
            Instantiate(shot, transform.position, transform.rotation);
            //Set direction of the projectile
            shot.GetComponent<EnemyProjectile>().direction = getNearestVisibleSlime().transform.position - transform.position;
    }

    private bool tryEnterFleeState()
    {
        if (getNearestVisibleSlime() != null && bullets == 0)
        {
            _currentState = GuardState.FLEEING;
            return true;
        }
        return false;
    }

    private void fleeState()
    {
        runAwayFromSlime(fleeSpeed);
    }
}
