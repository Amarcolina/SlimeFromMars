using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum ElementalCastType {
    NONE,
    ELECTRICITY_OFFENSIVE,
    BIO_OFFENSIVE,
    RADIATION_OFFENSIVE,
    RADIATION_DEFENSIVE
}



/*This class keeps track of slime attribute values, mutation types, offense/defense abilities based on mutation type, and offense/defense values
 * 
 */
public class SlimeController : MonoBehaviour {
    //energy is a pool of resources used to move, attack and defend
    public int energy;
    private GameUI _gameUi;
    public GameObject spinePrefab;
    //levels dictate how much more powerful your attacks/defenses are
    //levels also give bonuses in energy from items of that attribute
    private int radiationLevel;
    private int electricityLevel;
    private int bioLevel;

    //list of sound effects for abilities
    private AudioClip electricDefenseSFX;
    private AudioClip bioDefenseSFX;
    private AudioClip bioOffenseSFX;
    private AudioClip radioactiveDefenseSFX;
    private AudioClip radioactiveOffenseSFX;
    private AudioClip slimeExpansionSFX;
    private AudioClip slimeEatingSFX;

    //cost for using skills
    public const int ELECTRICITY_DEFENSE_COST = 5;
    public const int ELECTRICITY_OFFENSE_COST = 10;
    public const int BIO_DEFENSE_COST = 8;
    public const int BIO_OFFENSE_COST = 8;
    public const int RADIATION_DEFENSE_COST = 12;
    public const int RADIATION_OFFENSE_COST = 10;

    //base damage for skills
    private const int ELECTRICITY_BASE_DAMAGE = 5;
    private const int BIO_BASE_DAMAGE = 2;
    private const int RADIATION_BASE_DAMAGE = 1;

    //base range for skills
    private const int ELECTRICITY_BASE_RANGE = 3;
    private const int BIO_BASE_RANGE = 3;
    private const int RADIATION_BASE_RANGE = 2;

    private ElementalCastType _currentCastType = ElementalCastType.NONE;
    private bool _shouldSkipNext = false;

	//The Animator for the eye, used to transfer states via triggers
	public Animator Eye_Animator;

    //asdasd
    private Path _slimeHighlightPath = null;
    private Texture2D _greenTexture;
    private Texture2D _redTexture;

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
        electricDefenseSFX = Resources.Load<AudioClip>("Sounds/SFX/electricity_defense");

        bioDefenseSFX = Resources.Load<AudioClip>("Sounds/SFX/bio_defense");
        bioOffenseSFX = Resources.Load<AudioClip>("Sounds/SFX/bio_offense_impale");

        radioactiveDefenseSFX = Resources.Load<AudioClip>("Sounds/SFX/radiation_defense");
        radioactiveOffenseSFX = Resources.Load<AudioClip>("Sounds/SFX/radiation_offense");

        slimeExpansionSFX = Resources.Load<AudioClip>("Sounds/SFX/slime_expanding");
        slimeEatingSFX = Resources.Load<AudioClip>("Sounds/SFX/slime_eating");
        _greenTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
        _greenTexture.SetPixel(0, 0, new Color(0.3f, 0.45f, 0.3f, 0.5f));
        _greenTexture.Apply();
        _redTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
        _redTexture.SetPixel(0, 0, new Color(0.5f, 0.3f, 0.3f, 0.6f));
        _redTexture.Apply();
    }

    // Use this for initialization
    void Start() {
        _gameUi = GameUI.getInstance();
        radiationLevel = 0;
        electricityLevel = 0;
        bioLevel = 0;
        gainEnergy(20);
    }

    public void OnGUI() {
        if (_currentCastType != ElementalCastType.NONE) {
            Vector2 mousePosition = Input.mousePosition;
            mousePosition.y = Screen.height - mousePosition.y;
            GUI.Box(new Rect(mousePosition.x - 8, mousePosition.y - 8, 16, 16), "");
        }

        if (currentSelectedSlime != null) {
            switch (_currentCastType) {
                case ElementalCastType.NONE:
                case ElementalCastType.BIO_OFFENSIVE:
                    doPathHighlight();
                    break;
                case ElementalCastType.ELECTRICITY_OFFENSIVE:
                    doLineHighlight();
                    break;
                case ElementalCastType.RADIATION_DEFENSIVE:
                    doCircleHighlight(getRadiationDefenceRadius(), getRadiationDefenceRange());
                    break;
                case ElementalCastType.RADIATION_OFFENSIVE:
                    doCircleHighlight(getRadiationOffenceRadius(), getRadiationOffenceRange());
                    break;
                default:
                    Debug.LogWarning("Cannot handle [" + _currentCastType + "] elementatl cast type");
                    break;
            }
        }
    }

    private void doLineHighlight() {
        Vector2 startWorld = getStartLocation();
        Vector2 endWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition); ;
        Vector2 delta = endWorld - startWorld;
        float distance = delta.magnitude;
        for (float d = distance; d >= 0.0f; d -= 1.0f) {
            Vector2 pos = startWorld + delta / distance * d;
            drawGuiTexture(pos, 0.5f, d > getElectricityOffenseRange() ? _redTexture : _greenTexture);
        }
    }

    private void doCircleHighlight(int radius, float range) {
        Vector2 castDelta = getStartLocation() - getCursorPosition();
        Texture2D _fillTexture = castDelta.magnitude > range ? _redTexture : _greenTexture;

        for (int dx = -radius; dx <= radius; dx++) {
            for (int dy = -radius; dy <= radius; dy++) {
                if (dx * dx + dy * dy <= radius * radius) {
                    drawGuiTexture(getCursorPosition() + new Vector2Int(dx, dy), 1.0f, _fillTexture);
                }
            }
        }
    }

    private void doPathHighlight() {
        if ((Input.GetAxis("Mouse X") == 0) && (Input.GetAxis("Mouse Y") == 0)) {
            _slimeHighlightPath = null;
            if (!Minimap.getInstance().isPositionInFogOfWar(getCursorPosition())) {

                if (_currentCastType == ElementalCastType.NONE) {
                    Astar.isWalkableFunction = Tile.isSlimeableFunction;
                    Astar.isNeighborWalkableFunction = Tile.isSlimeableFunction;
                } else if (_currentCastType == ElementalCastType.BIO_OFFENSIVE){
                    Astar.isWalkableFunction = Tile.isSpikeableFunction;
                    Astar.isNeighborWalkableFunction = Tile.isSpikeableFunction;
                } else {
                    Debug.LogWarning("Unexpected elemental cast type [" + _currentCastType + "]");
                }
                
                _slimeHighlightPath = Astar.findPath(getStartLocation(), getCursorPosition());
            }
        }

        if (_slimeHighlightPath != null) {

            Texture2D fillColor = null;
            if (_currentCastType == ElementalCastType.NONE) {
                float pathCost = Slime.getPathCost(_slimeHighlightPath);
                fillColor = pathCost > energy ? _redTexture : _greenTexture;
            } else {
                float pathCost = _slimeHighlightPath.getLength();
                fillColor = pathCost > getBioOffenseLength() ? _redTexture : _greenTexture;
            }

            for (int i = 0; i < _slimeHighlightPath.Count; i++) {
                drawGuiTexture(_slimeHighlightPath[i], 1.0f, fillColor);
            }
        }
    }

    private void drawGuiTexture(Vector2 position, float worldSize, Texture2D texture) {
        Vector2 nodePos = position;
        Vector2 widthPos = Camera.main.WorldToScreenPoint(nodePos + Vector2.right * worldSize);

        nodePos = Camera.main.WorldToScreenPoint(nodePos);
        float width = widthPos.x - nodePos.x;

        nodePos.y = Screen.height - nodePos.y;

        GUI.DrawTexture(new Rect(nodePos.x - width / 2, nodePos.y - width / 2, width, width), texture);
    }

    // Update is called once per frame
    void LateUpdate() {
        if (Input.GetKeyDown(KeyCode.E)) {
            gainEnergy(1000000);
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            gainRadiationLevel();
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            gainElectricityLevel();
        }
        if (Input.GetKeyDown(KeyCode.B)) {
            gainBioLevel();
        }

        if (_shouldSkipNext) {
            _shouldSkipNext = false;
        } else {
            if (_currentCastType == ElementalCastType.NONE) {
                handleNormalInteraction();
            } else {
                handleCastInteraction();
            }
        }
    }

    private void handleNormalInteraction() {
        if (Input.GetMouseButtonDown(0)){
            highlightSlimeTile();
        }

        if (currentSelectedSlime == null){
            renderer.enabled = false;
        }else{
            attemptToEat();
        }

        if (Input.GetMouseButtonDown(1) && currentSelectedSlime != null){
            if (energy > 0) {
                Astar.isWalkableFunction = Tile.isSlimeableFunction;
                Astar.isNeighborWalkableFunction = Tile.isSlimeableFunction;
                Path astarPath = Astar.findPath(getStartLocation(), getCursorPosition());
                if (astarPath != null) {
                    int pathCost = Slime.getPathCost(astarPath);
                    //if the slime has the energy to move, take the astar path
                    if (energy >= pathCost) {
                        loseEnergy(pathCost);
                        currentSelectedSlime.requestExpansionAllongPath(astarPath);
                        if (!audio.isPlaying) {
                            audio.clip = slimeExpansionSFX;
                            audio.Play();
                        }
                    }
                }
            } else {
                GameOver();
            }
        }
    }

    private void handleCastInteraction() {
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            _currentCastType = ElementalCastType.NONE;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            bool didCast = false;
            switch (_currentCastType) {
                case ElementalCastType.BIO_OFFENSIVE:
                    didCast = useBioOffense();
                    break;
                case ElementalCastType.ELECTRICITY_OFFENSIVE:
                    didCast = useElectricityOffense();
                    break;
                case ElementalCastType.RADIATION_DEFENSIVE:
                    didCast = useRadiationDefense();
                    break;
                case ElementalCastType.RADIATION_OFFENSIVE:
                    didCast = useRadiationOffense();
                    break;
                default:
                    _currentCastType = ElementalCastType.NONE;
                    throw new System.Exception("Unexpected elemental cast type " + _currentCastType);
            }
            if (didCast) {
                _currentCastType = ElementalCastType.NONE;
            }
        }
    }

    public void skipNextFrame() {
        _shouldSkipNext = true;
    }

    public void beginCast(ElementalCastType castType) {
        _currentCastType = castType;
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
            gainRadiationLevel();
        }
        if (eatenItem.isElectricityMutation) {
            gainElectricityLevel();
        }
        if (eatenItem.isBioMutation) {
            gainBioLevel();
        }

        Destroy(eatenItem.gameObject);

    }

    public void highlightSlimeTile() {
        Tile tileUnderCursor = getTileUnderCursor();
        //gets the slime component under the highlighted tile, if it exists
        if (tileUnderCursor != null) {
            Slime slimeTile = tileUnderCursor.GetComponent<Slime>();
            if (slimeTile != null && slimeTile.isConnected()) {
                setSelectedSlime(slimeTile);
            }
        }
    }

    public void setSelectedSlime(Slime slime) {
        currentSelectedSlime = slime;
        if (currentSelectedSlime == null) {
            renderer.enabled = false;
        } else {
			Eye_Animator.SetTrigger ("Blink");
        }
    }

    public Slime getSelectedSlime() {
        return currentSelectedSlime;
    }

    public Vector2Int getCursorPosition() {
        //finds the cursorPosition and then uses cursorPosition to find position of tileUnderCursor
		Camera testCam = Camera.main;
        return testCam.ScreenToWorldPoint(Input.mousePosition);
    }

    public Tile getTileUnderCursor() {
        return Tilemap.getInstance().getTile(getCursorPosition());
    }

    public Vector2Int getStartLocation() {
        return currentSelectedSlime.transform.position;
    }

    public void attemptToEat() {

        Tile tileComponent = currentSelectedSlime.GetComponent<Tile>();
        HashSet<TileEntity> entities = tileComponent.getTileEntities();
        if (entities != null) {
            foreach (TileEntity entity in entities) {
                GenericConsumeable possibleConsumeable = entity.GetComponent<GenericConsumeable>();
                if (possibleConsumeable != null) {
                    consume(possibleConsumeable);
                    gameObject.AddComponent<SoundEffect>().sfx = slimeEatingSFX;
                }
            }
        }
    }

    private void loseEnergy(int cost) {
        energy -= cost;
        _gameUi.ResourceUpdate(energy);
    }

    public void gainEnergy(int plus) {
        energy += plus;
        _gameUi.ResourceUpdate(energy);
    }

    private void GameOver() {
        PauseMenu gameover = _gameUi.GetComponent<PauseMenu>();
        gameover.GameOver();
    }

    public int getEnergyAmount() {
        return energy;
    }

    public void gainRadiationLevel() {
        radiationLevel++;
        _gameUi.RadiationUpdate(radiationLevel);
    }

    public void gainBioLevel() {
        bioLevel++;
        _gameUi.BioUpdate(bioLevel);
    }

    public void gainElectricityLevel() {
        electricityLevel++;
        _gameUi.LightningUpdate(electricityLevel);
    }

    /*###################################### ELEMENTAL SKILLS #######################################*/

    //Allows slime to irradiate tiles permanently so that enemies that walk into the area are stunned for short periods of time
    public bool useRadiationDefense() {
        float rangeOfAttack = getRadiationDefenceRange();
        //if distance is within range of attack, check each tile in the radius and then irradiate each tile that can be irradiated
        if (Vector2Int.distance(getStartLocation(), getCursorPosition()) <= rangeOfAttack) {
            int circleRadius = getRadiationDefenceRadius();

            gameObject.AddComponent<SoundEffect>().sfx = radioactiveDefenseSFX;
            for (int dx = -circleRadius; dx <= circleRadius; dx++) {
                for (int dy = -circleRadius; dy <= circleRadius; dy++) {
                    Vector2 tileOffset = new Vector2(dx, dy);
                    if (tileOffset.sqrMagnitude <= circleRadius * circleRadius) {
                        Tile tile = Tilemap.getInstance().getTile(getCursorPosition() + new Vector2Int(dx, dy));
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
            loseEnergy(RADIATION_DEFENSE_COST);
            return true;
        }
        return false;
    }

    private float getRadiationDefenceRange() {
        return RADIATION_BASE_RANGE * radiationLevel;
    }

    private int getRadiationDefenceRadius() {
        return 3 * radiationLevel;
    }

    //Allows slime to irradiate an area for a period of time such that enemies are damaged per second
    //Damage and range will increase based on level
    public bool useRadiationOffense() {
        float rangeOfAttack = getRadiationOffenceRange();
        //if distance is within range of attack, create the radius of radiation
        if (Vector2Int.distance(getStartLocation(), getCursorPosition()) <= rangeOfAttack) {
            gameObject.AddComponent<SoundEffect>().sfx = radioactiveOffenseSFX;
            int circleRadius = getRadiationOffenceRadius();
            for (int dx = -circleRadius; dx <= circleRadius; dx++) {
                for (int dy = -circleRadius; dy <= circleRadius; dy++) {
                    Vector2 tileOffset = new Vector2(dx, dy);
                    if (tileOffset.sqrMagnitude <= circleRadius * circleRadius) {
                        Tile tile = Tilemap.getInstance().getTile(getCursorPosition() + new Vector2Int(dx, dy));
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

            //AudioSource.PlayClipAtPoint(radioactiveOffenseSFX, getStartLocation(), 0.3f);
            loseEnergy(RADIATION_OFFENSE_COST);
            return true;
        }
        return false;
    }

    private float getRadiationOffenceRange() {
        return RADIATION_BASE_RANGE * radiationLevel;
    }

    private int getRadiationOffenceRadius() {
        return 3 * radiationLevel;
    }

    //outputs circle of enemy-damaging electricity from central point of selected slime tile
    //radius increases with electricityLevel, as does damage
    public void useElectricityDefense() {
        //AudioSource.PlayClipAtPoint(electricDefenseSFX, getStartLocation(), 0.2f);

        gameObject.AddComponent<SoundEffect>().sfx = electricDefenseSFX;
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
    public bool useElectricityOffense() {
        int damageDone = ELECTRICITY_BASE_DAMAGE * electricityLevel;
        float rangeOfAttack = getElectricityOffenseRange();
        //if enemy is within range of attack, use electricity
        if (Vector2Int.distance(getStartLocation(), getCursorPosition()) <= rangeOfAttack) {
            bool canDamage = getTileUnderCursor().canDamageEntities();

            //if an enemy was damaged, check to see if there are enemies close by to arc to
            if (canDamage) {
                loseEnergy(ELECTRICITY_OFFENSE_COST);
                GameObject electricityArc = new GameObject("ElectricityArc");
                electricityArc.transform.position = getStartLocation();
                ElectricityArc arc = electricityArc.AddComponent<ElectricityArc>();

                arc.setArcRadius(electricityLevel + 1);
                arc.setArcDamage(damageDone);
                arc.setArcNumber(electricityLevel + 1);
                arc.setDestination(getCursorPosition());
                return true;
            }
        }
        return false;
    }

    private float getElectricityOffenseRange() {
        return ELECTRICITY_BASE_RANGE * electricityLevel;
    }

    //outputs a circle of thick, high health slime from central point of selected slime tile
    //radius and health increases with bioLevel
    //defense will remain until destroyed by enemies
    public void useBioDefense() {
        //AudioSource.PlayClipAtPoint(bioDefenseSFX, getStartLocation(), 0.3f);

        gameObject.AddComponent<SoundEffect>().sfx = bioDefenseSFX;
        int circleRadius = bioLevel;
        for (int dx = -circleRadius; dx <= circleRadius; dx++) {
            for (int dy = -circleRadius; dy <= circleRadius; dy++) {
                Vector2 tileOffset = new Vector2(dx, dy);
                if (tileOffset.sqrMagnitude <= circleRadius * circleRadius) {
                    Tile tile = Tilemap.getInstance().getTile(getStartLocation() + new Vector2Int(dx, dy));
                    if (tile != null && tile.GetComponent<Slime>() != null && tile.GetComponent<BioMutated>() == null) {
                        tile.isWalkable = false;
                        tile.isSlimeable = true;
                        tile.isTransparent = true;
                        tile.gameObject.AddComponent<BioMutated>();
                    }
                }
            }
        }
        loseEnergy(BIO_DEFENSE_COST);
    }

    //Creates a tentacle that can stab and impale enemies, as well as drag them towards the slime at higher levels
    //Damage and range are based on level
    public bool useBioOffense() {
        int damageDone = BIO_BASE_DAMAGE * bioLevel;
        float rangeOfAttack = getBioOffenseLength();
        Astar.isWalkableFunction = Tile.isSpikeableFunction;
        Astar.isNeighborWalkableFunction = Tile.isSpikeableFunction;
        Path astarPath = Astar.findPath(getStartLocation(), getCursorPosition());

        if (astarPath != null && astarPath.getLength() <= rangeOfAttack) {
            GameObject bioLance = Instantiate(spinePrefab) as GameObject;
            bioLance.transform.position = getTileUnderCursor().transform.position;
            BioLance bio = bioLance.GetComponent<BioLance>();
            bio.setLancePath(astarPath);
            bio.setLanceDamage(damageDone);
            loseEnergy(BIO_OFFENSE_COST);

            gameObject.AddComponent<SoundEffect>().sfx = bioOffenseSFX;
            return true;
        }
        return false;
    }

    private float getBioOffenseLength() {
        return BIO_BASE_RANGE * bioLevel;
    }

    public float getElectricityLevel() {
        return electricityLevel;
    }
    public float getBioLevel() {
        return bioLevel;
    }

    public float getRadiationLevel() {
        return radiationLevel;
    }
	//Called at the end of the blink animation to move the eye to the new position and play the opening animation.
	public void EyeBlink(){
        if (currentSelectedSlime == null) {
            renderer.enabled = false;
            return;
        }
        transform.position = currentSelectedSlime.transform.position;
        renderer.enabled = true;
        Eye_Animator.SetTrigger ("ReverseBlink");
    }
}

