using UnityEngine;
using UnityEditor;

namespace LOS
{
    [CustomEditor(typeof(LOSStencilRenderer))]
    public class LOSStencilRendererEditor : Editor
    {
        private SerializedProperty m_IsDynamicBatchingDisabled;

        private void OnEnable()
        {
            m_IsDynamicBatchingDisabled = serializedObject.FindProperty("m_IsDynamicBatchingDisabled");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            LOSStencilRenderer targetScript = target as LOSStencilRenderer;

            m_IsDynamicBatchingDisabled.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Prevent Dynamic Batching", "Prevents Renderer from being batched"), m_IsDynamicBatchingDisabled.boolValue);

            if (m_IsDynamicBatchingDisabled.boolValue)
            {
                if (targetScript.IsStatic)
                {
                    EditorGUILayout.HelpBox("The GameObject this component is attached to is marked as static. This setting will be ignored.", MessageType.Warning);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}