using System;
using SkyNet.Editor;
using UnityEditor;
using UnityEngine;

namespace SkyNet.Compiler
{
    public static class PropertyInt
    {
        public static void Show(PropertyDefinition p)
        {
            SkyEditorGUI.WithLabel("Compression", (() =>
            {
                p.IntCompression.Enabled = SkyEditorGUI.Toggle(p.IntCompression.Enabled);
                EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);                
                EditorGUI.BeginDisabledGroup(!p.IntCompression.Enabled);
                p.IntCompression.minValue = Mathf.Min(SkyEditorGUI.IntFieldOverlay(p.IntCompression.minValue, "Min"), p.IntCompression.maxValue - 1);
                p.IntCompression.maxValue = Mathf.Max(SkyEditorGUI.IntFieldOverlay(p.IntCompression.maxValue, "Max"), p.IntCompression.minValue + 1);
                CompressorInt bits = new CompressorInt(p.IntCompression.minValue, p.IntCompression.maxValue, p.IntCompression.Enabled);
                GUILayout.Label("Bits: " + (p.IntCompression.Enabled ? bits.BitsRequired : 32), EditorStyles.miniLabel, new GUILayoutOption[1] { GUILayout.Width(50f) });
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }));
        }
    }
}
