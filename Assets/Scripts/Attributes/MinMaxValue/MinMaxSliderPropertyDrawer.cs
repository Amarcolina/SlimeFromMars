#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(MinMaxSlider))]
public class MinMaxSliderPropertyDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        MinMaxSlider slider = (MinMaxSlider)attribute;

        if (property.propertyType == SerializedPropertyType.Vector2) {
            float x = property.vector2Value.x;
            float y = property.vector2Value.y;
            EditorGUI.MinMaxSlider(label, position, ref x, ref y, slider.minValue, slider.maxValue);

            Rect minRect = position;
            minRect.x += EditorGUIUtility.labelWidth;
            minRect.width -= EditorGUIUtility.labelWidth;
            minRect.width /= 2;
            minRect.y += EditorGUIUtility.singleLineHeight;

            Rect maxRect = minRect;
            maxRect.x += maxRect.width;

            EditorGUI.LabelField(minRect, x.ToString());
            EditorGUI.LabelField(maxRect, y.ToString());
            property.vector2Value = new Vector2(x, y);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return base.GetPropertyHeight(property, label) * 2;
    }
}
#endif
