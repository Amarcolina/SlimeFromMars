using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour {

    public bool doorOpen = false;
    private Tile[] tilesUnderDoor = new Tile[6];
    // Use this for initialization
    void Start() {
        //Gets the offset of each tile under the door and stores them in an array for ease of use, later
        tilesUnderDoor[0] = Tilemap.getInstance().getTile(transform.position + new Vector3(.5f, 0f, 0));
        tilesUnderDoor[1] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, 0f, 0));
        tilesUnderDoor[2] = Tilemap.getInstance().getTile(transform.position + new Vector3(.5f, 1f, 0));
        tilesUnderDoor[3] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, 1f, 0));
        tilesUnderDoor[4] = Tilemap.getInstance().getTile(transform.position + new Vector3(.5f, -1f, 0));
        tilesUnderDoor[5] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, -1f, 0));

        if (!doorOpen) {
            doorClosedState();
        }
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.O) && !doorOpen) {
            doorOpenedState();
        }

        if (Input.GetKeyDown(KeyCode.C) && doorOpen) {
            doorClosedState();
        }
    }

    public void doorOpenedState() {
        //play opening animation and stop on open frame
        renderer.enabled = false;
        doorOpen = true;
        //set tiles to passable
        for (int i = 0; i <= 5; i++) {
            tilesUnderDoor[i].isWalkable = true;
            tilesUnderDoor[i].isSlimeable = true;
            tilesUnderDoor[i].isSpikeable = true;
            tilesUnderDoor[i].isTransparent = true;
        }

    }

    public void doorClosedState() {
        if (safeToClose()) {
            //play closing animation and stop on closed frame
            renderer.enabled = true;
            doorOpen = false;
            //set tiles to unpassable
            for (int i = 0; i <= 5; i++) {
                tilesUnderDoor[i].isWalkable = false;
                tilesUnderDoor[i].isSlimeable = false;
                tilesUnderDoor[i].isSpikeable = false;
                tilesUnderDoor[i].isTransparent = false;
            }
        }
    }

    public bool safeToClose() {
        for (int i = 0; i <= 5; i++) {
            if (tilesUnderDoor[i].getTileEntities() != null || tilesUnderDoor[i].GetComponent<Slime>() != null) {
                return false;
            }
        }
        return true;
    }
}
