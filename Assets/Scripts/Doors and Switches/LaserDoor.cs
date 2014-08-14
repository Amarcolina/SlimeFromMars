using UnityEngine;
using System.Collections;

public class LaserDoor : GenericDoor {
    SpriteRenderer spriteComponent;

   void Awake(){
        spriteComponent = GetComponent<SpriteRenderer>();
   }
    // Update is called once per frame
    void Update() {
        if (doorOpen) {
            doorOpenedState();
        }
        if (!doorOpen) {
            doorClosedState();
        }
    }

    public override void doorClosedState() {
        //play closing animation and stop on closed frame
        renderer.enabled = true;
        spriteComponent.sprite = Resources.Load<Sprite>("Sprites/Accessories/closedLaserDoors");
        doorOpen = false;
        //set tiles to unpassable
        for (int i = 4; i <= 5; i++) {
            tilesUnderDoor[i].isSlimeable = false;
            tilesUnderDoor[i].isWalkable = false;
            //tilesUnderDoor[i].isTransparent = false;
        }
    }

    public override void doorOpenedState() {
        //play opening animation and stop on open frame
        spriteComponent.sprite = Resources.Load<Sprite>("Sprites/Accessories/openLaserDoors");
        doorOpen = true;
        //set tiles to passable
        for (int i = 4; i <= 5; i++) {
            tilesUnderDoor[i].isSlimeable = true;
            tilesUnderDoor[i].isWalkable = true;
            //tilesUnderDoor[i].isTransparent = true;
        }

    }
    public override bool safeToClose() {
        for (int i = 4; i <= 5; i++) {
            if (tilesUnderDoor[i].getTileEntities() != null || tilesUnderDoor[i].GetComponent<Slime>() != null) {
                return false;
            }
        }
        return true;
    }
}
