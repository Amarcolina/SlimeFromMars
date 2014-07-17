using UnityEngine;
using System.Collections;

/*CONVENTIONS:
 * Although a mutation item may have multiple affinities, each affinity must be of value 100.
 * Non-mutation item affinities should have values that do not exceed 20.
 */
public enum ItemSize {
    SMALL = 5,
    MEDIUM = 10,
    LARGE = 20
}

public class GenericConsumeable : MonoBehaviour {
    
    public ItemSize size;
    public int acid;
    public int bio;
    public int electricity;

    public bool isMutation = false;
}
