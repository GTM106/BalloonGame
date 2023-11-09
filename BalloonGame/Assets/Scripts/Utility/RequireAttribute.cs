using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// Inspector�Œl��null�̏ꍇ�A�x�����o�������ł��B
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class RequiredAttribute : PropertyAttribute
{
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(RequiredAttribute))]
public class RequiredDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = base.GetPropertyHeight(property, label);
        
        if (IsRequire(property))
        {
            //�x���̕\��
            EditorGUI.HelpBox(position, property.displayName + " must be set to a value.", MessageType.Error);
            
            //�ʏ�̃v���p�e�B�t�B�[���h�̕\�������Ɏ����Ă���
            position.y += position.height;
        }

        //�ʏ�̃v���p�e�B�t�B�[���h�̕\��
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
            //����ȊO�̑������ǉ��ł��܂�
            SerializedPropertyType.String => string.IsNullOrEmpty(property.stringValue),
            SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,            
            _ => false,
        };
    }
}
#endif