using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(LevelData.IngredientLaneData))]
public class IngredientLaneDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        position.height = EditorGUIUtility.singleLineHeight;
        
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            var isLockedProp = property.FindPropertyRelative("isLocked");
            var requiredKeysProp = property.FindPropertyRelative("requiredKeys");
            var groupsProp = property.FindPropertyRelative("groups");
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(position, isLockedProp);
            
            if (isLockedProp.boolValue)
            {
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(position, requiredKeysProp);
            }
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUI.GetPropertyHeight(groupsProp, true);
            EditorGUI.PropertyField(position, groupsProp, true);
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
        float height = EditorGUIUtility.singleLineHeight; 
        
        var isLockedProp = property.FindPropertyRelative("isLocked");
        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; 
        
        if (isLockedProp.boolValue)
        {
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; 
        }
        
        var groupsProp = property.FindPropertyRelative("groups");
        height += EditorGUI.GetPropertyHeight(groupsProp, true) + EditorGUIUtility.standardVerticalSpacing;
        
        return height;
    }
}
