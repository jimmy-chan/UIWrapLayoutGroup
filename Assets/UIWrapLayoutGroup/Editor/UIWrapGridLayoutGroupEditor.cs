using UnityEditor;

namespace Kiwi.JimmyGon
{
    [CustomEditor(typeof(UIWrapGridLayoutGroup), true)]
    [CanEditMultipleObjects]
    public class UIWrapGridLayoutGroupEditor : Editor
    {
        SerializedProperty m_Script;
        SerializedProperty m_Viewport;
        SerializedProperty m_Padding;
        SerializedProperty m_CellSize;
        SerializedProperty m_Spacing;
        SerializedProperty m_StartCorner;
        SerializedProperty m_StartAxis;
        SerializedProperty m_ChildAlignment;
        SerializedProperty m_Constraint;
        SerializedProperty m_ConstraintCount;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            m_Viewport = serializedObject.FindProperty("m_Viewport");
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_CellSize = serializedObject.FindProperty("m_CellSize");
            m_Spacing = serializedObject.FindProperty("m_Spacing");
            m_StartCorner = serializedObject.FindProperty("m_StartCorner");
            m_StartAxis = serializedObject.FindProperty("m_StartAxis");
            m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");
            m_Constraint = serializedObject.FindProperty("m_Constraint");
            m_ConstraintCount = serializedObject.FindProperty("m_ConstraintCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.PropertyField(m_Viewport);
            EditorGUILayout.PropertyField(m_Padding, true);
            EditorGUILayout.PropertyField(m_CellSize, true);
            EditorGUILayout.PropertyField(m_Spacing, true);
            EditorGUILayout.PropertyField(m_StartCorner, true);
            EditorGUILayout.PropertyField(m_StartAxis, true);
            EditorGUILayout.PropertyField(m_ChildAlignment, true);
            EditorGUILayout.PropertyField(m_Constraint, true);
            if (m_Constraint.enumValueIndex > 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_ConstraintCount, true);
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
