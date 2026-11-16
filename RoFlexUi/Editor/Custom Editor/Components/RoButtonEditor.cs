using RO_Flex_UI.Components;
using UnityEditor;
using UnityEditor.UI;

namespace RO_Flex_UI.Editor
{
    [CustomEditor(typeof(RoButton), true), CanEditMultipleObjects]
    public class RoButtonEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("label"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pointerClick"),
                new UnityEngine.GUIContent("On Pointer Click"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
