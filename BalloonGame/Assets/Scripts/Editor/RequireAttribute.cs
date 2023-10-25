using System;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class RequiredAttribute : PropertyAttribute
{
}

[CustomPropertyDrawer(typeof(RequiredAttribute))]
public class RequiredDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = base.GetPropertyHeight(property, label);
        EditorGUI.PropertyField(position, property, label);
        position.y += position.height;
        if (IsRequire(property))
        {
            EditorGUI.HelpBox(position, "Set Value", MessageType.Error);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (IsRequire(property))
        {
            return base.GetPropertyHeight(property, label) * 2f;
        }
        return base.GetPropertyHeight(property, label);
    }

    private bool IsRequire(SerializedProperty property)
    {
        if (property.isArray)
            return property.arraySize == 0;

        return property.propertyType switch
        {
            SerializedPropertyType.Integer => property.intValue == 0,
            SerializedPropertyType.Float => property.floatValue == 0f,
            SerializedPropertyType.String => property.stringValue == "",
            SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,
            _ => false,
        };
    }
}