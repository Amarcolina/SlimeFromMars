using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PatternEndAction {
    PING_PONG,
    LOOP
}

[System.Serializable]
public class MovementPatternInfo{
    public MovementPattern pattern = null;
    [MinValue (0)]
    public int repeatCount = 0;
}

public class MovementPattern : MonoBehaviour {
    public List<MovementPatternInfo> movementPatterns = new List<MovementPatternInfo>();
    public PatternEndAction loopAction = PatternEndAction.LOOP;

    private int _patternIndex = 0;

    public virtual MovementWaypoint getNextWaypoint() {
        MovementWaypoint waypoint = movementPatterns[_patternIndex].;



        return waypoint;
    }
}
