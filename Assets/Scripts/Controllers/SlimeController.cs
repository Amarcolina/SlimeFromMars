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
    private int radiationLevel;
    private int electricityLevel;
    private int bioLevel;

    //cost for using skills
    private const int ELECTRICITY_DEFENSE_COST = 5;
    private const int ELECTRICITY_OFFENSE_COST = 10;
    private const int BIO_DEFENSE_COST = 8;
    private const int BIO_OFFENSE_COST = 5;
    private const int RADIATION_DEFENSE_COST = 10;
    private const int RADIATION_OFFENSE_COST = 10;
    
    private bool elementalMode = false;
    //selected tile of slime
    private Slime currentSelectedSlime;
    private static SlimeController _instance = null;
    public static SlimeController getInstance() {
        if (_instance == null) {
            _instance = FindObjectOfType<SlimeController>();
        }
        return _instance;
    }

    // Use this for initialization
    void Start() {
        radiationLevel = 0;
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
            //calculates astar path with start and goal locations, then calculates the cost of the path
            Vector2Int startLocation = Tilemap.getTilemapLocation(currentSelectedSlime.transform.position);
            Vector2Int goalLocation = Tilemap.getTilemapLocation(getTilePositionUnderCursor().transform.position);
            Path astarPath = Astar.findPath(startLocation, goalLocation);
            int pathCost = Slime.getPathCost(astarPath);

            //if the slime has the energy to move, take the astar path
            if (energy >= pathCost) {
                loseEnergy(pathCost);
                currentSelectedSlime.requestExpansionAllongPath(astarPath);
            } else {
                //message: not enough energy
            }
        }
        //if in elemental mode, slime tile is selected and you have correct mutation
        if (Input.GetKeyDown(KeyCode.E) && currentSelectedSlime != null && electricityLevel > 0) {
            elementalMode = true;
        }
        if (elementalMode) {
            if (Input.GetKeyDown(KeyCode.D) && energy >= ELECTRICITY_DEFENSE_COST) {
                elementalMode = false;
                Vector2Int circleCenter = Tilemap.getTilemapLocation(currentSelectedSlime.transform.position);
                useElectricityDefense(circleCenter);
            }

            if (Input.GetKeyDown(KeyCode.O) && energy >= ELECTRICITY_OFFENSE_COST) {
                elementalMode = false;
                if (Input.GetMouseButtonDown(1)) {

                    useElectricityOffense();
                }
            }
        }
    }

    public void consume(GenericConsumeable eatenItem) {
        //calculates resource bonus from item element affinity multiplied by level of slime attribute
        //calculates default item resource value based on size and adds any bonuses
        energy = (int)eatenItem.size + radiationLevel * eatenItem.radiation + bioLevel * eatenItem.bio + electricityLevel * eatenItem.electricity;

        //if the eatenItem is a mutation, level up affinity
        if (eatenItem.isRadiationMutation) {
            radiationLevel++;
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
            setSelectedSlime(slimeTile);
        }
    }

    public void setSelectedSlime(Slime slime) {
        currentSelectedSlime = slime;
        //moves highlighter to tile position
        transform.position = currentSelectedSlime.transform.position;
        //makes sprite visible
        renderer.enabled = true;
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

    private void loseEnergy(int cost) {
        energy -= cost;
    }

    public void useRadiationDefense() {
        loseEnergy(RADIATION_DEFENSE_COST);
    }
    public void useRadiationOffense() {
        loseEnergy(RADIATION_OFFENSE_COST);
    }

    //outputs circle of enemy-damaging electricity from central point of selected slime tile
    //radius increases with electricityLevel
    public void useElectricityDefense(Vector2Int center) {
        int circleRadius = electricityLevel;
        for (int dx = -circleRadius; dx <= circleRadius; dx++) {
            for (int dy = -circleRadius; dy <= circleRadius; dy++) {
                Vector2 tileOffset = new Vector2(dx, dy);
                if (tileOffset.sqrMagnitude <= circleRadius * circleRadius) {
                    Tile tile = Tilemap.getInstance().getTile(center + new Vector2Int(dx, dy));
                    if (tile != null && tile.GetComponent<Slime>() != null) {
                        tile.gameObject.AddComponent<Electrified>();
                    }
                }
            }
        }
        loseEnergy(ELECTRICITY_DEFENSE_COST);
    }

    public void useElectricityOffense() {
        loseEnergy(ELECTRICITY_OFFENSE_COST);
    }

    public void useBioDefense() {
        loseEnergy(BIO_DEFENSE_COST);
    }
    public void useBioOffense() {
        loseEnergy(BIO_OFFENSE_COST);
    }
}

