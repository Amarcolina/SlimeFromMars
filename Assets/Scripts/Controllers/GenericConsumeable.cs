using UnityEngine;
using System.Collections;

//Energy is given based off item's size when consumed
//Enemies are also considered consumeable
public enum ItemSize {
    SMALL = 5,
    MEDIUM = 10,
    LARGE = 20
}

public class GenericConsumeable : MonoBehaviour {
    //items have elemenal affinity values as well as default energy value
    public ItemSize size;
    public int radiation;
    public int bio;
    public int electricity;

    //flags item with special mutation property and type
    public bool isRadiationMutation = false;
    public bool isBioMutation = false;
    public bool isElectricityMutation = false;
}
