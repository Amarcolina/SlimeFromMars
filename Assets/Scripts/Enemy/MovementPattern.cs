using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PatternType {
    NORMAL,
    PING_PONG
}

[System.Serializable]
public class MovementPatternInfo {
    public GameObject waypoint = null;
    [MinValue(0)]
    public int repeatCount = 0;
}

public class MovementPattern : MonoBehaviour {
    public List<MovementPatternInfo> movementPatterns = new List<MovementPatternInfo>();
    public PatternType loopAction = PatternType.NORMAL;

    private int _waypointsContained = int.MaxValue;

    public void Awake() {
        if (_waypointsContained != int.MaxValue) {
            int index = int.MaxValue;
            getWaypointInternal(ref index);
        }
    }

    private Waypoint getWaypointInternal(ref int index) {
        int patternIndex = 0;
        int patternDirection = 1;

        int startIndex = index;

        while ((loopAction == PatternType.NORMAL && patternIndex < movementPatterns.Count) ||
               (loopAction == PatternType.PING_PONG && patternIndex != 0 && patternDirection != -1)) {
            MovementPatternInfo info = movementPatterns[patternIndex];

            Waypoint waypoint = info.waypoint.GetComponent<Waypoint>();
            MovementPattern pattern = info.waypoint.GetComponent<MovementPattern>();

            for (int i = 0; i <= info.repeatCount; i++) {
                if (waypoint != null) {
                    if (index == 0) {
                        return waypoint;
                    }
                    index--;
                } else if (pattern != null){
                    if (i < pattern._waypointsContained || pattern._waypointsContained == int.MaxValue) {
                        Waypoint potentialWaypoint = pattern.getWaypointInternal(ref index);
                        if (potentialWaypoint != null) {
                            return potentialWaypoint;
                        }
                    } else {
                        i -= pattern._waypointsContained;
                    }
                }
            }

            patternIndex += patternDirection;
            if (patternIndex == movementPatterns.Count - 1) {
                patternDirection = -1;
            }
        }

        if (_waypointsContained == int.MaxValue) {
            _waypointsContained = startIndex - index;
        }

        return null;
    }

    public Waypoint this[int i] {
        get {
            if (_waypointsContained == int.MaxValue) {
                throw new System.Exception("Cannot use MovementPattern[] until it's Awake has finished");
            }
            i = i % _waypointsContained;
            return getWaypointInternal(ref i);
        }
    }
}


