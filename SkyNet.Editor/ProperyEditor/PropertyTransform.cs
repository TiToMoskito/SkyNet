using System;
using SkyNet.Editor;
using UnityEditor;
using UnityEngine;

namespace SkyNet.Compiler
{
    public static class PropertyTransform
    {
        public static void Show(PropertyDefinition p)
        {
            SkyEditorGUI.WithLabel("Smooth Algorithm", (() =>
            {
                GUILayout.BeginHorizontal();
                p.SmoothAlgorithm = SkyEditorGUI.Toggle(p.SmoothAlgorithm);
                GUILayout.EndHorizontal();
            }));
            if (p.SmoothAlgorithm)
            {
                SkyEditorGUI.WithLabel("Interpolation time", (() =>
                {
                    GUILayout.BeginHorizontal();
                    p.InterpolationBackTime = SkyEditorGUI.FloatFieldOverlay(p.InterpolationBackTime, "time back");
                    GUILayout.EndHorizontal();
                }));

                SkyEditorGUI.WithLabel("Extrapolation Limit", (() =>
                {
                    GUILayout.BeginHorizontal();
                    p.ExtrapolationLimit = SkyEditorGUI.FloatFieldOverlay(p.ExtrapolationLimit, "limit");
                    p.ExtrapolationDistanceLimit = SkyEditorGUI.FloatFieldOverlay(p.ExtrapolationDistanceLimit, "distance");
                    GUILayout.EndHorizontal();
                }));

                SkyEditorGUI.WithLabel("Snap Threshold", (() =>
                {
                    EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
                    p.PositionSnapThreshold = SkyEditorGUI.FloatFieldOverlay(p.PositionSnapThreshold, "pos");
                    p.RotationSnapThreshold = SkyEditorGUI.FloatFieldOverlay(p.RotationSnapThreshold, "rot");
                    EditorGUILayout.EndHorizontal();
                }));

                SkyEditorGUI.WithLabel("Lerp Speed", (() =>
                {
                    EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
                    p.PositionLerpSpeed = SkyEditorGUI.FloatFieldOverlay(p.PositionLerpSpeed, "pos");
                    p.RotationLerpSpeed = SkyEditorGUI.FloatFieldOverlay(p.RotationLerpSpeed, "rot");
                    EditorGUILayout.EndHorizontal();
                }));
            }

            GUILayout.Space(10f);
            SkyEditorGUI.Header("Position", "mc_position");
            SkyEditorGUI.WithLabel("Axes", (() =>
            {
                p.PosAxe = (AxisSelection)EditorGUILayout.EnumPopup("", p.PosAxe);
            }));
            SkyEditorGUI.WithLabel("Compression", (() =>
            {
                SkyEditorGUI.EditAxes(p.PositionCompression, p.PosAxe);
            }));

            GUILayout.Space(10f);
            SkyEditorGUI.Header("Rotation", "mc_rotation");
            SkyEditorGUI.WithLabel("Axes", (() =>
            {
                p.RotAxe = (AxisSelection)EditorGUILayout.EnumPopup("", p.RotAxe);
            }));
            SkyEditorGUI.WithLabel("Compression", (() =>
            {
                SkyEditorGUI.EditAxes(p.RotationCompression, p.RotAxe);
            }));

            GUILayout.Space(10f);
            SkyEditorGUI.Header("Velocity", "mc_rotation");
            SkyEditorGUI.WithLabel("Axes", (() =>
            {
                p.VelAxe = (AxisSelection)EditorGUILayout.EnumPopup("", p.VelAxe);
            }));
            SkyEditorGUI.WithLabel("Compression", (() =>
            {
                SkyEditorGUI.EditAxes(p.VelocityCompression, p.VelAxe);
            }));

            GUILayout.Space(10f);
            SkyEditorGUI.Header("Angular Velocity", "mc_rotation");
            SkyEditorGUI.WithLabel("Axes", (() =>
            {
                p.AVelAxe = (AxisSelection)EditorGUILayout.EnumPopup("", p.AVelAxe);
            }));
            SkyEditorGUI.WithLabel("Compression", (() =>
            {
                SkyEditorGUI.EditAxes(p.AngularVelocityCompression, p.AVelAxe);
            }));
        }
    }
}
