#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace aeric.rewind_plugin {
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RewindAttribute))]
    public class RewindAttributeDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            // Create a GUIStyle with the desired color
            var coloredTextStyle = new GUIStyle(EditorStyles.label);
            coloredTextStyle.normal.textColor = Color.green;

            // Get the position of the label and apply the modified style to it
            var p3 = position;
            p3.width = 10.0f;

            // Use EditorGUI.PropertyField to draw the property with the modified style
            var p2 = position;
            p2.xMin += 10.0f;
            EditorGUI.PropertyField(p2, property, label, true);
            EditorGUI.LabelField(p3, "r", coloredTextStyle);
            EditorGUI.EndProperty();
        }
    }
#endif
}