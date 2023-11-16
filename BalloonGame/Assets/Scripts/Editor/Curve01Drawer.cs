using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Curve01Attribute))]
public sealed class Curve01Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.AnimationCurve) return;

        Rect ranges = new(0f, 0f, 1f, 1f);

        EditorGUI.CurveField(position, property, Color.cyan, ranges);
    }
}