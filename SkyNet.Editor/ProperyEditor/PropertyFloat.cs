using System;
using SkyNet.Editor;
using UnityEditor;
using UnityEngine;

namespace SkyNet.Compiler
{
    public static class PropertyFloat
    {
        public static void Show(PropertyDefinition p)
        {
            SkyEditorGUI.WithLabel("Compression", (() =>
            {
                EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
                EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
                p.FloatCompression.Enabled = SkyEditorGUI.Toggle(p.FloatCompression.Enabled);
                EditorGUI.BeginDisabledGroup(!p.FloatCompression.Enabled);
                p.FloatCompression.minValue = Mathf.Min(SkyEditorGUI.FloatFieldOverlay(p.FloatCompression.minValue, "Min"), p.FloatCompression.maxValue - 1);
                p.FloatCompression.maxValue = Mathf.Max(SkyEditorGUI.FloatFieldOverlay(p.FloatCompression.maxValue, "Max"), p.FloatCompression.minValue + 1);
                p.FloatCompression.precision = Mathf.Max(SkyEditorGUI.FloatFieldOverlay(p.FloatCompression.precision, "Accuracy"), 1f / 1000f);
                CompressorFloat bits = new CompressorFloat(p.FloatCompression.minValue, p.FloatCompression.maxValue, p.FloatCompression.precision, p.FloatCompression.Enabled);
                GUILayout.Label("Bits: " + (p.FloatCompression.Enabled ? bits.BitsRequired : 32), EditorStyles.miniLabel, new GUILayoutOption[1] { GUILayout.Width(50f) });
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }));
        }
    }
}
