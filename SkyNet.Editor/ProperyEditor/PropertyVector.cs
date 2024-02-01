using System;
using SkyNet.Editor;
using UnityEditor;
using UnityEngine;

namespace SkyNet.Compiler
{
    public static class PropertyVector
    {
        public static void Show(PropertyDefinition p)
        {
            SkyEditorGUI.WithLabel("Axes", (() =>
            {
                p.PosAxe = (AxisSelection)EditorGUILayout.EnumPopup("", p.PosAxe);
            }));
            SkyEditorGUI.WithLabel("Compression", (() =>
            {
                SkyEditorGUI.EditAxes(p.PositionCompression, p.PosAxe);
            }));
        }
    }
}
