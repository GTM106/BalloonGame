#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspectorで値がnullの場合、警告を出す属性です。
/// </summary>
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
        
        if (IsRequire(property))
        {
            //警告の表示
            EditorGUI.HelpBox(position, property.displayName + " must be set to a value.", MessageType.Error);
            
            //通常のプロパティフィールドの表示を下に持ってくる
            position.y = position.height;
        }

        //通常のプロパティフィールドの表示
        EditorGUI.PropertyField(position, property, label);
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
            //これ以外の属性も追加できます
            SerializedPropertyType.String => string.IsNullOrEmpty(property.stringValue),
            SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,            
            _ => false,
        };
    }
}
#endif