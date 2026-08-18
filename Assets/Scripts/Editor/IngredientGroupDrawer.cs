using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(LevelData.IngredientGroup))]
public class IngredientGroupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        position.height = EditorGUIUtility.singleLineHeight;
        
        // Draw the foldout
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            var typeProp = property.FindPropertyRelative("ingredientType");
            var qtyProp = property.FindPropertyRelative("quantity");
            var mysteryProp = property.FindPropertyRelative("isMystery");
            var hasKeyProp = property.FindPropertyRelative("hasKey");
            var keyIndexProp = property.FindPropertyRelative("keyIndex");
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(position, typeProp);
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(position, qtyProp);
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(position, mysteryProp);
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(position, hasKeyProp);
            
            if (hasKeyProp.boolValue)
            {
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(position, keyIndexProp);
            }
            
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
        
        int lineCount = 5; // label, type, qty, mystery, hasKey
        if (property.FindPropertyRelative("hasKey").boolValue)
        {
            lineCount++; // keyIndex
        }
        
        return (EditorGUIUtility.singleLineHeight * lineCount) + (EditorGUIUtility.standardVerticalSpacing * (lineCount - 1));
    }
}
