using UnityEngine;
using System.Collections;

public class MinMaxSlider : PropertyAttribute{
    public float minValue;
    public float maxValue;

    public MinMaxSlider(float minValue, float maxValue) {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}
