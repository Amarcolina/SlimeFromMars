using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour {

    private bool doorOpen;
    private Tile[] tilesUnderDoor;
                                            

	// Use this for initialization
	void Start () {
        doorOpen = false;
        //Gets the offset of each tile under the door and stores them in an array for ease of use, later
        tilesUnderDoor[0] = Tilemap.getInstance().getTile(transform.position + new Vector3( .5f, 0f, 0));
        tilesUnderDoor[1] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, 0f, 0));	
        tilesUnderDoor[2] = Tilemap.getInstance().getTile(transform.position + new Vector3( .5f, 1f, 0));	
        tilesUnderDoor[3] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f, 1f, 0));	
        tilesUnderDoor[4] = Tilemap.getInstance().getTile(transform.position + new Vector3( .5f,-1f, 0));	
        tilesUnderDoor[5] = Tilemap.getInstance().getTile(transform.position + new Vector3(-.5f,-1f, 0));	
    }
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown(KeyCode.O)){
            openDoor();
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            closeDoor();
        }
	}

    public bool isDoorOpen() {
        return doorOpen;
    }

    public void openDoor(){
        //play opening animation and stop on open frame
        renderer.enabled = false;
        
        //set tiles to walkable and slimeable
        for (int i = 0; i <= 5; i++) {
            tilesUnderDoor[i].isWalkable = true;
            tilesUnderDoor[i].isSlimeable = true;
        }
           
    }

    public void closeDoor(){
        //play closing animation and stop on closed frame
        renderer.enabled = true;
        for (int i = 0; i <= 5; i++) {
            tilesUnderDoor[i].isWalkable = false;
            tilesUnderDoor[i].isSlimeable = false;
        }
    }
}
