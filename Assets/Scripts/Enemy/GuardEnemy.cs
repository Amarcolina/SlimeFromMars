using UnityEngine;
using System.Collections;

public class GuardEnemy : BaseEnemy {

    public enum GuardState
    {
        WANDERING,
        CHASING,
        SHOOTING
    }

    public GuardState startState = GuardState.WANDERING;


    public float wanderSpeed = 2.5f;

    private GuardState _currentState;

    public override void Awake()
    {
        base.Awake();
        _currentState = startState;
    }

    void Update()
    {
        if (isOnSlimeTile())
        {
            Destroy(this.gameObject);
            return;
        }

        switch (_currentState)
        {
            case GuardState.WANDERING:
                wanderState();
                break;
            case GuardState.CHASING:
                chaseState();
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
    }

    private bool enterChaseState()
    {
        return false;
    }

    private void chaseState()
    {

    }
}
