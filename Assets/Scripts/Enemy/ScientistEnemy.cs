using UnityEngine;
using System.Collections;

public enum ScientistState {
    WANDERING,
    FLEEING,
    HIDING
}


public class ScientistEnemy : BaseEnemy {
    public const float CHECK_RADIUS_FOR_HIDING_SPOTS = 25.0f;
    public const float MAX_DISTANCE_TO_HIDING_SPOT_SQRD = 8.0f * 8.0f;

    public ScientistState startState = ScientistState.WANDERING;

    public float wanderSpeed = 2.5f;
    public float fleeSpeed = 3.5f;

    private ScientistState _currentState;
    private delegate void StateFunction();
    private StateFunction _currentExitFunction = null;
    private StateFunction _currentStateFunction = null;

    private static ScientistHidingSpot[] _hidingSpots = null;
    private ScientistHidingSpot _currentHidingSpot = null;
    private ProximitySearcher<ScientistHidingSpot> _hidingSpotSearcher = null;
    private Path _pathToHidingSpot = null;
    private bool _leavingHidingSpot = false;

    private AudioClip screamSFX;
    //private AudioClip walkSFX;
    //private AudioClip deathSFX;

    public override void Awake() {
        base.Awake();
        if (_hidingSpots == null) {
            _hidingSpots = FindObjectsOfType<ScientistHidingSpot>();
        }
        _hidingSpotSearcher = new ProximitySearcher<ScientistHidingSpot>(_hidingSpots, CHECK_RADIUS_FOR_HIDING_SPOTS);

        changeState(startState);

        screamSFX = Resources.Load<AudioClip>("Sounds/SFX/scientist_scream");
        //walkSFX = Resources.Load<AudioClip>("Sounds/SFX/scientist_footsteps");
        //deathSFX = Resources.Load<AudioClip>("Sounds/SFX/scientist_death");
    }

    void Start()
    {
        sound = SoundManager.getInstance();
    }

    void Update() {
        if (isStunned()) {
            return;
        }

        _currentStateFunction();
    }

    private void changeState(ScientistState newState) {
        if (_currentExitFunction != null) {
            _currentExitFunction();
            _currentExitFunction = null;
        }

        _currentState = newState;
        switch (_currentState) {
            case ScientistState.WANDERING:
                //gameObject.AddComponent<SoundEffect>().sfx = walkSFX;
                //gameObject.GetComponent<SoundEffect>().loop = true;
                _currentStateFunction = wanderState;
                break;
            case ScientistState.FLEEING:
                sound.PlaySound(gameObject.transform, screamSFX);

                _currentStateFunction = fleeState;
                break;
            case ScientistState.HIDING:
                _currentStateFunction = hideState;
                _currentExitFunction = exitHideState;
                break;
            default:
                Debug.LogWarning("Cannot handle state " + _currentState);
                break;
        }
    }

    private void enterWanderState() {
        recalculateMovementPatternPath();
        changeState(ScientistState.WANDERING);
    }

    private void wanderState() {
        followMovementPattern(wanderSpeed);
        tryEnterFleeState();
    }

    private bool tryEnterFleeState(int searchDistance = 20) {
        if (getNearestVisibleSlime(searchDistance) != null) {
            changeState(ScientistState.FLEEING);
            return true;
        }
        return false;
    }

    private void fleeState() {
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
            enterWanderState();
        }
        runAwayFromSlime(fleeSpeed);
        tryEnterHidingState();
    }

    private bool tryEnterHidingState() {
        ScientistHidingSpot spot = _hidingSpotSearcher.searchForClosest(transform.position, 9, 1);
        if (getNearestVisibleSlime(1) != null) {
            return false;
        }

        if ((transform.position - spot.transform.position).sqrMagnitude < MAX_DISTANCE_TO_HIDING_SPOT_SQRD){
            if(spot.tryToClaim()){
                _currentHidingSpot = spot;
                _leavingHidingSpot = false;
                _pathToHidingSpot = Astar.findPath(transform.position, spot.enterLocation.position);
                changeState(ScientistState.HIDING);
                return true;
            }        
        }
        return false;
    }

    private void hideState() {
        if (_pathToHidingSpot != null) {
            if (followPath(_pathToHidingSpot, fleeSpeed)) {
                _pathToHidingSpot = null;
            }
        } else if (_leavingHidingSpot) {
            if(moveTowardsPoint(_currentHidingSpot.enterLocation.position, fleeSpeed)){
                tryEnterFleeState();
            }
        }else{
            if(moveTowardsPoint(_currentHidingSpot.transform.position, fleeSpeed)){
                if (getNearestVisibleSlime(1) != null) {
                    _leavingHidingSpot = true;
                }
            }
        }
    }

    private void exitHideState() {
        if (_currentHidingSpot != null) {
            _currentHidingSpot.release();
        }
    }
}
