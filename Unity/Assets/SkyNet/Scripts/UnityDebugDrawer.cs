using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkyNet
{
    public class UnityDebugDrawer : IDebugDrawer
    {
        bool isEditor;

        public void Indent(int level)
        {
#if UNITY_EDITOR
            if (isEditor)
            {
                UnityEditor.EditorGUI.indentLevel = level;
                return;
            }            
#endif
        }

        public void IsEditor(bool isEditor)
        {
            this.isEditor = isEditor;
        }

        public void Label(string text)
        {
#if UNITY_EDITOR
            if (isEditor)
            {
                GUILayout.Label(text);
                return;
            }
#endif

            //Bolt.DebugInfo.Label(text);
        }

        public void LabelBold(string text)
        {
#if UNITY_EDITOR
            if (isEditor)
            {
                GUILayout.Label(text, EditorStyles.boldLabel);
                return;
            }
#endif

            //Bolt.DebugInfo.LabelBold(text);
        }

        public void LabelField(string text, object value)
        {
#if UNITY_EDITOR
            if (isEditor)
            {
                UnityEditor.EditorGUILayout.LabelField(text, value.ToString());
                return;
            }
#endif

            //Bolt.DebugInfo.LabelField(text, value);
        }

        public void SelectGameObject(GameObject gameObject)
        {
#if UNITY_EDITOR
            if (!isEditor)
            {
                UnityEditor.Selection.activeGameObject = gameObject;
            }
#endif
        }

        public void Separator()
        {
#if UNITY_EDITOR
            if (isEditor)
            {
                UnityEditor.EditorGUILayout.Separator();
                return;
            }
#endif

            GUILayout.Space(2);
        }
    }
}
