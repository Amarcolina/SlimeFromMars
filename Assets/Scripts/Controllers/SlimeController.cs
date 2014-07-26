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

    //list of sound effects for abilities
    private AudioClip electricDefenseSFX;
    private AudioClip bioDefenseSFX;
    private AudioClip radioactiveDefenseSFX;

    //cost for using skills
    private const int ELECTRICITY_DEFENSE_COST = 5;
    private const int ELECTRICITY_OFFENSE_COST = 10;
    private const int BIO_DEFENSE_COST = 8;
    private const int BIO_OFFENSE_COST = 8;
    private const int RADIATION_DEFENSE_COST = 12;
    private const int RADIATION_OFFENSE_COST = 10;

    //base damage for skills
    private const int ELECTRICITY_BASE_DAMAGE = 5;
    private const int BIO_BASE_DAMAGE = 2;
    private const int RADIATION_BASE_DAMAGE = 1;

    //base range for skills
    private const int ELECTRICITY_BASE_RANGE = 5;
    private const int BIO_BASE_RANGE = 2;
    private const int RADIATION_BASE_RANGE = 4;

    //selected tile of slime
    private Slime currentSelectedSlime;
    private static SlimeController _instance = null;
    public static SlimeController getInstance() {
        if (_instance == null) {
            _instance = FindObjectOfType<SlimeController>();
        }
        return _instance;
    }

    void Awake() {
        //Load all sounds from File
        electricDefenseSFX = Resources.Load<AudioClip>("Sounds/SFX/electric_defense");
        bioDefenseSFX = Resources.Load<AudioClip>("Sounds/SFX/bio_defense");
        radioactiveDefenseSFX = Resources.Load<AudioClip>("Sounds/SFX/radiation_defense");
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

        if (Input.GetKeyDown(KeyCode.E)) {
            gainEnergy(1000000);
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            radiationLevel++;
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            electricityLevel++;
        }
        if (Input.GetKeyDown(KeyCode.B)) {
            bioLevel++;
        }

        if (currentSelectedSlime == null) {
            renderer.enabled = false;
        } else {
            attemptToEat();
        }

        if (Input.GetMouseButtonDown(1) && currentSelectedSlime != null) {
            Path astarPath = Astar.findPath(getStartLocation(), getGoalLocation());
            int pathCost = Slime.getPathCost(astarPath);

            //if the slime has the energy to move, take the astar path
            if (energy >= pathCost) {
                loseEnergy(pathCost);
                currentSelectedSlime.requestExpansionAllongPath(astarPath);
            } else {
                //message: not enough energy
            }
        }
    }

    //AudioSource.PlayClipAtPoint(electricDefenseSFX, currentSelectedSlime.transform.position);
    //AudioSource.PlayClipAtPoint(electricDefenseSFX, currentSelectedSlime.transform.position);
    //AudioSource.PlayClipAtPoint(bioDefenseSFX, currentSelectedSlime.transform.position);
    //AudioSource.PlayClipAtPoint(radioactiveDefenseSFX, currentSelectedSlime.transform.position);
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
    public Vector2Int getStartLocation() {
        return Tilemap.getTilemapLocation(currentSelectedSlime.transform.position);
    }

    public Vector2Int getGoalLocation() {
        return Tilemap.getTilemapLocation(getTilePositionUnderCursor().transform.position);
    }

    /*###################################### ELEMENTAL SKILLS #######################################*/

    //Allows slime to irradiate tiles permanently so that enemies that walk into the area are stunned for short periods of time
    public void useRadiationDefense() {
        float rangeOfAttack = RADIATION_BASE_RANGE * radiationLevel;
        //if distance is within range of attack, check each tile in the radius and then irradiate each tile that can be irradiated
        if (Vector2Int.distance(getStartLocation(), getGoalLocation()) <= rangeOfAttack) {
            int circleRadius = 3 * radiationLevel;

            AudioSource.PlayClipAtPoint(radioactiveDefenseSFX, getGoalLocation());

            for (int dx = -circleRadius; dx <= circleRadius; dx++) {
                for (int dy = -circleRadius; dy <= circleRadius; dy++) {
                    Vector2 tileOffset = new Vector2(dx, dy);
                    if (tileOffset.sqrMagnitude <= circleRadius * circleRadius) {
                        Tile tile = Tilemap.getInstance().getTile(getStartLocation() + new Vector2Int(dx, dy));
                        if (tile != null) {
                            Irradiated radComponent = tile.GetComponent<Irradiated>();
                            if (radComponent == null) {
                                radComponent = tile.gameObject.AddComponent<Irradiated>();
                            }
                            radComponent.setStunned(true);
                        }
                    }
                }
            }
        }
        loseEnergy(RADIATION_DEFENSE_COST);
    }

    //Allows slime to irradiate an area for a period of time such that enemies are damaged per second
    //Damage and range will increase based on level
    public void useRadiationOffense() {
        float rangeOfAttack = RADIATION_BASE_RANGE * radiationLevel;
        //if distance is within range of attack, create the radius of radiation
        if (Vector2Int.distance(getStartLocation(), getGoalLocation()) <= rangeOfAttack) {
            int circleRadius = 3 * radiationLevel;
            for (int dx = -circleRadius; dx <= circleRadius; dx++) {
                for (int dy = -circleRadius; dy <= circleRadius; dy++) {
                    Vector2 tileOffset = new Vector2(dx, dy);
                    if (tileOffset.sqrMagnitude <= circleRadius * circleRadius) {
                        Tile tile = Tilemap.getInstance().getTile(getStartLocation() + new Vector2Int(dx, dy));
                        if (tile != null) {
                            Irradiated radComponent = tile.GetComponent<Irradiated>();
                            if (radComponent == null) {
                                radComponent = tile.gameObject.AddComponent<Irradiated>();
                            }
                            radComponent.setDamaged(true);
                        }
                    }
                }
            }
        }
        loseEnergy(RADIATION_OFFENSE_COST);
    }

    //outputs circle of enemy-damaging electricity from central point of selected slime tile
    //radius increases with electricityLevel, as does damage
    public void useElectricityDefense() {
        AudioSource.PlayClipAtPoint(electricDefenseSFX, getStartLocation());

        int circleRadius = electricityLevel;
        for (int dx = -circleRadius; dx <= circleRadius; dx++) {
            for (int dy = -circleRadius; dy <= circleRadius; dy++) {
                Vector2 tileOffset = new Vector2(dx, dy);
                if (tileOffset.sqrMagnitude <= circleRadius * circleRadius) {
                    Tile tile = Tilemap.getInstance().getTile(getStartLocation() + new Vector2Int(dx, dy));
                    if (tile != null && tile.GetComponent<Slime>() != null) {
                        tile.gameObject.AddComponent<Electrified>();
                    }
                }
            }
        }
        loseEnergy(ELECTRICITY_DEFENSE_COST);
    }

    //sends a bolt of electricity at an enemy, up to a max distance away, and ars to nearby enemies for chain damage
    //damage and range increase with electricityLevel
    public void useElectricityOffense() {
        int damageDone = ELECTRICITY_BASE_DAMAGE * electricityLevel;
        float rangeOfAttack = ELECTRICITY_BASE_RANGE * electricityLevel;
        //if enemy is within range of attack, use electricity
        if (Vector2Int.distance(getStartLocation(), getGoalLocation()) <= rangeOfAttack) {
            bool canDamage = getTilePositionUnderCursor().canDamageEntities();

            //if an enemy was damaged, check to see if there are enemies close by to arc to
            if (canDamage) {
                loseEnergy(ELECTRICITY_OFFENSE_COST);
                GameObject electricityArc = new GameObject("ElectricityArc");
                electricityArc.transform.position = getTilePositionUnderCursor().transform.position;
                ElectricityArc arc = electricityArc.AddComponent<ElectricityArc>();
                arc.setArcRadius(electricityLevel + 1);
                arc.setArcDamage(damageDone);
                arc.setArcNumber(electricityLevel + 1);
            }
        }
    }

    //outputs a circle of thick, high health slime from central point of selected slime tile
    //radius and health increases with bioLevel
    //defense will remain until destroyed by enemies
    public void useBioDefense() {
        AudioSource.PlayClipAtPoint(bioDefenseSFX, getStartLocation());

        int circleRadius = bioLevel;
        for (int dx = -circleRadius; dx <= circleRadius; dx++) {
            for (int dy = -circleRadius; dy <= circleRadius; dy++) {
                Vector2 tileOffset = new Vector2(dx, dy);
                if (tileOffset.sqrMagnitude <= circleRadius * circleRadius) {
                    Tile tile = Tilemap.getInstance().getTile(getStartLocation() + new Vector2Int(dx, dy));
                    if (tile != null && tile.GetComponent<Slime>() != null) {
                        tile.isWalkable = false;
                        tile.gameObject.AddComponent<BioMutated>();
                    }
                }
            }
        }
        loseEnergy(BIO_DEFENSE_COST);
    }

    //Creates a tentacle that can stab and impale enemies, as well as drag them towards the slime at higher levels
    //Damage and range are based on level
    public void useBioOffense() {
        int damageDone = BIO_BASE_DAMAGE * bioLevel;
        float rangeOfAttack = BIO_BASE_RANGE * bioLevel;
        Path astarPath = Astar.findPath(getStartLocation(), getGoalLocation());
        
        float pathCost = astarPath.getLength();
        if (pathCost <= rangeOfAttack) {
            bool canDamage = getTilePositionUnderCursor().canDamageEntities();
            if (canDamage) {
                GameObject bioLance = new GameObject("BioLance");
                bioLance.transform.position = getTilePositionUnderCursor().transform.position;
                BioLance bio = bioLance.AddComponent<BioLance>(); 
                bio.setLancePath(astarPath);
                bio.setLanceDamage(damageDone);
                loseEnergy(BIO_OFFENSE_COST);
            }
        }
    }
}

