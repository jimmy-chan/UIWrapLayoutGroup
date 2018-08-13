using UnityEditor;

namespace Kiwi.JimmyGon
{
    [CustomEditor(typeof(UIWrapDataContent<,>), true)]
    public class UIWrapDataContentEditor : Editor
    {
        SerializedProperty m_Script;
        SerializedProperty m_UIWrapItem;
        SerializedProperty m_AutoSize;
        SerializedProperty m_Data;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            m_UIWrapItem = serializedObject.FindProperty("m_UIWrapItem");
            m_AutoSize = serializedObject.FindProperty("m_AutoSize");
            m_Data = serializedObject.FindProperty("m_Data");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(m_Script);
            EditorGUI.BeginChangeCheck();
            var obj = EditorGUILayout.ObjectField("UIWrapItem", m_UIWrapItem.objectReferenceValue, typeof(UIWrapItem), false);
            if (EditorGUI.EndChangeCheck()) m_UIWrapItem.objectReferenceValue = obj;
            EditorGUILayout.PropertyField(m_AutoSize);
            if (m_Data != null) EditorGUILayout.PropertyField(m_Data, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
