using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour {

    private bool doorOpen;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown("O")){
            openDoor();
        }
        if (Input.GetKeyDown("C")) {
            closeDoor();
        }
	}

    public bool isDoorOpen() {
        return doorOpen;
    }

    public void openDoor(){
        //play opening animation and stop on open frame
        
    }

    public void closeDoor(){
        //play closing animation and stop on closed frame

    }
}
