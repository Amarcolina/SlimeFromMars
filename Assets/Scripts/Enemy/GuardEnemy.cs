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
    public int ammo = 0;
    [MinValue(0)]
    public float timePerShot = 1.5f;
    [MinValue(0)]
    public float fireRange = 8.0f;

    private GuardState _currentState;
    private float timeUntilDeath = 3f;

    //The flame shot prefab
    public GameObject shot;

    private float countDown = 2f;

    private AudioClip flameThrowerSFX;

    public override void Awake()
    {
        base.Awake();
        _currentState = startState;

        flameThrowerSFX = Resources.Load<AudioClip>("Sounds/SFX/guard_flamethrower");
    }

    void Start()
    {
        shot.GetComponent<FlameProjectile>();
        gameObject.AddComponent<SoundEffect>();
    }

    void Update()
    {
        if (isStunned())
        {
            return;
        }
        if (isOnSlimeTile())
        {
            timeUntilDeath -= Time.deltaTime;
            if (timeUntilDeath <= 0)
            {
                Destroy(gameObject);
            }
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
        if (getNearestVisibleSlime() != null && ammo != 0)
        {
            _currentState = GuardState.ATTACKING;
            return true;
        }
        return false;
    }

    private void attackState()
    {
        if (ammo == 0)
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
            }
            else
            {
                if (gameObject.GetComponent<SoundEffect>() != null)
                {
                    gameObject.GetComponent<SoundEffect>().PlaySound(flameThrowerSFX);
                }
                if (countDown >= 0)
                {
                    //use flamethrower for certain amount of time
                    countDown -= Time.deltaTime;
                    useFlameThrower();
                }
                //Reset countdown
                if(countDown <= 0)
                {
                    ammo--;
                    countDown = 5f;
                }
            }
        }
    }


    private void useFlameThrower()
    {
            Instantiate(shot, transform.position, transform.rotation);
            //Set direction of the projectile
            shot.GetComponent<FlameProjectile>().direction = getNearestVisibleSlime().transform.position - transform.position;
    }

    private bool tryEnterFleeState()
    {
        if (getNearestVisibleSlime() != null && ammo == 0)
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
