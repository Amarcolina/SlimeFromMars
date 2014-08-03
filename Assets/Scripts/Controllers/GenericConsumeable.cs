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
    public GameObject resourcedisplay_GameObject;

    //flags item with special mutation property and type
    public bool isRadiationMutation = false;
    public bool isBioMutation = false;
    public bool isElectricityMutation = false;

    //Will destroy the info box if the item is eaten by slime
	public void OnDestroy(){
        if (resourcedisplay_Label != null) {
             resourcedisplay_Label.enabled = false;
        }
    }

    //Finds reference to the information display label and adds a box collider
    public void Awake(){
        resourcedisplay_GameObject = GameObject.FindGameObjectWithTag ("ItemInfo");
        resourcedisplay_Label = resourcedisplay_GameObject.GetComponent<UILabel> ();
        gameObject.AddComponent<BoxCollider2D> ();
	}

    //Displays the information for a given item
	public void OnMouseOver(){
        resourcedisplay_Label.text = gameObject.name +"\nRadiation:" + radiation + "\nBio:" + bio + "\nElectricity:" + electricity;
        resourcedisplay_Label.enabled = true;
	}
    //Hides the information for a given item
	public void OnMouseExit(){
        resourcedisplay_Label.enabled = false;
	}

}

