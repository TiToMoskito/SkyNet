using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using SkyNet.Compiler;
using SkyNet;
using System.Collections.Generic;
using System.IO;

namespace SkyNet.Editor
{
    internal static class SkyEditorGUI
    {
        public static readonly Color Blue = new Color(0.0f, 0.6352941f, 0.9098039f);
        public static readonly Color LightBlue = new Color(0.0f, 0.9098039f, 0.8862745f);
        public static readonly Color Orange = new Color(1f, 0.4980392f, 0.1529412f);
        public static readonly Color LightGreen = new Color(0.4117647f, 0.9843137f, 0.03529412f);
        public static readonly Color DarkGreen = new Color(0.1333333f, 0.6941177f, 0.2980392f);
        public static readonly Color LightOrange = new Color(1f, 0.7882353f, 0.04705882f);
        public const int HEADER_HEIGHT = 23;
        public const int GLOBAL_INSET = 1;

        public static Color HighlightColor
        {
            get
            {
                return Color.white;
            }
        }

        public static void SetWindowTitle(this EditorWindow editor, string title, Texture icon)
        {
            typeof(EditorWindow).GetField("m_CachedTitleContent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue((object)editor, (object)new GUIContent(title, icon));
        }

        public static bool DeleteDialog()
        {
            return EditorUtility.DisplayDialog("Confirm", "Do you want to delete this item?", "Yes", "No");
        }

        public static Texture2D LoadIcon(string name)
        {
            return Resources.Load("icons/" + name, typeof(Texture2D)) as Texture2D;
        }

        public static GUIStyle HeaderBackgorund
        {
            get
            {
                GUIStyle guiStyle = new GUIStyle();
                guiStyle.padding = new RectOffset(3, 3, 3, 0);
                guiStyle.border = new RectOffset(3, 3, 3, 3);
                guiStyle.margin = new RectOffset();
                guiStyle.normal.background = Resources.Load("background/dark", typeof(Texture2D)) as Texture2D;
                guiStyle.hover.background = Resources.Load("background/dark_hover", typeof(Texture2D)) as Texture2D;
                guiStyle.normal.textColor = Color.white;
                return guiStyle;
            }
        }

        public static void UseEvent()
        {
            if (UnityEngine.Event.current == null)
                return;
            UnityEngine.Event.current.Use();
        }

        public static void Header(string text, string icon)
        {
            EditorGUILayout.BeginHorizontal(PaddingStyle(5, 0, 0, 0), new GUILayoutOption[0]);
            Icon(icon);
            GUILayout.Label(text, new GUIStyle(EditorStyles.boldLabel)
            {
                margin = {
                top = 0
            }
            }, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
        }

        public static void Icon(string icon)
        {
            Icon(icon, 1f);
        }

        public static void Icon(string icon, float alpha)
        {
            GUIStyle style = new GUIStyle(GUIStyle.none);
            style.padding = new RectOffset(0, 0, 0, 0);
            style.margin = new RectOffset(0, 0, 0, 0);
            style.contentOffset = new Vector2(0.0f, 0.0f);
            Color highlightColor = HighlightColor;
            highlightColor.a = alpha;
            GUI.color = highlightColor;
            GUILayout.Button(LoadIcon(icon), style, GUILayout.Width(16f), GUILayout.Height(16f));
            GUI.color = Color.white;
        }

        public static bool IconButton(string icon, float alpha)
        {
            GUIStyle style = new GUIStyle(GUIStyle.none);
            style.padding = new RectOffset(0, 0, 0, 0);
            style.margin = new RectOffset(0, 0, 0, 0);
            style.contentOffset = new Vector2(0.0f, 0.0f);
            Color highlightColor = HighlightColor;
            highlightColor.a = alpha;
            GUI.color = highlightColor;
            bool flag = GUILayout.Button((Texture)LoadIcon(icon), style, GUILayout.Width(16f), GUILayout.Height(16f));
            GUI.color = Color.white;
            return flag;
        }

        public static bool IconButton(string icon)
        {
            return IconButton(icon, 1f);
        }

        public static bool IconButton(string icon, bool enabled)
        {
            return IconButton(icon, enabled ? 1f : 0.25f) && enabled;
        }

        private static GUIStyle PaddingStyle(int left, int right, int top, int bottom)
        {
            return new GUIStyle(GUIStyle.none)
            {
                padding = new RectOffset(left, right, top, bottom)
            };
        }

        public static void WithLabel(string label, Action gui)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, new GUILayoutOption[1]
            {
      GUILayout.Width(220f)
            });
            gui();
            GUILayout.EndHorizontal();
        }

        public static string TextFieldOverlay(string value, string overlay, params GUILayoutOption[] options)
        {
            GUIStyle guiStyle = new GUIStyle((GUIStyle)"TextField");
            value = EditorGUILayout.TextField(value, guiStyle, options);
            GUI.Label(GUILayoutUtility.GetLastRect(), overlay, new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                contentOffset = new Vector2(-2f, 0.0f),
                normal = {
        textColor = Color.gray
      }
            });
            return value;
        }

        public static int IntFieldOverlay(int value, string overlay, params GUILayoutOption[] options)
        {
            GUIStyle guiStyle = new GUIStyle((GUIStyle)"TextField");
            value = EditorGUILayout.IntField(value, guiStyle, options);
            GUI.Label(GUILayoutUtility.GetLastRect(), overlay, new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                contentOffset = new Vector2(-2f, 0.0f),
                normal = {
        textColor = Color.gray
      }
            });
            return value;
        }

        public static float FloatFieldOverlay(float value, string overlay, params GUILayoutOption[] options)
        {
            GUIStyle guiStyle = new GUIStyle((GUIStyle)"TextField");
            value = EditorGUILayout.FloatField(value, guiStyle, options);
            GUI.Label(GUILayoutUtility.GetLastRect(), overlay, new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                contentOffset = new Vector2(-2f, 0.0f),
                normal = {
                textColor = Color.gray
              }
            });
            return value;
        }

        public static void PropertyTypePopup(PropertyDefinition definition, Dictionary<string, ObjDefinition> _objects)
        {
            List<string> Types = new List<string> { "Array", "Bool", "Color", "Entity", "Float", "Integer", "NetworkId", "Object", "PrefabId", "ProtocolToken", "Quaternion", "String", "Transform", "Vector" };

            if (_objects == null || _objects.Count == 0 || definition.Type == "Object")
                Types.Remove("Object");                

            int num = Array.IndexOf(Types.ToArray(), definition.Type);
            int index = EditorGUILayout.Popup(num, Types.ToArray());
            definition.Type = Types[index];
        }

        public static void ArrayTypePopup(ArrayDefinition definition, Dictionary<string, ObjDefinition> _objects)
        {
            List<string> Types = new List<string> { "Bool", "Color", "Entity", "Float", "Integer", "NetworkId", "Object", "PrefabId", "Quaternion", "String", "Vector" };

            if (_objects == null || _objects.Count == 0)
                Types.Remove("Object");

            int num = Array.IndexOf(Types.ToArray(), definition.Type);
            int index = EditorGUILayout.Popup(num, Types.ToArray());
            definition.Type = Types[index];
        }

        public static void ObjectsPopup(PropertyDefinition definition, Dictionary<string, ObjDefinition> _objects)
        {
            if (_objects == null || _objects.Count == 0) return;

            List<string> objs = new List<string>();
            foreach (var item in _objects)
            {
                objs.Add(item.Value.Name);
            }

            int index = EditorGUILayout.Popup(0, objs.ToArray());

            foreach (var item in _objects)
            {
                if (item.Value.Name == objs.ToArray()[index]) definition.objDefinition = item.Value;
            }
        }

        public static bool ToggleDropdown(string on, string off, bool enabled)
        {
            return EditorGUILayout.Popup(enabled ? 0 : 1, new string[2]
            {
          on,
          off
            }, new GUILayoutOption[0]) == 0;
        }

        public static bool Toggle(bool value)
        {
            return EditorGUILayout.Toggle((value ? 1 : 0) != 0, new GUIStyle(nameof(Toggle))
            {
                margin = new RectOffset(),
                padding = new RectOffset()
            }, new GUILayoutOption[1] { GUILayout.Width(15f) });
        }

        public static bool IsLeftClick
        {
            get
            {
                return UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.button == 0;
            }
        }

        public static bool IsRightClick
        {
            get
            {
                return UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.button == 1 || UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.modifiers == EventModifiers.Command || UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.modifiers == EventModifiers.Control;
            }
        }

        public static void EditAxes(CompressorFloatConfig compression)
        {
            EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
            compression = EditAxis(compression, "", true);
            EditorGUILayout.EndVertical();
        }

        public static void EditAxes(CompressorFloatConfig[] compression, AxisSelection selection)
        {
            EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
            compression[0] = EditAxis(compression[0], "X", (selection & AxisSelection.X) == AxisSelection.X);
            compression[1] = EditAxis(compression[1], "Y", (selection & AxisSelection.Y) == AxisSelection.Y);
            compression[2] = EditAxis(compression[2], "Z", (selection & AxisSelection.Z) == AxisSelection.Z);
            EditorGUILayout.EndVertical();
        }

        private static CompressorFloatConfig EditAxis(CompressorFloatConfig compression, string label, bool enabled)
        {
            if (enabled)
            {
                EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
                GUILayout.Label(label, new GUILayoutOption[1]
                {
                    GUILayout.Width(15f)
                });
                compression = EditFloatCompression(compression);
                EditorGUILayout.EndHorizontal();
            }
            return compression;
        }

        public static CompressorFloatConfig EditFloatCompression(CompressorFloatConfig c)
        {
            c.Enabled = Toggle(c.Enabled);
            EditorGUI.BeginDisabledGroup(!c.Enabled);
            c.minValue = Mathf.Min(FloatFieldOverlay(c.minValue, "Min"), c.maxValue - 1);
            c.maxValue = Mathf.Max(FloatFieldOverlay(c.maxValue, "Max"), c.minValue + 1);
            c.precision = Mathf.Max(FloatFieldOverlay(c.precision, "Accuracy"), 1f / 1000f);
            CompressorFloat bits = new CompressorFloat(c.minValue, c.maxValue, c.precision, c.Enabled);
            GUILayout.Label("Bits: " + (c.Enabled ? bits.BitsRequired : 32), EditorStyles.miniLabel, new GUILayoutOption[1] { GUILayout.Width(50f) });
            EditorGUI.EndDisabledGroup();
            return c;
        }

        /// <summary>
        /// Read all bytes in this stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>All bytes in the stream.</returns>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        private static readonly Dictionary<string, Texture2D> _embeddedIcons = new Dictionary<string, Texture2D>();

        /// <summary>
        /// Get the embedded icon with the given resource name.
        /// </summary>
        /// <param name="resourceName">The resource name.</param>
        /// <returns>The embedded icon with the given resource name.</returns>
        public static Texture2D GetEmbeddedIcon(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            Texture2D icon;
            if (!_embeddedIcons.TryGetValue(resourceName, out icon) || icon == null)
            {
                byte[] iconBytes;
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                    iconBytes = stream.ReadAllBytes();
                icon = new Texture2D(128, 128);
                icon.LoadImage(iconBytes);
                icon.name = resourceName;

                _embeddedIcons[resourceName] = icon;
            }

            return icon;
        }

        /// <summary>
        /// Set the icon for this object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="texture">The icon.</param>
        public static void SetIcon(this UnityEngine.Object obj, Texture2D texture)
        {
            var ty = typeof(EditorGUIUtility);
            var mi = ty.GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
            mi.Invoke(null, new object[] { obj, texture });
        }

        /// <summary>
        /// Set the icon for this object from an embedded resource.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="texture">The icon.</param>
        public static void SetIcon(this UnityEngine.Object obj, string resourceName)
        {
            SetIcon(obj, GetEmbeddedIcon(resourceName));
        }

        /// <summary>
        /// Get the icon for this object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The icon for this object.</returns>
        public static Texture2D GetIcon(this UnityEngine.Object obj)
        {
            var ty = typeof(EditorGUIUtility);
            var mi = ty.GetMethod("GetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
            return mi.Invoke(null, new object[] { obj }) as Texture2D;
        }

        /// <summary>
        /// Remove this icon's object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public static void RemoveIcon(this UnityEngine.Object obj)
        {
            SetIcon(obj, (Texture2D)null);
        }
    }
}