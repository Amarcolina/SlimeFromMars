using UnityEngine;
using System.Collections;

public class AstarEnemy : BaseEnemy {
    public Transform goal;

    public void Update() {
        moveTowardsPointAstar(goal.transform.position);
    }
}
