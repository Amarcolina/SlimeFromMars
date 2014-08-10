using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour {

    private bool doorOpen;

	// Use this for initialization
	void Start () {
        doorOpen = false;
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
        //get tile offsets under door

    }

    public void closeDoor(){
        //play closing animation and stop on closed frame
        renderer.enabled = true;
    }
}
