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
public class SlimeController : MonoBehaviour, ISaveable {
    //cost for using skills
    public const int ELECTRICITY_DEFENSE_COST = 5;
    public const int ELECTRICITY_OFFENSE_COST = 10;
    public const int BIO_DEFENSE_COST = 8;
    public const int BIO_OFFENSE_COST = 8;
    public const int RADIATION_DEFENSE_COST = 12;
    public const int RADIATION_OFFENSE_COST = 10;

    //energy is a pool of resources used to move, attack and defend
    public int energy = 20;
    public GameObject spinePrefab;

    //levels dictate how much more powerful your attacks/defenses are
    //levels also give bonuses in energy from items of that attribute
    private int _radiationLevel = 0;
    private int _electricityLevel = 0;
    private int _bioLevel = 0;

    //list of sound effects for abilities
    private AudioClip _electricDefenseSFX;
    private AudioClip _bioDefenseSFX;
    private AudioClip _bioOffenseSFX;
    private AudioClip _radioactiveDefenseSFX;
    private AudioClip _radioactiveOffenseSFX;
    private AudioClip _slimeExpansionSFX;
    private AudioClip _slimeEatingSFX;
    private AudioClip _slimeEatingRadioActivateSFX;
    private AudioClip _slimeEatingBioSFX;
    private AudioClip _slimeEatingElectricitySFX;

    public ElementalCastType _currentCastType = ElementalCastType.NONE;
    private bool _shouldSkipNext = false;
    private bool isSlimeMoving;
    private bool didSlimeEat;
    private bool usingBioLance;
    private Slime _currentSelectedSlime;

    //The Animator for the eye, used to transfer states via triggers
    private Animator _eyeAnimator;

    //Resource UI related objects, necessary for checks
    private GameUI _gameUi;
    private UILabel _resourcedisplayLabel;
    private UISprite _resourcedisplaySprite;
    private GameObject _resourcedisplayGameObject;

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

    private SoundManager sound;

    private static SlimeController _instance = null;
    public static SlimeController getInstance() {
        if (_instance == null) {
            _instance = FindObjectOfType<SlimeController>();
        }
        return _instance;
    }

    void Awake() {
        //Load all sounds from File
        _electricDefenseSFX = Resources.Load<AudioClip>("Sounds/SFX/electricity_defense");
        _bioDefenseSFX = Resources.Load<AudioClip>("Sounds/SFX/bio_defense");
        _bioOffenseSFX = Resources.Load<AudioClip>("Sounds/SFX/bio_offense_impale");
        _radioactiveDefenseSFX = Resources.Load<AudioClip>("Sounds/SFX/radiation_defense");
        _radioactiveOffenseSFX = Resources.Load<AudioClip>("Sounds/SFX/radiation_offense");
        _slimeExpansionSFX = Resources.Load<AudioClip>("Sounds/SFX/slime_expanding");
        _slimeEatingSFX = Resources.Load<AudioClip>("Sounds/SFX/slime_eating");
        _slimeEatingElectricitySFX = Resources.Load<AudioClip>("Sounds/SFX/electric_mutation");
        _slimeEatingRadioActivateSFX = Resources.Load<AudioClip>("Sounds/SFX/radiation_mutation");
        _slimeEatingBioSFX = Resources.Load<AudioClip>("Sounds/SFX/bio_mutation");

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

        sound = SoundManager.getInstance();
        _gameUi = GameUI.getInstance();
        _gameUi.ResourceUpdate(energy, false);
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
        if (Input.GetKeyDown(KeyCode.T)) {
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

        if (_currentSelectedSlime != null) {
            attemptToEat();
        }
        if(energy <= 0){
            StartCoroutine(GameOverCoroutine());
        }
        if (_shouldSkipNext) {
            _shouldSkipNext = Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1);
        } else {
            if (_currentCastType == ElementalCastType.NONE) {
                handleNormalInteraction();
            } else {
                handleCastInteraction();
            }
        }
    }

    IEnumerator GameOverCoroutine()
    {
        //If slime did not eat or move at 0 energy
        if (!didSlimeEat || !isSlimeMoving)
        {
            //Wait to check next frame if we land on resource
            yield return new WaitForSeconds(0.5f);
            if (energy <= 0)
            {
                //If we did not lance
                if (!usingBioLance)
                {
                    GameOver();
                }
                else
                {
                    //Otherwise wait to see if we used the lance to grab something
                    yield return new WaitForSeconds(3.0f);
                }
                if (energy <= 0)
                {
                    GameOver();
                    if(Analytics.Enabled){
                        GA.API.Design.NewEvent("Game Over", getCursorPosition());
                    }
                }
            }
        }
    }

    private void handleNormalInteraction() {
        if (Input.GetMouseButtonDown(0)) {
            disableResourcePopup();
            highlightSlimeTile();
        }

        if (Input.GetMouseButtonUp(1) && _currentSelectedSlime != null) {
            disableResourcePopup();
            if (energy > 0) {
                AstarSettings settings = new AstarSettings();
                settings.isWalkableFunction = Tile.isSlimeableFunction;
                settings.isNeighborWalkableFunction = Tile.isSlimeableFunction;
                Path astarPath = Astar.findPath(getStartLocation(), getCursorPosition(), settings);
                if (astarPath != null) {
                    int pathCost = Slime.getPathCost(astarPath);
                    //if the slime has the energy to move, take the astar path
                    if (energy >= pathCost)
                    {
                        isSlimeMoving = true;
                        usingBioLance = false;
                        loseEnergy(pathCost);
                        _currentSelectedSlime.requestExpansionAllongPath(astarPath);
                        sound.PlaySound(gameObject.transform, _slimeExpansionSFX, true);
                    }
                }
            }
        }
        isSlimeMoving = false;
    }

    private void handleCastInteraction() {
        disableResourcePopup();
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            _currentCastType = ElementalCastType.NONE;
            skipNextFrame();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            switch (_currentCastType) {
                case ElementalCastType.BIO_OFFENSIVE:
                    if (_gameUi.checkCanCastAbility(BIO_OFFENSE_COST)) {
                        useBioOffense();
                    }
                    break;
                case ElementalCastType.ELECTRICITY_OFFENSIVE:
                    if (_gameUi.checkCanCastAbility(ELECTRICITY_OFFENSE_COST)) {
                        useElectricityOffense();
                    }
                    break;
                case ElementalCastType.RADIATION_DEFENSIVE:
                    if (_gameUi.checkCanCastAbility(RADIATION_DEFENSE_COST)) {
                        useRadiationDefense();
                    }
                    
                    break;
                case ElementalCastType.RADIATION_OFFENSIVE:
                    if (_gameUi.checkCanCastAbility(RADIATION_OFFENSE_COST)) {
                        useRadiationOffense();
                    }
                    break;
                default:
                    _currentCastType = ElementalCastType.NONE;
                    throw new System.Exception("Unexpected elemental cast type " + _currentCastType);
            }
        }
    }

    private void consume(GenericConsumeable eatenItem) {
        //calculates resource bonus from item element affinity multiplied by level of slime attribute
        //calculates default item resource value based on size and adds any bonuses
        gainEnergy((int)eatenItem.size + _radiationLevel * eatenItem.radiation + _bioLevel * eatenItem.bio + _electricityLevel * eatenItem.electricity);
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
                disableResourcePopup();
                setSelectedSlime(slimeTile);
            }
        }
    }

    private void attemptToEat() {
        Tile tileComponent = _currentSelectedSlime.GetComponent<Tile>();
        HashSet<TileEntity> entities = tileComponent.getTileEntities();
        if (entities != null) {
            foreach (TileEntity entity in entities) {
                GenericConsumeable possibleConsumeable = entity.GetComponent<GenericConsumeable>();
                if (possibleConsumeable != null)
                {
                    consume(possibleConsumeable);
                    didSlimeEat = true;
                }
                else
                {
                    didSlimeEat = false;
                }
            }
        }
    }

    private void GameOver() {
        
            PauseMenu gameover = _gameUi.GetComponent<PauseMenu>();
            gameover.GameOver();
        
    }

    public void onSave(SavedComponent data) {
        data.put(energy);
        data.put(_bioLevel);
        data.put(_electricityLevel);
        data.put(_radiationLevel);
    }

    public void onLoad(SavedComponent data) {
        energy = (int)data.get();
        _gameUi.ResourceUpdate(energy, false);

        _bioLevel = (int)data.get();
        _electricityLevel = (int)data.get();
        _radiationLevel = (int)data.get();

        if (_bioLevel != 0) {
            _gameUi.BioUpdate(_bioLevel, false);
        }
        if (_electricityLevel != 0) {
            _gameUi.LightningUpdate(_electricityLevel, false);
        }
        if (_radiationLevel != 0) {
            _gameUi.RadiationUpdate(_radiationLevel, false);
        }
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

        if (_currentSelectedSlime != null) {
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
                    doLineHighlight(getRadiationDefenceRange());
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

                AstarSettings settings = new AstarSettings();
                if (_currentCastType == ElementalCastType.NONE) {
                    settings.isWalkableFunction = Tile.isSlimeableFunction;
                    settings.isNeighborWalkableFunction = Tile.isSlimeableFunction;
                } else if (_currentCastType == ElementalCastType.BIO_OFFENSIVE) {
                    settings.isWalkableFunction = Tile.isSpikeableFunction;
                    settings.isNeighborWalkableFunction = Tile.isSpikeableFunction;
                } else {
                    Debug.LogWarning("Unexpected elemental cast type [" + _currentCastType + "]");
                }

                _slimeHighlightPath = Astar.findPath(getStartLocation(), getCursorPosition(), settings);
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
    //Called automatically by the Animator
    public void EyeBlink() {
        if (_currentSelectedSlime == null) {
            renderer.enabled = false;
            return;
        }
        transform.position = _currentSelectedSlime.transform.position;
        renderer.enabled = true;
        _eyeAnimator.SetTrigger("ReverseBlink");
    }

    public void enableResourcePopup(string name, int size, int bio, int radiation, int electricity, bool ismutation) {
 
        //Don't do popup if in cast mode
        if (_currentCastType != ElementalCastType.NONE) {
            return;
        }

        skipNextFrame();
            
        if (ismutation) {
           _resourcedisplayLabel.color = Color.green;
        } else {
           _resourcedisplayLabel.color = Color.white;   
        }   
         int potentialenergy = size + (_radiationLevel * radiation) + (_electricityLevel * electricity) + (_bioLevel * bio);
        _resourcedisplayLabel.text = name + "\nRadiation:" + radiation + "\nBio:" + bio + "\nElectricity:" + electricity + "\nEnergy:" + potentialenergy;
        _resourcedisplayLabel.enabled = true;
        _resourcedisplaySprite.enabled = true;
    }

    private void disableResourcePopup() {
        _resourcedisplayLabel.enabled = false;
        _resourcedisplaySprite.enabled = false;
    }

    /*###############################################################################################*/
    /*###################################### ELEMENTAL SKILLS #######################################*/
    /*###############################################################################################*/

    //Allows slime to irradiate tiles permanently so that enemies that walk into the area are stunned for short periods of time
    public bool useRadiationDefense()
    {
        //if distance is within range of attack, check each tile in the radius and then irradiate each tile that can be irradiated
        if (canCastToCursor(getRadiationDefenceRange()))
        {
            sound.PlaySound(gameObject.transform, _radioactiveDefenseSFX);
            TileCircleFunction circleFunction = delegate(Tile tile, Vector2Int position)
            {
                Irradiated radComponent = tile.GetComponent<Irradiated>();
                if (radComponent == null)
                {
                    if(Analytics.Enabled){
                        GA.API.Design.NewEvent("Radiation Defensive", getCursorPosition());
                    }
					radComponent = tile.gameObject.AddComponent<Irradiated>();
                }
            };
            forEveryTileInCircle(circleFunction, getCursorPosition(), getRadiationDefenceRadius(), true);

            loseEnergy(RADIATION_DEFENSE_COST);
            return true;
        }
        return false;
    }

    private float getRadiationDefenceRange() {
        return 2 * _radiationLevel + 3;
    }

    private int getRadiationDefenceRadius() {
        return _radiationLevel + 1;
    }

    //Allows slime to irradiate an area for a period of time such that enemies are damaged per second
    //Damage and range will increase based on level
    public bool useRadiationOffense() {
        if (canCastToCursor(getRadiationOffenceRange())) {
            HashSet<TileEntity> damageableEntities = getTileUnderCursor().getTileEntities();
            bool didCast = false;
            if (damageableEntities != null) {
                foreach (TileEntity entity in damageableEntities) {
                    BaseEnemy enemy = entity.GetComponent<BaseEnemy>();
                    if (enemy != null) {
                        didCast = true;
                        if(Analytics.Enabled){
                            GA.API.Design.NewEvent("Radiation Offensive", getCursorPosition());
                        }
						enemy.gameObject.AddComponent<RadiationLeech>();
                    }
                }
            }

            if (didCast) {
                sound.PlaySound(transform, _radioactiveOffenseSFX);
                loseEnergy(RADIATION_OFFENSE_COST);
                return true;
            }
        }
        return false;
    }

    private float getRadiationOffenceRange() {
        return 3 * _radiationLevel + 2;
    }

    private int getRadiationOffenceRadius() {
        return _radiationLevel + 1;
    }

    //outputs circle of enemy-damaging electricity from central point of selected slime tile
    //radius increases with electricityLevel, as does damage
    public void useElectricityDefense() {
        sound.PlaySound(gameObject.transform, _electricDefenseSFX);

        TileCircleFunction circleFunction = delegate(Tile tile, Vector2Int tilePosition) {
            if (tile.GetComponent<Slime>() != null) {
                Electrified electrified = tile.gameObject.AddComponent<Electrified>();
                electrified.setDamage(getElectricityDefenseDamage());
                if(Analytics.Enabled){
                    GA.API.Design.NewEvent("Electricity Defensive", gameObject.transform.position);
                }
            }
        };
        forEveryTileInCircle(circleFunction, getStartLocation(), getElectricityDefenceRadius(), true);

        loseEnergy(ELECTRICITY_DEFENSE_COST);
    }

    private int getElectricityDefenceRadius() {
        return _electricityLevel;
    }

    private float getElectricityDefenseDamage() {
        return _electricityLevel * 0.2f + 0.2f;
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
                if(Analytics.Enabled){
                    GA.API.Design.NewEvent("Electricity Offensive", getCursorPosition());
                }
                arc.setArcRadius(_electricityLevel + 1);
                arc.setArcDamage(getElectricityOffenseDamage());
                arc.setArcNumber(_electricityLevel + 1);
                arc.setDestination(getCursorPosition());
                return true;
            }
        }
        return false;
    }

    private float getElectricityOffenseRange() {
        return 3 * _electricityLevel + 2;
    }

    private float getElectricityOffenseDamage() {
        return 0.15f;
    }

    //outputs a circle of thick, high health slime from central point of selected slime tile
    //radius and health increases with bioLevel
    //defense will remain until destroyed by enemies
    public void useBioDefense() {
        sound.PlaySound(gameObject.transform, _bioDefenseSFX);
        TileCircleFunction circleFuction = delegate(Tile tile, Vector2Int position){
            if (tile.GetComponent<Slime>() != null && tile.GetComponent<BioMutated>() == null) {
                tile.isWalkable = false;
                tile.isSlimeable = true;
                tile.isTransparent = true;
                tile.gameObject.AddComponent<BioMutated>();
                if(Analytics.Enabled){
                    GA.API.Design.NewEvent("Bio Defensive", gameObject.transform.position);
                }
            }
        };
        forEveryTileInCircle(circleFuction, getStartLocation(), getBioDefenceRadius(), true);
        loseEnergy(BIO_DEFENSE_COST);
    }

    private int getBioDefenceRadius() {
        return _bioLevel;
    }

    //Creates a tentacle that can stab and impale enemies, as well as drag them towards the slime at higher levels
    //Damage and range are based on level
    public bool useBioOffense() {
        float rangeOfAttack = getBioOffenseLength();
        AstarSettings settings = new AstarSettings();
        settings.isWalkableFunction = Tile.isSpikeableFunction;
        settings.isNeighborWalkableFunction = Tile.isSpikeableFunction;
        Path astarPath = Astar.findPath(getStartLocation(), getCursorPosition(), settings);
        if (astarPath != null && astarPath.getLength() <= rangeOfAttack) {
            GameObject bioLance = Instantiate(spinePrefab, getCursorPosition(), Quaternion.identity) as GameObject;
            BioLance bio = bioLance.GetComponent<BioLance>();
            bio.setLancePath(astarPath);
            bio.setLanceDamage(getBioOffenseDamage());
            usingBioLance = true;
            loseEnergy(BIO_OFFENSE_COST);
            sound.PlaySound(gameObject.transform, _bioOffenseSFX);
            if(Analytics.Enabled){
                GA.API.Design.NewEvent("Bio Offensive", getCursorPosition());
            }
            return true;
        }
        usingBioLance = false;
        return false;
    }

    private float getBioOffenseLength() {
        return 3 * _bioLevel + 3;
    }

    private float getBioOffenseDamage() {
        return _bioLevel * 0.21f;
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
        return _currentSelectedSlime;
    }

    public void setSelectedSlime(Slime slime) {
        Slime previousSlime = _currentSelectedSlime;
        _currentSelectedSlime = slime;

        if (_currentSelectedSlime == null) {
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
        sound.PlaySound(gameObject.transform, _slimeEatingSFX);
        energy += plus;
        if (plus != 0) {
            bool tween = true;
            _gameUi.ResourceUpdate(energy, tween);
        }
    }

    private void loseEnergy(int cost) {
        energy -= cost;
        if (cost != 0) {
            bool tween = false;
            _gameUi.ResourceUpdate(energy, tween);
        }
    }

    public float getBioLevel() {
        return _bioLevel;
    }

    public void gainBioLevel() {
        if(Analytics.Enabled){
            GA.API.Design.NewEvent("Bio Levels Gained", gameObject.transform.position);
        }
        sound.PlaySound(gameObject.transform, _slimeEatingBioSFX);
        _bioLevel++;
        _gameUi.BioUpdate(_bioLevel);
    }

    public float getElectricityLevel() {
        return _electricityLevel;
    }

    public void gainElectricityLevel() {
        if(Analytics.Enabled){
            GA.API.Design.NewEvent("Electricity Levels Gained", gameObject.transform.position);
        }
        sound.PlaySound(gameObject.transform, _slimeEatingElectricitySFX);
        _electricityLevel++;
        _gameUi.LightningUpdate(_electricityLevel);
    }

    public float getRadiationLevel() {
        return _radiationLevel;
    }

    public void gainRadiationLevel() {
        if(Analytics.Enabled){
            GA.API.Design.NewEvent("Radiation Levels Gained", gameObject.transform.position);
        }
        sound.PlaySound(gameObject.transform, _slimeEatingRadioActivateSFX);
        _radiationLevel++;
        _gameUi.RadiationUpdate(_radiationLevel);
    }

    public Vector2Int getCursorPosition() {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public Tile getTileUnderCursor() {
        return Tilemap.getInstance().getTile(getCursorPosition());
    }

    public Vector2Int getStartLocation() {
        return _currentSelectedSlime.transform.position;
    }
}
