using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Slime : MonoBehaviour {
    public const float OPACITY_CHANGE_SPEED = 1.0f;
    public const float HEALTH_REGEN_RATE = 0.1f;
    public const float TIME_PER_EXPAND = 1.0f;

    private static Sprite[] _slimeSpriteLookup = null;
    private static int[] _slimeSpriteAngleLookup = { 0, 0, 90, 0, 180, 0, 90, 0, 270, 270, 180, 270, 180,  180, 90, 0 };

    private Tile _myTile;
    private Tilemap _tilemap;
    private SpriteRenderer _slimeRenderer;

    private bool _isSolid = false;
    private float _percentHealth = 1.0f;
    private int _slimeNeighbors = 0;

    private float currentOpacity = 0.0f;

    private Path _currentExpandPath = null;
    private float _timeUntilExpand = 0.0f;

    private void initSlimeSprites() {
        Sprite slimeSprite0x1 = Resources.Load<Sprite>("Sprites/Slime/Slime-0x1");
        Sprite slimeSprite0x3 = Resources.Load<Sprite>("Sprites/Slime/Slime-0x3");
        Sprite slimeSprite0x5 = Resources.Load<Sprite>("Sprites/Slime/Slime-0x5");
        Sprite slimeSprite0x7 = Resources.Load<Sprite>("Sprites/Slime/Slime-0x7");
        Sprite slimeSprite0xF = Resources.Load<Sprite>("Sprites/Slime/Slime-0xF");
        _slimeSpriteLookup = new Sprite[16];
        _slimeSpriteLookup[0x1] = slimeSprite0x1;
        _slimeSpriteLookup[0x2] = slimeSprite0x1;
        _slimeSpriteLookup[0x3] = slimeSprite0x3;
        _slimeSpriteLookup[0x4] = slimeSprite0x1;
        _slimeSpriteLookup[0x5] = slimeSprite0x5;
        _slimeSpriteLookup[0x6] = slimeSprite0x3;
        _slimeSpriteLookup[0x7] = slimeSprite0x7;
        _slimeSpriteLookup[0x8] = slimeSprite0x1;
        _slimeSpriteLookup[0x9] = slimeSprite0x3;
        _slimeSpriteLookup[0xA] = slimeSprite0x5;
        _slimeSpriteLookup[0xB] = slimeSprite0x7;
        _slimeSpriteLookup[0xC] = slimeSprite0x3;
        _slimeSpriteLookup[0xD] = slimeSprite0x7;
        _slimeSpriteLookup[0xE] = slimeSprite0x7;
        _slimeSpriteLookup[0xF] = slimeSprite0xF;
    }

    public void Awake() {
        if (_slimeSpriteLookup == null) {
            initSlimeSprites();
        }

        _tilemap = Tilemap.getInstance();
        _myTile = GetComponent<Tile>();

        GameObject slimeRendererObject = new GameObject("Slime");
        slimeRendererObject.transform.parent = transform;
        slimeRendererObject.transform.position = transform.position;
        _slimeRenderer = slimeRendererObject.AddComponent<SpriteRenderer>();
        _slimeRenderer.sortingLayerName = "Slime";

        updateNeighborCount(true, 1);
    }

    public void OnDestroy() {
        updateNeighborCount(true, _isSolid ? -1 : 0);
    }

    public void forceSlimeUpdate(int modifyNeighborCount = 0) {
        enabled = true;
        _slimeNeighbors += modifyNeighborCount;
        updateNeighborCount(false);
    }

    [ContextMenu ("Toggle Solidity")]
    public void toggleSolid() {
        setSolid(!_isSolid);
    }

    public void setSolid(bool isSolid) {
        if (_isSolid != isSolid) {
            _isSolid = isSolid;
            forceSlimeUpdate();

            if (_isSolid) {
                List<Tile> neighbors = _tilemap.getNeighboringTiles(transform.position);
                foreach (Tile tile in neighbors) {
                    if (tile.GetComponent<Slime>() == null) {
                        tile.gameObject.AddComponent<Slime>();
                    }
                }
            }

            updateNeighborCount(true, _isSolid ? 1 : -1);
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

    private void updateNeighborCount(bool forceNeighborUpdates = false, int modifyNeighborCount = 0) {
        List<Tile> neighbors = _tilemap.getNeighboringTiles(transform.position);
        _slimeNeighbors = 0;

        int spriteMask = 0;

        foreach (Tile tile in neighbors) {
            Slime slime = tile.GetComponent<Slime>();
            if (slime != null) {
                if (slime._isSolid) {
                    Vector2Int delta = Tilemap.getTilemapLocation(tile.transform.position) - Tilemap.getTilemapLocation(transform.position);
                    int neighborMask = 0xF;
                    if (delta.y == 1) neighborMask &= 0x3; //0011
                    if (delta.y == -1) neighborMask &= 0xC; //1100
                    if (delta.x == 1) neighborMask &= 0x6; //0110
                    if (delta.x == -1) neighborMask &= 0x9; //1001
                    spriteMask |= neighborMask;

                    _slimeNeighbors++;
                }
                if (forceNeighborUpdates) {
                    slime.forceSlimeUpdate(modifyNeighborCount);
                }
            }
        }

        if (_isSolid) {
            spriteMask = 0xF;
        }
        _slimeRenderer.sprite = _slimeSpriteLookup[spriteMask];
        _slimeRenderer.gameObject.transform.eulerAngles = new Vector3(0, 0, -_slimeSpriteAngleLookup[spriteMask]);
    }
}
