using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Slime : MonoBehaviour {
    public const float OPACITY_CHANGE_SPEED = 1.0f;
    public const float HEALTH_REGEN_RATE = 0.1f;
    public const float TIME_PER_EXPAND = 0.02f;

    public const float SLIME_RENDERER_TIME = 0.2f;

    public Texture2D textureRamp = null;

    private Tilemap _tilemap;
    
    private float _percentHealth = 1.0f;

    private Path _currentExpandPath = null;
    private float _timeUntilExpand = 0.0f;

    /* This method does the following:
     *      Initializes the slime sprite lookup table if it is not already initialized
     *      Creates the gameobject to display the slime sprite
     *      Updates the neighbor count of this slime
     *      Lets all neighboring slimes know that this slime has been added
     */
    public void Start() {
        _tilemap = Tilemap.getInstance();

        GameObject slimeRendererObject = new GameObject("Slime");
        slimeRendererObject.transform.parent = transform;
        slimeRendererObject.transform.position = transform.position;

        SlimeRenderer slimeRenderer = GetComponent<SlimeRenderer>();
        if (slimeRenderer == null) {
            slimeRenderer = gameObject.AddComponent<SlimeRenderer>();
            slimeRenderer.distanceRamp = textureRamp;
            slimeRenderer.morphTime = SLIME_RENDERER_TIME;
            slimeRenderer.wakeUpRenderer();
        }

        wakeUpSlime();
    }

    /* Forces this slime to wake up.  This causes it to recount it's
     * neighbors, as well as start the Update cycle until it can fall
     * asleep again
     */
    public void wakeUpSlime() {
        enabled = true;
        SlimeRenderer slimeRenderer = GetComponent<SlimeRenderer>();
        if (slimeRenderer != null) {
            slimeRenderer.wakeUpRenderer();
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
        wakeUpSlime();
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
        for (int i = 0; i < path.Count - 1; i++) {
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

        Tile newSlimeTile = _tilemap.getTile(nextNode);
        if (newSlimeTile && newSlimeTile.isWalkable) {
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
