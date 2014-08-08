using UnityEngine;
using System.Collections;

public class AstarEnemy : BaseEnemy {
    public Transform goal;

    public void Update() {
        moveTowardsPointAstar(goal.transform.position);
    }

    public void OnDrawGizmos() {
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.3f);
        foreach (Vector2Int pos in Astar.checkedNodesList) {
            Gizmos.DrawCube(pos, Vector3.one);
        }
    }
}
