using System;
using System.Collections.Generic;
using SkyNet.Editor;
using UnityEditor;
using UnityEngine;

namespace SkyNet.Compiler
{
    public static class PropertyObject
    {
        public static void Show(PropertyDefinition p, Dictionary<string, ObjDefinition> _objects)
        {
            SkyEditorGUI.WithLabel("Object Type", (() =>
            {
                SkyEditorGUI.ObjectsPopup(p, _objects);
            }));
        }
    }
}
