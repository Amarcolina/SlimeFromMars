using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*This class keeps track of slime attribute values, mutation types, offense/defense abilities based on mutation type, and offense/defense values
 * 
 */
public class SlimeController : MonoBehaviour {
    //energy is a pool of resources used to move, attack and defend
    private int energy;
    //levels dictate how much more powerful your attacks/defenses are
    //levels also give bonuses in energy from items of that attribute
    private int acidLevel;
    private int electricityLevel;
    private int bioLevel;
    private Slime currentSelectedSlime;
    // Use this for initialization
    void Start() {
        acidLevel = 0;
        electricityLevel = 0;
        bioLevel = 0;
        energy = 20;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            highlightSlimeTile();
        }

        if (currentSelectedSlime == null) {
            renderer.enabled = false;
        } else {
            attemptToEat();
        }

        if (Input.GetMouseButtonDown(1) && currentSelectedSlime != null) {
            Vector2Int startLocation = Tilemap.getTilemapLocation(currentSelectedSlime.transform.position);
            Vector2Int goalLocation = Tilemap.getTilemapLocation(getTilePositionUnderCursor().transform.position);
            Astar.findPath(startLocation, goalLocation);
            highlightSlimeTile();
        }
    }

    public void consume(GenericConsumeable eatenItem) {
        //calculates resource bonus from item element affinity multiplied by level of slime attribute
        //calculates default item resource value based on size and adds any bonuses
        energy = (int)eatenItem.size + acidLevel * eatenItem.acid + bioLevel * eatenItem.bio + electricityLevel * eatenItem.electricity;

        //if the eatenItem is a mutation, level up affinity
        if (eatenItem.isAcidMutation) {
            acidLevel++;
        }
        if (eatenItem.isElectricityMutation) {
            electricityLevel++;
        }
        if (eatenItem.isBioMutation) {
            bioLevel++;
        }

        Destroy(eatenItem.gameObject);
    }

    public void highlightSlimeTile() {
        Tile tileUnderCursor = getTilePositionUnderCursor();

        //gets the slime component under the highlighted tile, if it exists
        Slime slimeTile = tileUnderCursor.GetComponent<Slime>();
        if (slimeTile != null) {
            //makes sprite visible
            renderer.enabled = true;
            currentSelectedSlime = slimeTile;
            //moves highlighter to tile position
            transform.position = tileUnderCursor.transform.position;
        }
    }

    public Tile getTilePositionUnderCursor() {
        //finds the cursorPosition and then uses cursorPosition to find position of tileUnderCursor
        Camera testCam = Camera.main;
        Vector2 cursorPosition = testCam.ScreenToWorldPoint(Input.mousePosition);
        Tilemap tilemap = Tilemap.getInstance();
        return tilemap.getTile(cursorPosition);
    }

    public void attemptToEat() {
        Tile tileComponent = currentSelectedSlime.GetComponent<Tile>();
        HashSet<TileEntity> entities = tileComponent.getTileEntities();
        if (entities != null) {
            foreach (TileEntity entity in entities) {
                GenericConsumeable possibleConsumeable = entity.GetComponent<GenericConsumeable>();
                if (possibleConsumeable != null) {
                    consume(possibleConsumeable);
                }
            }
        }
    }
    public void useAcidOffense() {
        //multiply acidLevel to attack power (radius?) to get offense output
        //do same for defense
    }
    public void useElectricityOffense() {
    }
    public void useBioOffense() {
    }


    public void useAcidDefense() {
    }
    public void useElectricityDefense() {
    }
    public void useBioDefense() {
    }
}
