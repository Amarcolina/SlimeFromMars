using UnityEngine;
using System.Collections;

/*CONVENTIONS:
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
    public bool isAcidMutation = false;
    public bool isBioMutation = false;
    public bool isElectricityMutation = false;

}
