using UnityEngine;
using System.Collections;

public enum WaypointAction {
    WAIT,
    EXAMINE,
    ACTION
}

public class MovementWaypoint : MovementPattern {
    public float duration = 1.0f;
    public WaypointAction waypointAction;
}
