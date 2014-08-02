using UnityEngine;
using System.Collections;

public class SlimeContainer : IDamageable {

    public static int containerCounter = 0;
    private bool mintCondition = true;
    private bool partiallyDamaged = false;
    private bool broken = false;
    private SlimeController containerEnergy;
    //upon creation, update container number plus one

    private GameUI _gameUi;

    void Awake() {
        containerCounter++;
    }

    void Start()
    {
        _gameUi = GameUI.getInstance();
    }
   
    // Update is called once per frame
    void Update() {
        if (containerCounter <= 0)
        {
            winState();
        }
    }

    private void winState()
    {
        PauseMenu winState = _gameUi.GetComponent<PauseMenu>();
        winState.Victory();
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
