using UnityEngine;
using System.Collections;

//Energy is given based off item's size when consumed
//Enemies are also considered consumeable
public enum ItemSize {
    SMALL = 4,
    MEDIUM = 8,
    LARGE = 12
}

public class GenericConsumeable : MonoBehaviour, IGrabbable {
    //items have elemenal affinity values as well as default energy value
    public ItemSize size;
    public int radiation;
    public int bio;
    public int electricity;
    public UILabel resourcedisplay_Label;
    public UISprite resourcedisplay_Sprite;
    public GameObject resourcedisplay_GameObject;
    private SlimeController _slimeControllerInstance;

    //flags item with special mutation property and type
    public bool isRadiationMutation = false;
    public bool isBioMutation = false;
    public bool isElectricityMutation = false;


    //Will destroy the info box if the item is eaten by slime
    public void OnDestroy() {
        if (resourcedisplay_Label != null && resourcedisplay_Sprite != null) {
            resourcedisplay_Label.enabled = false;
            resourcedisplay_Sprite.enabled = false;
        }
    }

    //Finds reference to the information display label and adds a box collider
    public void Awake() {
        _slimeControllerInstance = SlimeController.getInstance();
        resourcedisplay_GameObject = GameObject.FindGameObjectWithTag("ItemInfo");
        resourcedisplay_Label = resourcedisplay_GameObject.GetComponentInChildren<UILabel>();
        resourcedisplay_Sprite = resourcedisplay_GameObject.GetComponentInChildren<UISprite>();
        gameObject.AddComponent<BoxCollider2D>();
    }

    //Displays the information for a given item and calculates potential energy
    public void OnMouseDown() {
        _slimeControllerInstance.enableResourcePopup(gameObject.name, (int)size, bio, radiation, electricity);
        _slimeControllerInstance.skipNextFrame();
    }

}

