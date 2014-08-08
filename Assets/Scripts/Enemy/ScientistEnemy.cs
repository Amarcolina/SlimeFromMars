using UnityEngine;
using System.Collections;

public class ScientistEnemy : BaseEnemy {
    public const float CHECK_RADIUS_FOR_HIDING_SPOTS = 25.0f;
    public const float MAX_DISTANCE_TO_HIDING_SPOT_SQRD = 8.0f * 8.0f;

    public float wanderSpeed = 2.5f;
    public float fleeSpeed = 3.5f;

    private static ScientistHidingSpot[] _hidingSpots = null;
    private ScientistHidingSpot _currentHidingSpot = null;
    private ProximitySearcher<ScientistHidingSpot> _hidingSpotSearcher = null;
    private Path _pathToHidingSpot = null;
    private bool _leavingHidingSpot = false;

    private AudioClip screamSFX;

    public override void Awake() {
        base.Awake();
        if (_hidingSpots == null) {
            _hidingSpots = FindObjectsOfType<ScientistHidingSpot>();
        }
        _hidingSpotSearcher = new ProximitySearcher<ScientistHidingSpot>(_hidingSpots, CHECK_RADIUS_FOR_HIDING_SPOTS);

        tryEnterState(startState);

        sound = SoundManager.getInstance();
        screamSFX = Resources.Load<AudioClip>("Sounds/SFX/scientist_scream");
        //walkSFX = Resources.Load<AudioClip>("Sounds/SFX/scientist_footsteps");
        //deathSFX = Resources.Load<AudioClip>("Sounds/SFX/scientist_death");
    }

    void Update() {
        if (isStunned()) {
            return;
        }

        handleStateMachine();
    }

    //Wander state
    protected override void onEnterWanderState() {
        recalculateMovementPatternPath();
    }

    protected override void wanderState() {
        followMovementPattern(wanderSpeed);
        tryEnterState(EnemyState.FLEEING);
    }

    //Flee state
    protected override bool canEnterFleeState() {
        return getNearestVisibleSlime(20) != null;
    }

    protected override void onEnterFleeState() {
        AudioSource.PlayClipAtPoint(screamSFX, transform.position);
    }

    protected override void fleeState() {
        if (Time.time - getLastTimeViewedSlime() > 15.0f) {
            movementPattern = null;
            float closestDistance = float.MaxValue;
            foreach (MovementPattern pattern in MovementPattern.getAllMovementPatterns()) {
                if (pattern.isRecursive()) {
                    continue;
                }
                float distance = Vector3.Distance(transform.position, pattern.transform.position);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    movementPattern = pattern;
                }
            }
            tryEnterState(EnemyState.WANDERING);
        }
        runAwayFromSlime(fleeSpeed);
        tryEnterState(EnemyState.HIDING);
    }

    //Hide state
    protected override bool canEnterHideState(){
        ScientistHidingSpot spot = _hidingSpotSearcher.searchForClosest(transform.position, 9, 1);
        if (getNearestVisibleSlime(1) != null) {
            return false;
        }

        if ((transform.position - spot.transform.position).sqrMagnitude < MAX_DISTANCE_TO_HIDING_SPOT_SQRD) {
            if (spot.tryToClaim()) {
                _currentHidingSpot = spot;
                _leavingHidingSpot = false;
                _pathToHidingSpot = Astar.findPath(transform.position, spot.enterLocation.position);
                return true;
            }
        }
        return false;
    }

    protected override void hideState() {
        if (_pathToHidingSpot != null) {
            if (followPath(_pathToHidingSpot, fleeSpeed)) {
                _pathToHidingSpot = null;
            }
        } else if (_leavingHidingSpot) {
            if(moveTowardsPoint(_currentHidingSpot.enterLocation.position, fleeSpeed)){
                tryEnterState(EnemyState.WANDERING);
            }
        }else{
            if(moveTowardsPoint(_currentHidingSpot.transform.position, fleeSpeed)){
                if (getNearestVisibleSlime(1) != null) {
                    _leavingHidingSpot = true;
                }
            }
        }
    }

    protected override void onExitHideState() {
        if (_currentHidingSpot != null) {
            _currentHidingSpot.release();
        }
    }
}
