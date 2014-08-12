using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuardEnemy : SoldierEnemy {
    public Transform locationToWatch;
    [MinValue(0)]
    public float locationSize = 3;

    protected Vector3 _locationToStandGuard;
    protected Vector3 _locationToWatch;
    protected List<Vector2Int> _locationsToWatch = new List<Vector2Int>();
    protected Path _pathBackToGuardLocatin = null;

    public override void Awake() {
        base.Awake();

        _locationToStandGuard = transform.position;
        _locationToWatch = locationToWatch.position;

        Vector3 delta = locationToWatch.position - transform.position;
        Vector3 tangent = new Vector3(delta.y, -delta.x, 0).normalized;
        for (float t = -locationSize / 2.0f; t <= locationSize / 2.0f; t += 0.25f) {
            Vector2Int tileLocation = locationToWatch.position + tangent * t;
            if (!_locationsToWatch.Contains(tileLocation)) {
                _locationsToWatch.Add(tileLocation);
            }
        }
    }

    void Update() {
        if (isStunned()) {
            return;
        }

        handleStateMachine();
    }

    //Wander state
    protected override void onEnterWanderState() {
        _pathBackToGuardLocatin = Astar.findPath(transform.position, _locationToStandGuard);
    }

    protected override void wanderState() {
        bool isAtDestination = false;
        if (_pathBackToGuardLocatin == null) {
            isAtDestination = moveTowardsPointAstar(_locationToStandGuard);
        } else {
            isAtDestination = followPath(_pathBackToGuardLocatin);
        }

        if (isAtDestination) {
            _enemyAnimation.Flip(_locationToStandGuard.x - transform.position.x > 0.0f ? 1.0f : -1.0f);
        }

        tryEnterState(EnemyState.ATTACKING);
        tryEnterState(EnemyState.FLEEING);
    }

    //Attack state
    protected override bool canEnterAttackState() {
        if (isAtPost()) {
            return getSlimeInVisionCone() != null && bullets != 0;
        } else {
            return getNearestVisibleSlime() != null && bullets != 0;
        }
    }

    //Flee state
    protected override bool canEnterFleeState() {
        if (isAtPost()) {
            return getSlimeInVisionCone() != null && bullets == 0;
        } else {
            return getNearestVisibleSlime() != null && bullets == 0;
        }
    }

    //Functions

    protected bool isAtPost() {
        Vector2Int pos0 = transform.position;
        Vector2Int pos1 = _locationToStandGuard;
        return pos0 == pos1;
    }

    protected override Slime getNearestVisibleSlime(int maxTileDistance = 20, bool forceUpdate = false) {
        Slime s = getSlimeInVisionCone();
        if (s == null) {
            s = base.getNearestVisibleSlime(maxTileDistance, forceUpdate);
        }
        return s;
    }

    protected Slime getSlimeInVisionCone() {
        foreach (Vector2Int pos in _locationsToWatch) {
            TileRayHit hit = TilemapUtilities.castTileRay(transform.position, pos, BaseEnemy.tileRayHitSlime);

            if (hit.didHit) {
                GameObject hitObj = _tilemap.getTileGameObject(hit.hitPosition);
                if (hitObj != null) {
                    Slime slime = hitObj.GetComponent<Slime>();
                    if (slime != null) {
                        return slime;
                    }
                }
            }
        }

        return null;
    }

    public override void OnDrawGizmos() {
        base.OnDrawGizmos();

        Gizmos.color = Color.green;

        Vector3 watch;
        if (Application.isPlaying) {
            watch = _locationToWatch;
        } else {
            watch = locationToWatch.position;
        }

        Vector3 delta = watch - transform.position;
        Vector3 tangent = new Vector3(delta.y, -delta.x, 0).normalized;
        Vector3 pos0 = watch + tangent * locationSize / 2.0f;
        Vector3 pos1 = watch - tangent * locationSize / 2.0f;
        Gizmos.DrawLine(transform.position, pos0);
        Gizmos.DrawLine(transform.position, pos1);
        Gizmos.DrawLine(pos1, pos0);
    }
}
