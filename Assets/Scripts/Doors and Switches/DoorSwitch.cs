using UnityEngine;
using System.Collections;

public class DoorSwitch: MonoBehaviour, IDamageable{

    public GenericDoor door;
    //if the switch is damaged it will activate the door with which it is affiliated
    public void damage(float damage) {
        if (door.safeToClose() && door.doorOpen) {
            door.doorClosedState();
        } else if(!door.doorOpen) {
            door.doorOpenedState();
        }
    }

    public float getHealth() {
        return 1;
    }
}
