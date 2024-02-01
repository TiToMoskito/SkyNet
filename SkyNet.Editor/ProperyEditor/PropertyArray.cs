using System;
using System.Collections.Generic;
using SkyNet.Editor;
using UnityEditor;
using UnityEngine;

namespace SkyNet.Compiler
{
    public static class PropertyArray
    {
        public static void Show(ArrayDefinition p, Dictionary<string, ObjDefinition> _objects)
        {
            SkyEditorGUI.WithLabel("Element Type", (() =>
            {
                SkyEditorGUI.ArrayTypePopup(p, _objects);
            }));

            SkyEditorGUI.WithLabel("Element Count", (() =>
            {
                p.Count = Mathf.Max(2, EditorGUILayout.IntField(p.Count, new GUILayoutOption[0]));
            }));

            if (p.Type == "Vector")
            {
                PropertyVector.Show(p.Properties[0]);
            }
            if (p.Type == "Quaternion")
            {
                PropertyQuaternion.Show(p.Properties[0]);
            }
            if (p.Type == "Float")
            {
                PropertyFloat.Show(p.Properties[0]);
            }
            if (p.Type == "Integer")
            {
                PropertyInt.Show(p.Properties[0]);
            }            
            if (p.Type == "Object")
            {
                PropertyObject.Show(p.Properties[0], _objects);
            }

            foreach (var obj in _objects)
            {
                if (p.Type == obj.Value.Name)
                {
                    p.Properties[0].objDefinition = obj.Value;
                }
            }
        }
    }
}
