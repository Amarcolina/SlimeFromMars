﻿using UnityEngine;
using System.Collections;

/*This class keeps track of slime attribute values, mutation types, offense/defense abilities based on mutation type, and offense/defense values
 * 
 */
public class SlimeController : MonoBehaviour {
    private int energy;
    private int acidLevel;
    private int electricityLevel;
    private int bioLevel;

    // Use this for initialization
    void Start() {
        acidLevel = 0;
        electricityLevel = 0;
        bioLevel = 0;
        energy = 20;
    }

    // Update is called once per frame
    void Update() {
        //if slime and food overlap, consume
    }

    public void consume(GenericConsumeable eatenItem) {
        //calculates resource bonus from item element affinity multiplied by level of slime attribute
        //calculates default item resource value based on size and adds any bonuses
        energy = (int)eatenItem.size + acidLevel * eatenItem.acid + bioLevel * eatenItem.bio + electricityLevel * eatenItem.electricity;

        if (eatenItem.isMutation) {
            acidLevel += eatenItem.acid/100;
            electricityLevel += eatenItem.electricity/100;
            bioLevel += eatenItem.bio/100;
        }
    }

    public void useAcidOffense() { 
    }
    public void useElectricityOffense() {
    }
    public void useBioOffense() {
    }


    public void useAcidDefense() {
    }
    public void useElectricityDefense() {
    }
    public void useBioDefense() {
    }
}
