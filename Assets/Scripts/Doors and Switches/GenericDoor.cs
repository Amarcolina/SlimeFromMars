using UnityEngine;
using System.Collections;

public class GenericDoor : MonoBehaviour, ISaveable {

    public bool doorOpen = false;
    private float timer;
    private int doorAnimationState;
    //Tile variables
    protected Tile[] tilesUnderDoor = new Tile[6];
    protected Tile[] enemyEntranceTiles = new Tile[10];

    //Sprite variables
    SpriteRenderer genericSpriteComponent;
    protected Sprite[] openDoorSprites = new Sprite[5];
    protected Sprite[] closeDoorSprites = new Sprite[5];

    //Audio variables
    private SoundManager sound;
    private AudioClip openDoorSFX;
    private AudioClip closeDoorSFX;
    private AudioSource source;

    void Awake() {
        sound = SoundManager.getInstance();
        openDoorSFX = Resources.Load<AudioClip>("Sounds/SFX/door_open");
        closeDoorSFX = Resources.Load<AudioClip>("Sounds/SFX/door_close");
        genericSpriteComponent = GetComponent<SpriteRenderer>();
        doorAnimationState = 0;
        timer = 5;
        //Loads door sprites for animation
        for (int i = 0; i < 5; i++) {
            openDoorSprites[i] = Resources.Load<Sprite>("Sprites/Doors and Switches/Sliding Door/openDoors" + i);
            closeDoorSprites[i] = Resources.Load<Sprite>("Sprites/Doors and Switches/Sliding Door/closedDoors" + i);
        }
    }

    // Use this for initialization
    public virtual void Start() {
        //Gets the offset of each tile under the door and stores them in an array for ease of use, later
        tilesUnderDoor[0] = Tilemap.getInstance().getTile(transform.position + new Vector3(.5f, 0f, 0));
        tilesUnderDoor[1] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, 0f, 0));
        tilesUnderDoor[2] = Tilemap.getInstance().getTile(transform.position + new Vector3(.5f, 1f, 0));
        tilesUnderDoor[3] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, 1f, 0));
        tilesUnderDoor[4] = Tilemap.getInstance().getTile(transform.position + new Vector3(.5f, -1f, 0));
        tilesUnderDoor[5] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, -1f, 0));

        //Gets tiles above door and below door in order to check for enemies that may desire entrance
        enemyEntranceTiles[0] = Tilemap.getInstance().getTile(transform.position + new Vector3(.5f, 2f));
        enemyEntranceTiles[1] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, 2f));
        enemyEntranceTiles[2] = Tilemap.getInstance().getTile(transform.position + new Vector3(.5f, 0f, 0));
        enemyEntranceTiles[3] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, 0f, 0));
        enemyEntranceTiles[4] = Tilemap.getInstance().getTile(transform.position + new Vector3(.5f, 1f, 0));
        enemyEntranceTiles[5] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, 1f, 0));
        enemyEntranceTiles[6] = Tilemap.getInstance().getTile(transform.position + new Vector3(.5f, -1f, 0));
        enemyEntranceTiles[7] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, -1f, 0));
        enemyEntranceTiles[8] = Tilemap.getInstance().getTile(transform.position + new Vector3(.5f, -2f));
        enemyEntranceTiles[9] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, -2f));

        if (!doorOpen) {
            doorClosedState();
        }
    }

    // Update is called once per frame
    void Update() {
        if (safeToClose() && doorOpen && !enemyDesiresEntrance()) {
            doorClosedState();
            sound.PlaySound(gameObject.transform, closeDoorSFX, true);
        }

        if (!doorOpen) {
            genericSpriteComponent.sprite = closeDoorSprites[doorAnimationState];
            if (doorAnimationState > 0) {
                doorAnimationState--;
            }
        }

        if (enemyDesiresEntrance() && !doorOpen) {
            doorOpenedState();
            sound.PlaySound(gameObject.transform, openDoorSFX, true);
        }

        if (doorOpen) {
            genericSpriteComponent.sprite = openDoorSprites[doorAnimationState];
            if (doorAnimationState < 4) {
                doorAnimationState++;
            }
        }
    }

    public virtual void doorOpenedState() {
        //play opening animation and stop on open frame
        doorOpen = true;
        //set tiles to passable
        for (int i = 0; i <= 5; i++) {
            tilesUnderDoor[i].isSlimeable = true;
            tilesUnderDoor[i].isSpikeable = true;
            tilesUnderDoor[i].isTransparent = true;
        }

    }

    public virtual void doorClosedState() {
        //play closing animation and stop on closed frame
        // renderer.enabled = true;
        doorOpen = false;
        //set tiles to unpassable
        for (int i = 0; i <= 5; i++) {
            tilesUnderDoor[i].isSlimeable = false;
            tilesUnderDoor[i].isSpikeable = false;
            tilesUnderDoor[i].isTransparent = false;
        }
    }

    public virtual bool safeToClose() {
        for (int i = 0; i <= 5; i++) {
            if (tilesUnderDoor[i].getTileEntities() != null || tilesUnderDoor[i].GetComponent<Slime>() != null) {
                return false;
            }
        }
        return true;
    }

    public bool enemyDesiresEntrance() {

        for (int i = 0; i <= 9; i++) {
            if (enemyEntranceTiles[i].getTileEntities() != null) {
                return true;
            }
        }
        return false;
    }

    public void onSave(SavedComponent data) {
        data.put(doorOpen);
    }
    public void onLoad(SavedComponent data) {
        doorOpen = (bool)data.get();
    }
}
