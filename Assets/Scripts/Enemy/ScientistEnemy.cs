using UnityEngine;
using System.Collections;

public enum ScientistState {
    WANDERING,
    FLEEING
}

public class ScientistEnemy : BaseEnemy {
    public ScientistState startState = ScientistState.WANDERING;

    public float wanderSpeed = 2.5f;
    public float fleeSpeed = 3.5f;

    private ScientistState _currentState;

    public override void Awake() {
        base.Awake();
        _currentState = startState;
    }

    void Update() {
        if (isOnSlimeTile()) {
            Destroy(this.gameObject);
            return;
        }

        switch (_currentState) {
            case ScientistState.WANDERING:
                wanderState();
                break;
            case ScientistState.FLEEING:
                fleeState();
                break;
        }
    }

    private void enterWanderState() {
        recalculateMovementPatternPath();
        _currentState = ScientistState.WANDERING;
    }

    private void wanderState() {
        followMovementPattern(wanderSpeed);
    }

    private bool enterFleeState() {
        return false;
    }

    private void fleeState() {

    }
}
