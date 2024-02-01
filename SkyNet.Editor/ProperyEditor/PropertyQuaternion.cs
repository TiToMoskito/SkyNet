using System;
using SkyNet.Editor;
using UnityEditor;
using UnityEngine;

namespace SkyNet.Compiler
{
    public static class PropertyQuaternion
    {
        public static void Show(PropertyDefinition p)
        {
            SkyEditorGUI.WithLabel("Axes", (() =>
            {
                p.RotAxe = (AxisSelection)EditorGUILayout.EnumPopup("", p.RotAxe);
            }));
            SkyEditorGUI.WithLabel("Compression", (() =>
            {
                SkyEditorGUI.EditAxes(p.RotationCompression, p.RotAxe);                
            }));
        }
    }
}
