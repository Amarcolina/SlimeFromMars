using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*This class keeps track of slime attribute values, mutation types, offense/defense abilities based on mutation type, and offense/defense values
 * 
 */
public class SlimeController : MonoBehaviour {
    //energy is a pool of resources used to move, attack and defend
    private int energy;
    private GameUI _gameUi;

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

    //base damage for skills
    private const int ELECTRICITY_BASE_DAMAGE = 5;
    private const int BIO_BASE_DAMAGE = 2;
    private const int RADIATION_BASE_DAMAGE = 1;

    //base range for skills
    private const int ELECTRICITY_BASE_RANGE = 5;
    private const int BIO_BASE_RANGE = 2;
    private const int RADIATION_BASE_RANGE = 4;
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
        _gameUi = GameUI.getInstance();
        radiationLevel = 0;
        electricityLevel = 0;
        bioLevel = 0;
        gainEnergy(20);
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
        if (Input.GetKeyDown(KeyCode.E) && currentSelectedSlime != null) {
            elementalMode = true;
        }
        if (elementalMode) {
            if (Input.GetKeyDown(KeyCode.D) && electricityLevel > 0 && energy >= ELECTRICITY_DEFENSE_COST) {
                elementalMode = false;
                Vector2Int circleCenter = Tilemap.getTilemapLocation(currentSelectedSlime.transform.position);
                
                useElectricityDefense(circleCenter);

                AudioSource electricDefenseSource = currentSelectedSlime.gameObject.AddComponent<AudioSource>();
                electricDefenseSource.clip = Resources.Load<AudioClip>("Sounds/SFX/electricity_defense");
                electricDefenseSource.Play();
            }

            if (Input.GetKeyDown(KeyCode.O) && bioLevel > 0 && energy >= BIO_DEFENSE_COST) {
                elementalMode = false;
                Vector2Int circleCenter = Tilemap.getTilemapLocation(currentSelectedSlime.transform.position);
                useBioDefense(circleCenter);
                useElectricityDefense(circleCenter);

                AudioSource bioDefenseSource = currentSelectedSlime.gameObject.AddComponent<AudioSource>();
                bioDefenseSource.clip = Resources.Load<AudioClip>("Sounds/SFX/bio_defense");
                bioDefenseSource.Play();
            }

            if (Input.GetKeyDown(KeyCode.R) && radiationLevel > 0 && energy >= RADIATION_DEFENSE_COST) {
                elementalMode = false;
                Vector2Int circleCenter = Tilemap.getTilemapLocation(currentSelectedSlime.transform.position);
                useRadiationDefense(circleCenter);

                AudioSource radiationDefenseSource = currentSelectedSlime.gameObject.AddComponent<AudioSource>();
                radiationDefenseSource.clip = Resources.Load<AudioClip>("Sounds/SFX/radiation_defense");
                radiationDefenseSource.Play();
            }

            /*###################### DISABLED FOR TESTING########################
            if (Input.GetKeyDown(KeyCode.O) && energy >= ELECTRICITY_OFFENSE_COST) {
                elementalMode = false;
                if (Input.GetMouseButtonDown(1)) {
                    useElectricityOffense();
                }
            } */
        }
    }

    public void consume(GenericConsumeable eatenItem) {
        //calculates resource bonus from item element affinity multiplied by level of slime attribute
        //calculates default item resource value based on size and adds any bonuses
        gainEnergy((int)eatenItem.size + radiationLevel * eatenItem.radiation + bioLevel * eatenItem.bio + electricityLevel * eatenItem.electricity);

        //if the eatenItem is a mutation, level up affinity
        if (eatenItem.isRadiationMutation) {
            radiationLevel++;
            //   _gameUi.RadiationUpdate(radiationLevel);
        }
        if (eatenItem.isElectricityMutation) {
            electricityLevel++;
            _gameUi.LightningUpdate(electricityLevel);
        }
        if (eatenItem.isBioMutation) {
            bioLevel++;
            //  _gameUi.BioUpdate(bioLevel);
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
        _gameUi.ResourceUpdate(energy);
    }

    private void gainEnergy(int plus) {
        energy += plus;
        _gameUi.ResourceUpdate(energy);

    }
    /*###################################### ELEMENTAL SKILLS #######################################*/
    public void useRadiationDefense(Vector2Int center) {
        loseEnergy(RADIATION_DEFENSE_COST);
    }

    public void useRadiationOffense(Vector2Int center) {
        float rangeOfAttack = RADIATION_BASE_RANGE * radiationLevel;

        //gets distance between slime and enemy
        Vector2Int startLocation = Tilemap.getTilemapLocation(currentSelectedSlime.transform.position);
        Vector2Int goalLocation = Tilemap.getTilemapLocation(getTilePositionUnderCursor().transform.position);
        float distance = Mathf.Sqrt((goalLocation.x - startLocation.x) * (goalLocation.x - startLocation.x) +
                            (goalLocation.y - startLocation.y) * (goalLocation.y - startLocation.y));
        //if distance is within range of attack, create the radius of radiation
        if (distance <= rangeOfAttack) {
            int circleRadius = 3 * radiationLevel;
            for (int dx = -circleRadius; dx <= circleRadius; dx++) {
                for (int dy = -circleRadius; dy <= circleRadius; dy++) {
                    Vector2 tileOffset = new Vector2(dx, dy);
                    if (tileOffset.sqrMagnitude <= circleRadius * circleRadius) {
                        Tile tile = Tilemap.getInstance().getTile(center + new Vector2Int(dx, dy));
                        if (tile != null) {
                            tile.gameObject.AddComponent<Irradiated>();
                        }
                    }
                }
            }
        }
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

    //sends a bolt of electricity at an enemy, up to a max distance away
    public void useElectricityOffense() {
        float damageDone = ELECTRICITY_BASE_DAMAGE * electricityLevel;
        float rangeOfAttack = ELECTRICITY_BASE_RANGE * electricityLevel;

        //gets distance between slime and enemy
        Vector2Int startLocation = Tilemap.getTilemapLocation(currentSelectedSlime.transform.position);
        Vector2Int goalLocation = Tilemap.getTilemapLocation(getTilePositionUnderCursor().transform.position);
        float distance = Mathf.Sqrt((goalLocation.x - startLocation.x) * (goalLocation.x - startLocation.x) +
                            (goalLocation.y - startLocation.y) * (goalLocation.y - startLocation.y));
        if (distance <= rangeOfAttack) {
            bool wasDamaged = getTilePositionUnderCursor().damageTileEntities(damageDone);
            if (wasDamaged) {
                loseEnergy(ELECTRICITY_OFFENSE_COST);
            }
        }
    }

    //outputs circle of thick, high health slime from central point of selected slime tile
    //radius increases with bioLevel
    public void useBioDefense(Vector2Int center) {
        int circleRadius = bioLevel;
        for (int dx = -circleRadius; dx <= circleRadius; dx++) {
            for (int dy = -circleRadius; dy <= circleRadius; dy++) {
                Vector2 tileOffset = new Vector2(dx, dy);
                if (tileOffset.sqrMagnitude <= circleRadius * circleRadius) {
                    Tile tile = Tilemap.getInstance().getTile(center + new Vector2Int(dx, dy));
                    if (tile != null && tile.GetComponent<Slime>() != null) {
                        tile.isWalkable = false;
                        tile.gameObject.AddComponent<BioMutated>();
                    }
                }
            }
        }
        loseEnergy(BIO_DEFENSE_COST);
    }

    public void useBioOffense() {
        loseEnergy(BIO_OFFENSE_COST);
    }
}

