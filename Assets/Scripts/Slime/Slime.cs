using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Slime : MonoBehaviour {
    public const float OPACITY_CHANGE_SPEED = 1.0f;
    public const float HEALTH_REGEN_RATE = 0.1f;
    public const float TIME_PER_EXPAND = 0.1f;

    public bool startSolid = false;

    private static Sprite[] _slimeSpriteLookup = null;
    private static int[] _slimeSpriteAngleLookup = { 0, 0, 90, 0, 180, 0, 90, 0, 270, 270, 180, 270, 180,  180, 90, 0 };

    private bool _isSolid = false;
    private Tile _myTile;
    private Tilemap _tilemap;
    private SpriteRenderer _slimeRenderer;
    
    private float _percentHealth = 1.0f;
    private int _solidSlimeNeighborCount = 0;
    private float _currentOpacity = 0.0f;

    private Path _currentExpandPath = null;
    private float _timeUntilExpand = 0.0f;

    /* This initializes the _slimeSpriteLookup table.  This is a static table and so this
     * method only needs to be called once.  The table is static so tat all slime objects
     * can use the same sprites.
     */
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

    /* This method does the following:
     *      Initializes the slime sprite lookup table if it is not already initialized
     *      Creates the gameobject to display the slime sprite
     *      Updates the neighbor count of this slime
     *      Lets all neighboring slimes know that this slime has been added
     */
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

        if (startSolid) {
            setSolid(true);
        }

        updateNeighborCount(true);
    }

    public void OnDestroy() {
        Destroy(_slimeRenderer.gameObject);
    }

    /* Forces this slime to wake up.  This causes it to recount it's
     * neighbors, as well as start the Update cycle until it can fall
     * asleep again
     */
    public void wakeUpSlime() {
        enabled = true;
        updateNeighborCount(false);
    }

    /* Sets whether or not this slime should be solid.  Solid slime blocks
     * always stay around, even if they have no neighbors.  Un-Solid slime
     * blocks can only exist if they are touching at least 1 solid slime block
     * 
     * This method handles the addition of new slime blocks that may need
     * to be created, and also wakes the slime up
     */
    public void setSolid(bool shouldBeSolid) {
        if (_isSolid != shouldBeSolid) {
            _isSolid = shouldBeSolid;
            wakeUpSlime();

            if (_isSolid) {
                List<Tile> neighbors = _tilemap.getNeighboringTiles(transform.position);
                foreach (Tile tile in neighbors) {
                    if (tile.GetComponent<Slime>() == null) {
                        tile.gameObject.AddComponent<Slime>();
                    }
                }
            }

            updateNeighborCount(true);
        }
    }

    public bool isSolid() {
        return _isSolid;
    }

    /* The update loop is only proccessed if this slime is awake.
     * Every loop, the slime will try to go to sleep if it is able.
     * Every loop, it does the following:
     *      Checks to see if it is currently following a path
     *          If it is not time yet to expand, wait (prevent sleep)
     *          If it is time, expand
     *      Updates the current opacity to match the goal opacity
     *          If the current is not at the goal, move it towards the goal (prevent sleep)
     *      Handles health regeneration
     *          If the current health is not at maximum, regenerate (prevents sleep)
     * 
     * After all these actions are complete, go to sleep if we are able
     */
    public void Update() {
        bool canGoToSleep = true;

        if (_currentExpandPath != null) {
            _timeUntilExpand -= Time.deltaTime;
            if (_timeUntilExpand <= 0.0f) {
                expandSlime();
            }
            canGoToSleep = false;
        }

        if (_percentHealth != 1.0f && _percentHealth > 0.0f) {
            _percentHealth = Mathf.MoveTowards(_percentHealth, 1.0f, HEALTH_REGEN_RATE * Time.deltaTime);
            canGoToSleep = false;
        }

        if (canGoToSleep) {
            enabled = false;
        }
    }

    /* This damages the slime and lowers its total health
     * This wakes up the slime
     */
    public void damageSlime(float percentDamage) {
        _percentHealth -= percentDamage;
        if (_percentHealth <= 0.0f) {
            setSolid(false);
        }
        wakeUpSlime();
    }

    /* Requests that this slime expand allong the given path.  It will expand 
     * to the current node in the path.  This triggers a chain reaction where
     * the expanded slime follows the next node and so on
     * 
     * The user can specify a delay until the expansion occurs.  This is also
     * used internally in the chain reaction to control expand speed
     * 
     * This wakes up the slime
     */
    public void requestExpansionAllongPath(Path path, float timeUntilExpand = 0.0f) {
        wakeUpSlime();
        _currentExpandPath = path;
        _timeUntilExpand = timeUntilExpand;
        if (_timeUntilExpand <= 0.0f) {
            expandSlime();
        }
    }

    /* This is an internal method that handles the expansion of the slime
     * This calculates the node that we are expanding into, and handles
     * the creation of a new slime object if needed
     * 
     * This also handles the linking to the new slime node so that
     * the chain reaction can be sustained.  
     */
    private void expandSlime() {
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

    /* Updates the solid neghbor count of this slime.  This also updates the sprite
     * used to represent the slime based on the locations of neighboring solid
     * slimes.
     * 
     * An optional bool allows this method to wake up neighboring slimes
     */
    private void updateNeighborCount(bool shouldWakeUpNeighbors = false) {
        List<Tile> neighbors = _tilemap.getNeighboringTiles(transform.position);
        _solidSlimeNeighborCount = 0;

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

                    _solidSlimeNeighborCount++;
                }
                if (shouldWakeUpNeighbors) {
                    slime.wakeUpSlime();
                }
            }
        }

        if (_isSolid) {
            spriteMask = 0xF;
        } else {
            if (_solidSlimeNeighborCount == 0) {
                Destroy(this);
            }
        }
        _slimeRenderer.sprite = _slimeSpriteLookup[spriteMask];
        _slimeRenderer.gameObject.transform.eulerAngles = new Vector3(0, 0, -_slimeSpriteAngleLookup[spriteMask]);
    }
}
