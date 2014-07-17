using UnityEngine;
using System.Collections;

public enum ItemSize {
    SMALL,
    MEDIUM,
    LARGE
}

public class GenericConsumeable : MonoBehaviour {
    public int acid;
    public int bio;
    public int electricity;
    public ItemSize size; 
}
