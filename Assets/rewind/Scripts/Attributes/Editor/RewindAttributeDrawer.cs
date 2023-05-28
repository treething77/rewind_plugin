using UnityEditor;
using UnityEngine;

namespace ccl.rewind_plugin
{
    [CustomPropertyDrawer(typeof(RewindAttribute))]
    public class RewindAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
 
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Create a GUIStyle with the desired color
            GUIStyle coloredTextStyle = new GUIStyle(EditorStyles.label);
            coloredTextStyle.normal.textColor = Color.green;

            // Get the position of the label and apply the modified style to it
            Rect p3 = position;
            p3.width = 10.0f;
        
            // Use EditorGUI.PropertyField to draw the property with the modified style
            Rect p2 = position;
            p2.xMin += 10.0f;
            EditorGUI.PropertyField(p2, property, label, true);
            EditorGUI.LabelField(p3, "r", coloredTextStyle);
            EditorGUI.EndProperty();
        }
    }
}