using UnityEngine;
using System.Collections;

public class SlimeContainer : MonoBehaviour, IDamageable {

    public static int containerCounter = 0;
    private bool mintCondition = true;
    private bool partiallyDamaged = false;
    private bool broken = false;
    private SlimeController containerEnergy;
    private Sprite partiallyDamagedSprite;
    private Sprite brokenSprite;
    private SpriteRenderer spriteComponent;
    //upon creation, update container number plus one

    private static GameUI _gameUi;

    void Awake() {
        partiallyDamagedSprite = Resources.Load<Sprite>("Sprites/Accessories/partiallyBrokenHoldingTube");
        brokenSprite = Resources.Load<Sprite>("Sprites/Accessories/escapedHoldingTube");
        spriteComponent = GetComponent<SpriteRenderer>();     
    }

    void OnLevelWasLoaded() {
        containerCounter = 0;
    }

    void Start() {
        containerCounter++;
        _gameUi = GameUI.getInstance();
        gameObject.AddComponent<BoxCollider2D>();
    }

    //updates number of containers in the world minus one
    public static void decrementContainer() {
        containerCounter--;

        if (containerCounter <= 0)
        {
            PauseMenu winState = _gameUi.GetComponent<PauseMenu>();
            winState.Victory();
        }
    }

	public void OnMouseDown() {
        _gameUi.SlimeTankPopUp ();
	}

    //replaces full container with broken one
    public void replaceContainer() {
        if (partiallyDamaged) {
            spriteComponent.sprite = partiallyDamagedSprite;
            //replace with partiallyBroken animation
        } else if (broken) {
            spriteComponent.sprite = brokenSprite;
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
                SlimeController.getInstance().gainEnergy(20);
            }
            replaceContainer();
        }
    }

    public float getHealth() {
        return broken ? 0.0f : 1.0f;
    }
}
