using UnityEngine;
using System.Collections;

public class SlimeContainer : GenericConsumeable, IDamageable {

    public static int containerCounter = 0;
    private bool mintCondition = true;
    private bool partiallyDamaged = false;
    private bool broken = false;
    private SlimeController containerEnergy;
    //upon creation, update container number plus one
    void Awake() {
        containerCounter++;
    }
   
    // Update is called once per frame
    void Update() {
        
    }

    //updates number of containers in the world minus one
    public static void decrementContainer() {
        containerCounter--;
    }

    //replaces full container with broken one
    public void replaceContainer() {
        if (partiallyDamaged) {
            //replace with partiallyBroken animation
        } else if (broken) {
            //replace with broken animation
        }
    }

    public void damage(float dam) {
        if (!broken) {
            if (mintCondition) {
                mintCondition = false;
                partiallyDamaged = true;
            } else if (partiallyDamaged) {
                partiallyDamaged = false;
                broken = true;
                decrementContainer();
                SlimeController.getInstance().gainEnergy(50);
            }
            replaceContainer();
        }
    }
}
