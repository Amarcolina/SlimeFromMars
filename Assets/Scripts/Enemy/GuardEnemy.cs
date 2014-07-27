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
    private Slime _nearestSlime = null;

    public GameObject shot;
    public Transform shotSpawn;

    public Vector2 speed;
    public Vector2 direction;
    
 
    public override void Awake()
    {
        base.Awake();
        _currentState = startState;

        _tilemap = Tilemap.getInstance();
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
                    _timeUntilNextShot += timePerShot;

                    useFlameThrower();
                    bullets--;
                }
            }
        }
    }

    private void useFlameThrower()
    {
        Transform shotTransform = Instantiate(shot, transform.position, transform.rotation) as Transform;
        Instantiate(shot, transform.position, transform.rotation);
        Instantiate(shot, transform.position, transform.rotation);
        Instantiate(shot, transform.position, transform.rotation);
        Instantiate(shot, transform.position, transform.rotation); 
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
