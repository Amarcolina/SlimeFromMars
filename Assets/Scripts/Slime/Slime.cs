using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Slime : MonoBehaviour {
    public const float OPACITY_CHANGE_SPEED = 1.0f;
    public const float HEALTH_REGEN_RATE = 0.1f;
    public const float TIME_PER_EXPAND = 0.1f;

    private Tile _myTile;
    private Tilemap _tilemap;

    private bool _isSolid = false;
    private float _percentHealth = 1.0f;
    private int _slimeNeighbors = 0;

    private float currentOpacity = 0.0f;

    private Path _currentExpandPath = null;
    private float _timeUntilExpand = 0.0f;

    public void Awake() {
        _tilemap = Tilemap.getInstance();
        _myTile = GetComponent<Tile>();
        updateNeighbors(0);
    }

    public void OnDestroy() {
        updateNeighbors(_isSolid ? -1 : 0);
    }

    public void forceSlimeUpdate(int modifyNeighborCount = 0) {
        enabled = true;
        _slimeNeighbors += modifyNeighborCount;
    }

    [ContextMenu ("Toggle Solidity")]
    public void toggleSolid() {
        setSolid(!_isSolid);
    }

    public void setSolid(bool isSolid) {
        if (_isSolid != isSolid) {
            _isSolid = isSolid;
            forceSlimeUpdate();
            updateNeighbors(_isSolid ? 1 : -1);

            if (_isSolid) {
                List<Tile> neighbors = _tilemap.getNeighboringTiles(transform.position);
                foreach (Tile tile in neighbors) {
                    if (tile.GetComponent<Slime>() == null) {
                        tile.gameObject.AddComponent<Slime>();
                    }
                }
            }
        }
    }

    public void Update() {
        bool canGoToSleep = true;

        if (_currentExpandPath != null) {
            _timeUntilExpand -= Time.deltaTime;
            if (_timeUntilExpand <= 0.0f) {
                expandSlime();
            }
            canGoToSleep = false;
        }

        float goalOpacity = getGoalOpacity();
        if (currentOpacity != goalOpacity) {
            currentOpacity = Mathf.MoveTowards(currentOpacity, goalOpacity, OPACITY_CHANGE_SPEED * Time.deltaTime);
            if (currentOpacity <= 0.0f) {
                destroySlime();
            }
            canGoToSleep = false;
        }

        if (_percentHealth != 1.0f) {
            _percentHealth = Mathf.MoveTowards(_percentHealth, 1.0f, HEALTH_REGEN_RATE * Time.deltaTime);
            canGoToSleep = false;
        }

        if (canGoToSleep) {
            enabled = false;
        }
    }

    public void damageSlime(float percentDamage) {
        _percentHealth -= percentDamage;
    }

    public void destroySlime() {
        Destroy(this);
    }

    [ContextMenu ("Request example path")]
    public void requestExamplePath() {
        Path path = new Path();
        Vector2Int myPos = Tilemap.getTilemapLocation(transform.position);
        path.addNodeToEnd(myPos + Vector2Int.right);
        path.addNodeToEnd(myPos + Vector2Int.right*2);
        path.addNodeToEnd(myPos + Vector2Int.right*3);
        path.addNodeToEnd(myPos + Vector2Int.right*4);
        path.addNodeToEnd(myPos + Vector2Int.right*5);
        path.addNodeToEnd(myPos + Vector2Int.right*6);
        requestExpansionAllongPath(path);
    }

    public void requestExpansionAllongPath(Path path, float timeUntilExpand = 0.0f) {
        forceSlimeUpdate();
        _currentExpandPath = path;
        _timeUntilExpand = timeUntilExpand;
        if (_timeUntilExpand <= 0.0f) {
            expandSlime();
        }
    }

    public void expandSlime() {
        Vector2Int nextNode = _currentExpandPath.getNext();

        Tile newSlimeTile = _tilemap.getTile(nextNode);
        if (newSlimeTile && newSlimeTile.isWalkable) {
            Slime newSlime = newSlimeTile.GetComponent<Slime>();
            if(newSlime == null){
                newSlime = newSlimeTile.gameObject.AddComponent<Slime>();
            }
            newSlime.setSolid(true);
            if (_currentExpandPath.hasNext()) {
                newSlime.requestExpansionAllongPath(_currentExpandPath, _timeUntilExpand + TIME_PER_EXPAND);
            }
        }

        _currentExpandPath = null;
    }

    private float getGoalOpacity() {
        float opacity = _percentHealth;
        if (!_isSolid) {
            opacity *= _slimeNeighbors / 8.0f;
        }
        return opacity;
    }

    private void updateNeighbors(int modifyNeighborCount) {
        List<Tile> neighbors = _tilemap.getNeighboringTiles(transform.position);
        _slimeNeighbors = 0;
        foreach (Tile tile in neighbors) {
            Slime slime = tile.GetComponent<Slime>();
            if (slime != null) {
                if (slime._isSolid) {
                    _slimeNeighbors++;
                }
                slime.forceSlimeUpdate(modifyNeighborCount);
            }
        }
    }

    public void OnDrawGizmos() {
        Gizmos.color = new Color(1.0f, 0.0f, 1.0f, currentOpacity);
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
}
