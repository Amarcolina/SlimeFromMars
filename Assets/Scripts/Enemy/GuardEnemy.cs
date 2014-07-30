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
    public float fireRange = 4.0f;

    const float FLAME_SPEED = 1050f;

    private float _timeUntilNextShot = 0.0f;
    private GuardState _currentState;
    private Slime _nearestSlime = null;

    //The flame shot prefab
    public GameObject shot;
    //Set # of flames in the shot
    public float numFlames;

    private EnemyProjectile proj;



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

        _nearestSlime = getNearestVisibleSlime();

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
        if (_nearestSlime != null && bullets != 0)
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
                    shot.GetComponent<EnemyProjectile>().direction = getBulletDirection();
                    _timeUntilNextShot += timePerShot;
                    useFlameThrower();
                    bullets--;
                }
            }
        }
    }

    public Vector2 getBulletDirection()
    {
        Vector2 direction =_nearestSlime.transform.position - transform.position;

        return direction;
    }

    private void useFlameThrower()
    {
            

        //        for (int i = 0; i < 2; i++)
        //        {
        //Vector2 direction = Vector2.zero;
        //direction = _nearestSlime.transform.position - transform.position;
        //direction.Normalize();
        //direction += new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
        //direction = direction * Random.Range(FLAME_SPEED, FLAME_SPEED);
            Instantiate(shot, transform.position, transform.rotation);
                //}
            
    }

    private bool tryEnterFleeState()
    {
        if (_nearestSlime != null && bullets == 0)
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
