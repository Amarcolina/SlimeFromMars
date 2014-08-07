﻿using UnityEngine;
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
    //cost for using skills
    public const int ELECTRICITY_DEFENSE_COST = 5;
    public const int ELECTRICITY_OFFENSE_COST = 10;
    public const int BIO_DEFENSE_COST = 8;
    public const int BIO_OFFENSE_COST = 8;
    public const int RADIATION_DEFENSE_COST = 12;
    public const int RADIATION_OFFENSE_COST = 10;

    //energy is a pool of resources used to move, attack and defend
    public int energy;
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

    private ElementalCastType _currentCastType = ElementalCastType.NONE;
    private bool _shouldSkipNext = false;
    private Slime currentSelectedSlime;

    //The Animator for the eye, used to transfer states via triggers
    private Animator _eyeAnimator;

    //Resource UI related objects, necessary for checks
    private GameUI _gameUi;
    private UILabel _resourcedisplayLabel;
    private UISprite _resourcedisplaySprite;
    private GameObject _resourcedisplayGameObject;
    private bool _resourcesopen = false;
    private int _consumeradiation;
    private int _consumebio;
    private int _consumeelectricity;
    private int _consumesize;
    private string _consumename;

    //asdasd
    private Path _slimeHighlightPath = null;
    private Texture2D _edgeGreenHorizontal;
    private Texture2D _edgeGreenVertical;
    private Texture2D _edgeRedHorizontal;
    private Texture2D _edgeRedVertical;
    private Texture2D _pathDotRed;
    private Texture2D _pathDotGreen;
    private Texture2D _tileGreen;
    private Texture2D _tileRed;

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

        _edgeGreenHorizontal = Resources.Load<Texture2D>("Sprites/UISprites/Interface/BoundaryEdgeGreenHorizontal");
        _edgeGreenVertical = Resources.Load<Texture2D>("Sprites/UISprites/Interface/BoundaryEdgeGreenVertical");
        _edgeRedHorizontal = Resources.Load<Texture2D>("Sprites/UISprites/Interface/BoundaryEdgeRedHorizontal");
        _edgeRedVertical = Resources.Load<Texture2D>("Sprites/UISprites/Interface/BoundaryEdgeRedVertical");
        _pathDotGreen = Resources.Load<Texture2D>("Sprites/UISprites/Interface/PathDotGreen");
        _pathDotRed = Resources.Load<Texture2D>("Sprites/UISprites/Interface/PathDotRed");
        _tileGreen = Resources.Load<Texture2D>("Sprites/UISprites/Interface/TileGreen");
        _tileRed = Resources.Load<Texture2D>("Sprites/UISprites/Interface/TileRed");

        //Finds the Resource UI
        _eyeAnimator = GetComponent<Animator>();
        _resourcedisplayGameObject = GameObject.FindGameObjectWithTag("ItemInfo");
        _resourcedisplayLabel = _resourcedisplayGameObject.GetComponentInChildren<UILabel>();
        _resourcedisplaySprite = _resourcedisplayGameObject.GetComponentInChildren<UISprite>();
    }

    // Use this for initialization
    void Start() {
        _gameUi = GameUI.getInstance();
        radiationLevel = 0;
        electricityLevel = 0;
        bioLevel = 0;
        gainEnergy(20);
    }

    /*###############################################################################################*/
    /*###################################### MAIN SLIME LOGIC #######################################*/
    /*###############################################################################################*/

    public void skipNextFrame() {
        _shouldSkipNext = true;
    }

    public void beginCast(ElementalCastType castType) {
        _currentCastType = castType;
    }

    // Update is called once per frame
    public void LateUpdate() {
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
        if (Input.GetMouseButtonDown(0)) {
            if (_resourcesopen) {
                ResourceUIActivated();
            } else {
                RemoveResourceBox();
            }
            highlightSlimeTile();
        }

        if (currentSelectedSlime == null) {
            renderer.enabled = false;
        } else {
            attemptToEat();
        }

        if (Input.GetMouseButtonUp(1) && currentSelectedSlime != null) {
            RemoveResourceBox();
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
        RemoveResourceBox();
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

    private void consume(GenericConsumeable eatenItem) {
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

    private void highlightSlimeTile() {
        Tile tileUnderCursor = getTileUnderCursor();
        //gets the slime component under the highlighted tile, if it exists
        if (tileUnderCursor != null) {
            Slime slimeTile = tileUnderCursor.GetComponent<Slime>();
            if (slimeTile != null && slimeTile.isConnected()) {
                RemoveResourceBox();
                setSelectedSlime(slimeTile);
            }
        }
    }

    private void attemptToEat() {
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

    private void GameOver() {
        PauseMenu gameover = _gameUi.GetComponent<PauseMenu>();
        gameover.GameOver();
    }

    /*###############################################################################################*/
    /*####################################### INTERFACE CODE ########################################*/
    /*###############################################################################################*/

    public void OnGUI() {
        if (_currentCastType != ElementalCastType.NONE) {
            Vector2 mousePosition = Input.mousePosition;
            mousePosition.y = Screen.height - mousePosition.y;
            GUI.Box(new Rect(mousePosition.x - 8, mousePosition.y - 8, 16, 16), "");
        }

        if (currentSelectedSlime != null) {
            switch (_currentCastType) {
                case ElementalCastType.NONE:
                    if (Input.GetKey(KeyCode.Mouse1)) {
                        doPathHighlight();
                    } else {
                        _slimeHighlightPath = null;
                    }
                    break;
                case ElementalCastType.BIO_OFFENSIVE:
                    doPathHighlight();
                    break;
                case ElementalCastType.ELECTRICITY_OFFENSIVE:
                    doLineHighlight(getElectricityOffenseRange());
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

    private void doLineHighlight(float range) {
        Vector2 startWorld = getStartLocation();
        Vector2 endWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition); ;
        Vector2 delta = endWorld - startWorld;
        float distance = delta.magnitude;

        bool canCast = canCastToCursor(range);

        for (float d = distance; d >= 0.0f; d -= 1.0f) {
            Vector2 pos = startWorld + delta / distance * d;
            drawGuiTexture(pos, 0.5f, 0.5f, d <= range && canCast ? _pathDotGreen : _pathDotRed);
        }
    }

    private void doCircleHighlight(int radius, float range) {
        bool canCast = canCastToCursor(range);

        HashSet<Vector2Int> _positionsInCircle = new HashSet<Vector2Int>();
        TileCircleFunction circleFunction = delegate(Tile tile, Vector2Int position) {
            _positionsInCircle.Add(position);
        };
        forEveryTileInCircle(circleFunction, getCursorPosition(), radius, true);

        foreach (Vector2Int position in _positionsInCircle) {
            Vector2 worldPosition = position;
            drawGuiTexture(worldPosition, 1.0f, 1.0f, canCast ? _tileGreen : _tileRed);
            if (!_positionsInCircle.Contains(position + Vector2Int.up)) {
                drawGuiTexture(worldPosition + new Vector2(0, 0.5f), 1.0f, 0.2f, canCast ? _edgeGreenHorizontal : _edgeRedHorizontal);
            }
            if (!_positionsInCircle.Contains(position + Vector2Int.down)) {
                drawGuiTexture(worldPosition + new Vector2(0, -0.5f), 1.0f, 0.2f, canCast ? _edgeGreenHorizontal : _edgeRedHorizontal);
            }
            if (!_positionsInCircle.Contains(position + Vector2Int.right)) {
                drawGuiTexture(worldPosition + new Vector2(0.5f, 0), 0.2f, 1.0f, canCast ? _edgeGreenVertical : _edgeRedVertical);
            }
            if (!_positionsInCircle.Contains(position + Vector2Int.left)) {
                drawGuiTexture(worldPosition + new Vector2(-0.5f, 0), 0.2f, 1.0f, canCast ? _edgeGreenVertical : _edgeRedVertical);
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
                } else if (_currentCastType == ElementalCastType.BIO_OFFENSIVE) {
                    Astar.isWalkableFunction = Tile.isSpikeableFunction;
                    Astar.isNeighborWalkableFunction = Tile.isSpikeableFunction;
                } else {
                    Debug.LogWarning("Unexpected elemental cast type [" + _currentCastType + "]");
                }

                _slimeHighlightPath = Astar.findPath(getStartLocation(), getCursorPosition());
                if (_slimeHighlightPath != null) {
                    _slimeHighlightPath.removeNodeFromStart();
                }
            }
        }

        if (_slimeHighlightPath != null) {

            Texture2D fillColor = null;
            if (_currentCastType == ElementalCastType.NONE) {
                float pathCost = Slime.getPathCost(_slimeHighlightPath);
                fillColor = pathCost > energy ? _pathDotRed : _pathDotGreen;
            } else {
                float pathCost = _slimeHighlightPath.getLength();
                fillColor = pathCost > getBioOffenseLength() ? _pathDotRed : _pathDotGreen;
            }

            for (int i = 0; i < _slimeHighlightPath.Count; i++) {
                drawGuiTexture(_slimeHighlightPath[i], 1.0f, 1.0f, fillColor);
            }
        }
    }

    private void drawGuiTexture(Vector2 position, float worldWidth, float worldHeight, Texture2D texture) {
        Vector2 nodePos = position;
        Vector2 widthPos = Camera.main.WorldToScreenPoint(nodePos + Vector2.right * worldWidth);
        Vector2 heightPos = Camera.main.WorldToScreenPoint(nodePos + Vector2.up * worldHeight);

        nodePos = Camera.main.WorldToScreenPoint(nodePos);
        float width = widthPos.x - nodePos.x;
        float height = heightPos.y - nodePos.y;

        nodePos.y = Screen.height - nodePos.y;

        GUI.DrawTexture(new Rect(nodePos.x - width / 2, nodePos.y - height / 2, width, height), texture);
    }

    //Called at the end of the blink animation to move the eye to the new position and play the opening animation.
    public void EyeBlink() {
        if (currentSelectedSlime == null) {
            renderer.enabled = false;
            return;
        }
        transform.position = currentSelectedSlime.transform.position;
        renderer.enabled = true;
        _eyeAnimator.SetTrigger("ReverseBlink");
    }

    private void ResourceUIActivated() {
        int potentialenergy = _consumesize + (radiationLevel * _consumeradiation) + (electricityLevel * _consumeelectricity) + (bioLevel * _consumebio);
        _resourcedisplayLabel.text = _consumename + "\nRadiation:" + _consumeradiation + "\nBio:" + _consumebio + "\nElectricity:" + _consumeelectricity + "\nEnergy:" + potentialenergy;
        _resourcedisplayLabel.enabled = true;
        _resourcedisplaySprite.enabled = true;
        _resourcesopen = false;
    }

    //Called by the consumable that was clicked on to check request the slime controller to set up the UI
    public void ResourceUICheck(string name, int size, int bio, int radiation, int electricity) {
        _consumeradiation = radiation;
        _consumebio = bio;
        _consumeelectricity = electricity;
        _consumesize = size;
        _consumename = name;
        _resourcesopen = true;
    }

    private void RemoveResourceBox() {
        _resourcedisplayLabel.enabled = false;
        _resourcedisplaySprite.enabled = false;
    }

    /*###############################################################################################*/
    /*###################################### ELEMENTAL SKILLS #######################################*/
    /*###############################################################################################*/

    //Allows slime to irradiate tiles permanently so that enemies that walk into the area are stunned for short periods of time
    public bool useRadiationDefense() {
        //if distance is within range of attack, check each tile in the radius and then irradiate each tile that can be irradiated
        if (canCastToCursor(getRadiationDefenceRange())) {
            gameObject.AddComponent<SoundEffect>().sfx = radioactiveDefenseSFX;

            TileCircleFunction circleFunction = delegate(Tile tile, Vector2Int position) {
                Irradiated radComponent = tile.GetComponent<Irradiated>();
                if (radComponent == null) {
                    radComponent = tile.gameObject.AddComponent<Irradiated>();
                }
                radComponent.setStunned(true);
            };
            forEveryTileInCircle(circleFunction, getCursorPosition(), getRadiationDefenceRadius(), true);

            loseEnergy(RADIATION_DEFENSE_COST);
            return true;
        }
        return false;
    }

    private float getRadiationDefenceRange() {
        return 2 * radiationLevel + 3;
    }

    private int getRadiationDefenceRadius() {
        return radiationLevel + 1;
    }

    //Allows slime to irradiate an area for a period of time such that enemies are damaged per second
    //Damage and range will increase based on level
    public bool useRadiationOffense() {
        //if distance is within range of attack, create the radius of radiation
        if (canCastToCursor(getRadiationOffenceRange())) { 
            gameObject.AddComponent<SoundEffect>().sfx = radioactiveOffenseSFX;

            TileCircleFunction circleFunction = delegate(Tile tile, Vector2Int position) {
                Irradiated radComponent = tile.GetComponent<Irradiated>();
                if (radComponent == null) {
                    radComponent = tile.gameObject.AddComponent<Irradiated>();
                }
                radComponent.setDamaged(true);
            };
            forEveryTileInCircle(circleFunction, getCursorPosition(), getRadiationOffenceRadius(), true);

            loseEnergy(RADIATION_OFFENSE_COST);
            return true;
        }
        return false;
    }

    private float getRadiationOffenceRange() {
        return 3 * radiationLevel + 2;
    }

    private int getRadiationOffenceRadius() {
        return radiationLevel + 1;
    }

    //outputs circle of enemy-damaging electricity from central point of selected slime tile
    //radius increases with electricityLevel, as does damage
    public void useElectricityDefense() {
        gameObject.AddComponent<SoundEffect>().sfx = electricDefenseSFX;

        TileCircleFunction circleFunction = delegate(Tile tile, Vector2Int tilePosition) {
            if (tile.GetComponent<Slime>() != null) {
                Electrified electrified = tile.gameObject.AddComponent<Electrified>();
                electrified.setDamage(getElectricityDefenseDamage());
            }
        };
        forEveryTileInCircle(circleFunction, getStartLocation(), getElectricityDefenceRadius(), true);

        loseEnergy(ELECTRICITY_DEFENSE_COST);
    }

    private int getElectricityDefenceRadius() {
        return electricityLevel;
    }

    private float getElectricityDefenseDamage() {
        return electricityLevel * 0.2f + 0.2f;
    }

    //sends a bolt of electricity at an enemy, up to a max distance away, and ars to nearby enemies for chain damage
    //damage and range increase with electricityLevel
    public bool useElectricityOffense() {
        if (canCastToCursor(getElectricityOffenseRange())) {
            if (getTileUnderCursor().canDamageEntities()) {
                loseEnergy(ELECTRICITY_OFFENSE_COST);
                GameObject electricityArc = new GameObject("ElectricityArc");
                electricityArc.transform.position = getStartLocation();
                ElectricityArc arc = electricityArc.AddComponent<ElectricityArc>();

                arc.setArcRadius(electricityLevel + 1);
                arc.setArcDamage(getElectricityOffenseDamage());
                arc.setArcNumber(electricityLevel + 1);
                arc.setDestination(getCursorPosition());
                return true;
            }
        }
        return false;
    }

    private float getElectricityOffenseRange() {
        return 3 * electricityLevel + 2;
    }

    private float getElectricityOffenseDamage() {
        return 0.15f;
    }

    //outputs a circle of thick, high health slime from central point of selected slime tile
    //radius and health increases with bioLevel
    //defense will remain until destroyed by enemies
    public void useBioDefense() {
        gameObject.AddComponent<SoundEffect>().sfx = bioDefenseSFX;
        TileCircleFunction circleFuction = delegate(Tile tile, Vector2Int position){
            if (tile.GetComponent<Slime>() != null && tile.GetComponent<BioMutated>() == null) {
                tile.isWalkable = false;
                tile.isSlimeable = true;
                tile.isTransparent = true;
                tile.gameObject.AddComponent<BioMutated>();
            }
        };
        forEveryTileInCircle(circleFuction, getStartLocation(), getBioDefenceRadius(), true);
        loseEnergy(BIO_DEFENSE_COST);
    }

    private int getBioDefenceRadius() {
        return bioLevel;
    }

    //Creates a tentacle that can stab and impale enemies, as well as drag them towards the slime at higher levels
    //Damage and range are based on level
    public bool useBioOffense() {
        float rangeOfAttack = getBioOffenseLength();
        Astar.isWalkableFunction = Tile.isSpikeableFunction;
        Astar.isNeighborWalkableFunction = Tile.isSpikeableFunction;
        Path astarPath = Astar.findPath(getStartLocation(), getCursorPosition());

        if (astarPath != null && astarPath.getLength() <= rangeOfAttack) {
            GameObject bioLance = Instantiate(spinePrefab) as GameObject;
            bioLance.transform.position = getTileUnderCursor().transform.position;
            BioLance bio = bioLance.GetComponent<BioLance>();
            bio.setLancePath(astarPath);
            bio.setLanceDamage(getBioOffenseDamage());
            loseEnergy(BIO_OFFENSE_COST);

            gameObject.AddComponent<SoundEffect>().sfx = bioOffenseSFX;
            return true;
        }
        return false;
    }

    private float getBioOffenseLength() {
        return 3 * bioLevel + 3;
    }

    private float getBioOffenseDamage() {
        return bioLevel * 0.21f;
    }

    private bool canCastToCursor(float castRange) {
        Vector2Int delta = getStartLocation() - getCursorPosition();
        if (delta.getLengthSqrd() <= castRange * castRange) {
            if (TilemapUtilities.canSee(getStartLocation(), getCursorPosition())) {
                return true;
            }
        }
        return false;
    }

    private delegate void TileCircleFunction(Tile tile, Vector2Int tilePosition);
    private void forEveryTileInCircle(TileCircleFunction function, Vector2Int center, int radius, bool lineOfSight) {
        for (int dx = -radius; dx <= radius; dx++) {
            for (int dy = -radius; dy <= radius; dy++) {
                if ((dx * dx + dy * dy) <= radius * radius) {
                    Vector2Int tilePosition = center + new Vector2Int(dx, dy);
                    if (TilemapUtilities.canSee(center, tilePosition)) {
                        function(Tilemap.getInstance().getTile(tilePosition), tilePosition);
                    }
                }
            }
        }
    }
    
    

    /*###############################################################################################*/
    /*#################################### SETTERS AND GETTERS ######################################*/
    /*###############################################################################################*/

    public Slime getSelectedSlime() {
        return currentSelectedSlime;
    }

    public void setSelectedSlime(Slime slime) {
        Slime previousSlime = currentSelectedSlime;
        currentSelectedSlime = slime;

        if (currentSelectedSlime == null) {
            renderer.enabled = false;
        } else {
            if (previousSlime == null) {
                EyeBlink();
            } else {
                _eyeAnimator.SetTrigger("Blink");
            }
        }
    }

    public int getEnergyAmount() {
        return energy;
    }

    public void gainEnergy(int plus) {
        energy += plus;
        if (plus != 0) {
            _gameUi.ResourceUpdate(energy);
        }
    }

    private void loseEnergy(int cost) {
        energy -= cost;
        if (cost != 0) {
            _gameUi.ResourceUpdate(energy);
        }
    }

    public float getBioLevel() {
        return bioLevel;
    }

    public void gainBioLevel() {
        bioLevel++;
        _gameUi.BioUpdate(bioLevel);
    }

    public float getElectricityLevel() {
        return electricityLevel;
    }

    public void gainElectricityLevel() {
        electricityLevel++;
        _gameUi.LightningUpdate(electricityLevel);
    }

    public float getRadiationLevel() {
        return radiationLevel;
    }

    public void gainRadiationLevel() {
        radiationLevel++;
        _gameUi.RadiationUpdate(radiationLevel);
    }

    public Vector2Int getCursorPosition() {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public Tile getTileUnderCursor() {
        return Tilemap.getInstance().getTile(getCursorPosition());
    }

    public Vector2Int getStartLocation() {
        return currentSelectedSlime.transform.position;
    }
}

