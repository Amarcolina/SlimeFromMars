﻿using UnityEngine;
using System.Collections;
//this class handles affinities of items and how the can effect the slime.
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
