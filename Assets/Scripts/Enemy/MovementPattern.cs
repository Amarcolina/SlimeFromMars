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
        if (_waypointsContained == int.MaxValue) {
            int index = int.MaxValue;
            getWaypointInternal(ref index);
        }
    }

    public bool isRecursive() {
        foreach (MovementPatternInfo info in movementPatterns) {
            if (info.waypoint.GetComponent<MovementPattern>() != null) {
                return true;
            }
        }
        return false;
    }

    /* This array accessor is used to grab a given Waypoint.  For example, passing in
     * 0 will access the first waypoint, 1 will access the second and so on.  This 
     * method handles all of the recursion and nesting needed, as well as the loop
     * actions.  It also loops, so there is no need to ever reset your index back to
     * 0, you can simply keep incrementing it to get the next waypoint.
     */
    public Waypoint this[int i] {
        get {
            if (_waypointsContained == int.MaxValue) {
                throw new System.Exception("Cannot use MovementPattern[] until it's Awake has finished");
            }
            i = i % _waypointsContained;
            return getWaypointInternal(ref i);
        }
    }

    private Waypoint getWaypointInternal(ref int index) {
        int patternIndex = 0;
        int patternDirection = 1;

        int startIndex = index;

        while ((loopAction == PatternType.NORMAL && patternIndex < movementPatterns.Count) ||
               (loopAction == PatternType.PING_PONG && (patternIndex != 0 || patternDirection != -1))) {
            MovementPatternInfo info = movementPatterns[patternIndex];

            Waypoint waypoint = info.waypoint.GetComponent<Waypoint>();
            MovementPattern pattern = info.waypoint.GetComponent<MovementPattern>();

            if (waypoint == null && pattern == null) {
                Debug.LogError("The gameObject " + info.waypoint + " has neither a Waypoint nor a MovementPattern component\n" +
                               "It must have one or the other to be part of a MovementPattern");
                return null;
            }

            if (waypoint != null && pattern != null) {
                Debug.LogError("The gameObject " + info.waypoint + " has both a Waypoint and a MovementPattern component\n" +
                               "Only one or the other can be on a given game object");
                return null;
            }

            for (int i = 0; i <= info.repeatCount; i++) {
                if (waypoint != null) {
                    if (index == 0) {
                        return waypoint;
                    }
                    index--;
                } else if (pattern != null){
                    if (index < pattern._waypointsContained || pattern._waypointsContained == int.MaxValue) {
                        Waypoint potentialWaypoint = pattern.getWaypointInternal(ref index);
                        if (potentialWaypoint != null) {
                            return potentialWaypoint;
                        }
                    } else {
                        index -= pattern._waypointsContained;
                    }
                }
                //
            }

            patternIndex += patternDirection;
            if (patternIndex == movementPatterns.Count - 1 && loopAction == PatternType.PING_PONG) {
                patternDirection = -1;
            }
        }

        if (_waypointsContained == int.MaxValue) {
            _waypointsContained = startIndex - index;
        }

        return null;
    }

    public void OnDrawGizmosSelected() {
        if (_waypointsContained != int.MaxValue) {
            Gizmos.color = Color.blue;
            for (int i = 0; i < _waypointsContained; i++) {
                Waypoint waypoint0 = this[i];
                Waypoint waypoint1 = this[i + 1];
                Gizmos.DrawLine(waypoint0.transform.position, waypoint1.transform.position);
            }
        }
    }
}


