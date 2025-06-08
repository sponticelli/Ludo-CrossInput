using UnityEditor;
using UnityEngine;

namespace Ludo.CrossInput
{

    [CustomEditor(typeof(Joystick), true)]
    public class JoystickEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty background = serializedObject.FindProperty("background");
            SerializedProperty handle = serializedObject.FindProperty("handle");

            DrawPropertiesExcluding(serializedObject, "background", "handle");

            GUI.enabled = background.objectReferenceValue == null;
            EditorGUILayout.PropertyField(background);

            GUI.enabled = handle.objectReferenceValue == null;
            EditorGUILayout.PropertyField(handle);
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }
}