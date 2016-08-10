using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(GlowImage), true)]
    [CanEditMultipleObjects]
    public class GlowImageEditor : ImageEditor
    {
        private SerializedProperty m_GlowSize;

        private SerializedProperty m_GlowColor;

        private SerializedProperty m_GlowIntensitive;

        private SerializedProperty m_GlowQuality;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_GlowSize = serializedObject.FindProperty("mGlowSize");
            m_GlowColor = serializedObject.FindProperty("mGlowColor");
            m_GlowIntensitive = serializedObject.FindProperty("mGlowIntensitive");
            m_GlowQuality = serializedObject.FindProperty("mGlowQuality");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(m_GlowSize, new GUIContent("Glow Size"));
            EditorGUILayout.PropertyField(m_GlowColor, new GUIContent("Glow Color"));
            EditorGUILayout.PropertyField(m_GlowIntensitive, new GUIContent("Glow Intensitive"));
            EditorGUILayout.PropertyField(m_GlowQuality, new GUIContent("Glow Quality"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
