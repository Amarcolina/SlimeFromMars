using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Slime : MonoBehaviour {
    public const float OPACITY_CHANGE_SPEED = 1.0f;
    public const float HEALTH_REGEN_RATE = 0.1f;
    public const float TIME_PER_EXPAND = 0.02f;
    public const float SLIME_RENDERER_MORPH_TIME = 0.1f;

    public Sprite textureRamp = null;

    private float _percentHealth = 1.0f;
    private Path _currentExpandPath = null;
    private float _timeUntilExpand = 0.0f;

    private SlimeRenderer _slimeRenderer = null;

    public bool _isDead = false;
    private bool _killNeighbors = false;

    private static float _nextSlimeDestructionTime = 0.0f;
    private static List<Slime> _slimesToDestroyList = new List<Slime>();

    private int _aliveIndex = 0;
    private static int _currSearchingAliveIndex = 0;
    private static Vector2Int _anchorSlimeLocation = null;


    /* This method does the following:
     *      Initializes the slime sprite lookup table if it is not already initialized
     *      Creates the gameobject to display the slime sprite
     *      Updates the neighbor count of this slime
     *      Lets all neighboring slimes know that this slime has been added
     */
    public void Start() {
        if (_anchorSlimeLocation == null) {
            _anchorSlimeLocation = transform.position;
        }

        _slimeRenderer = GetComponent<SlimeRenderer>();
        if (_slimeRenderer == null) {
            _slimeRenderer = gameObject.AddComponent<SlimeRenderer>();
        }
        _slimeRenderer.setTextureRamp(textureRamp);
        _slimeRenderer.setMorphTime(SLIME_RENDERER_MORPH_TIME);
        _slimeRenderer.wakeUpRenderer();
    }

    /* Forces this slime to wake up.  This causes it to recount it's
     * neighbors, as well as start the Update cycle until it can fall
     * asleep again
     */
    public void wakeUpSlime() {
        enabled = true;
        if (_slimeRenderer) {
            _slimeRenderer.wakeUpRenderer();
        }
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

        int c = 0;
        foreach (Slime ss in _slimesToDestroyList) {
            if (ss == this) {
                c++;
            }
        }
        if (c > 1) {
            Debug.Log("nooooo");
        }


        if (_isDead) {
            if (_killNeighbors) {
                killNeighbors();
                canGoToSleep = false;
                _killNeighbors = false;
            }


            




            if (_slimesToDestroyList.Count != 0 && _slimesToDestroyList[0] == this) {
                if (_nextSlimeDestructionTime <= Time.time) {
                    //Debug.Log(_nextSlimeDestructionTime + " : " + Time.time);
                    damageSlime(1.0f);
                    _nextSlimeDestructionTime = _nextSlimeDestructionTime + 1.0f;

                    if (_slimesToDestroyList.Count > 1) {
                        int randomIndex = Random.Range(1, _slimesToDestroyList.Count - 1);
                        _slimesToDestroyList[0] = _slimesToDestroyList[randomIndex];
                        _slimesToDestroyList[randomIndex] = _slimesToDestroyList[_slimesToDestroyList.Count - 1];
                        _slimesToDestroyList[0].wakeUpSlime();
                    }

                    _slimesToDestroyList.RemoveAt(_slimesToDestroyList.Count - 1);
                }
                canGoToSleep = false;
            }
        } else {
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
        wakeUpSlime();
        if (_percentHealth <= 0.0f) {
            Vector2Int deathOrigin = transform.position;
            DestroyImmediate(this);
            if (!_isDead) {
                handleSlimeDetatch(deathOrigin);
            }
        }
    }

    private void handleSlimeDetatch(Vector2Int origin) {
        Astar.isWalkableFunction = isSlimeTile;
        Astar.earlySuccessFunction = isLocationAlive;
        Astar.earlyFailureFunction = isLocationDead;

        _currSearchingAliveIndex++;

        for (int i = 0; i < TilemapUtilities.neighborFullArray.Length; i++) {
            Vector2Int neighborPos = origin + TilemapUtilities.neighborFullArray[i];

            GameObject neighborObj = Tilemap.getInstance().getTileGameObject(neighborPos);
            if (neighborObj == null) {
                continue;
            }

            Slime neighborSlime = neighborObj.GetComponent<Slime>();
            if (neighborSlime == null) {
                continue;
            }

            Path pathHome = Astar.findPath(neighborPos, _anchorSlimeLocation, false);

            if (pathHome == null) {
                _slimesToDestroyList.Add(neighborSlime);
                neighborSlime._isDead = true;
                neighborSlime._killNeighbors = true;
                neighborSlime.wakeUpSlime();
            } else {
                for (int j = 0; j < pathHome.Count; j++) {
                    Vector2Int pathNode = pathHome[j];
                    Slime s = Tilemap.getInstance().getTileGameObject(pathNode).GetComponent<Slime>();
                    s._aliveIndex = _currSearchingAliveIndex;
                }
            }
        }

        _nextSlimeDestructionTime = Time.time + 1.0f;
    }

    private void killNeighbors() {
        for (int i = 0; i < TilemapUtilities.neighborFullArray.Length; i++) {
            Vector2Int neighborPos = Tilemap.getTilemapLocation(transform.position) + TilemapUtilities.neighborFullArray[i];
            if(!TilemapUtilities.areTilesNeighbors(Tilemap.getTilemapLocation(transform.position), neighborPos, true, Astar.defaultIsWalkable)){
                continue;
            }

            GameObject neighborObj = Tilemap.getInstance().getTileGameObject(neighborPos);
            if (neighborObj == null) {
                continue;
            }

            Slime neighborSlime = neighborObj.GetComponent<Slime>();
            if (neighborSlime == null) {
                continue;
            }

            if (!neighborSlime._isDead) {
                _slimesToDestroyList.Add(neighborSlime);
                neighborSlime._isDead = true;
                neighborSlime._killNeighbors = true;
                neighborSlime.wakeUpSlime();
            }
        }
    }

    private static bool isSlimeTile(Vector2Int location) {
        GameObject tileObj = Tilemap.getInstance().getTileGameObject(location);
        if (tileObj == null) {
            return false;
        }

        return tileObj.GetComponent<Slime>() != null;
    }

    private static bool isLocationDead(Vector2Int location) {
        GameObject tileObj = Tilemap.getInstance().getTileGameObject(location);
        if (tileObj == null) {
            return false;
        }
        Slime s = tileObj.GetComponent<Slime>();
        if (s == null) {
            return false;
        }

        return s._isDead;
    }

    private static bool isLocationAlive(Vector2Int location) {
        GameObject tileObj = Tilemap.getInstance().getTileGameObject(location);
        if (tileObj == null) {
            return false;
        }

        Slime s = tileObj.GetComponent<Slime>();
        if (s == null) {
            return false;
        }

        return s._aliveIndex == _currSearchingAliveIndex;
    }

    /* Requests that this slime expand allong the given path.  It will expand 
     * to the current node in the path.  This triggers a chain reaction where
     * the expanded slime follows the next node and so on
     * 
     * This wakes up the slime
     */
    public void requestExpansionAllongPath(Path path) {
        if (path.getNodeCount() <= 1) {
            return;
        }
        path.getNext();
        requestExpansionInternal(path, TIME_PER_EXPAND);
    }

    /* This returns the amount of enery it would cost to grow
     * the slime along the given path.  
     */
    public static int getPathCost(Path path) {
        int cost = 0;
        Tilemap tilemap = Tilemap.getInstance();
        for (int i = 0; i < path.Count; i++) {
            Vector2Int node = path[i];
            Slime slime = tilemap.getTile(node).GetComponent<Slime>();
            if (slime == null) {
                cost++;
            }
        }
        return cost;
    }

    private void requestExpansionInternal(Path path, float residualTimeLeft) {
        wakeUpSlime();

        _currentExpandPath = path;
        _timeUntilExpand = residualTimeLeft;
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
        float residualTimeLeft = _timeUntilExpand + TIME_PER_EXPAND;

        Tile newSlimeTile = Tilemap.getInstance().getTile(nextNode);
        if (newSlimeTile && newSlimeTile.isWalkable){
            Slime newSlime = newSlimeTile.GetComponent<Slime>();

            if(newSlime == null){
                newSlime = newSlimeTile.gameObject.AddComponent<Slime>();
                newSlime.textureRamp = textureRamp;
            } else {
                residualTimeLeft = 0.0f;
            }

            if (_currentExpandPath.getNodesLeft() > 0) {
                newSlime.requestExpansionInternal(_currentExpandPath, residualTimeLeft);
            } else {
                SlimeController.getInstance().setSelectedSlime(newSlime);
            }
        }

        _currentExpandPath = null;
    }
}
