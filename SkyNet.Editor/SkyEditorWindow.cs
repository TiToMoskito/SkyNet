using UnityEngine;
using UnityEditor;
using System;
using SkyNet.Compiler;
using SkyNet;
using System.Collections.Generic;
using SkyNet.Editor;
internal class SkyEditorWindow : SkyWindow
{
    public static SkyEditorWindow Instance { get; private set; }

    private Vector2 m_scroll;

    [MenuItem("SkyNet/Window/Editor")]
    public static void Open()
    {
        EditorWindow window = GetWindow(typeof(SkyEditorWindow), false, "Editor");
        window.titleContent = new GUIContent("Editor", SkyEditorGUI.LoadIcon("mc_input"));
        window.minSize = new Vector2(300f, 400f);
        window.Show();
        window.Focus();
    }

    void OnEnable()
    {
        Instance = this;
    }

    private new void Update()
    {
        base.Update();
    }

    private new void OnGUI()
    {
        base.OnGUI();

        Editor();
        Header();
        
        ClearAllFocus();
    }

    private void Editor()
    {
        if (m_asset == null)
            return;

        Rect position = this.position;
        GUILayout.BeginArea(new Rect(1, 22, (position.width - 2.0f), (position.height - 22.0f)));
        m_scroll = GUILayout.BeginScrollView(m_scroll, false, false, GUIStyle.none, GUIStyle.none);
        GUILayout.Space(5f);

        if (m_asset is StateDefinition || m_asset is EventDefinition)
        {
            SkyEditorGUI.WithLabel("Packet Flag", (() =>
            {
                GUILayout.BeginHorizontal();
                m_asset.PacketFlag = (ConfigPacketFlags)EditorGUILayout.EnumPopup(m_asset.PacketFlag);
                GUILayout.EndHorizontal();
            }));
        }

        if (m_asset is StateDefinition)
        {
            SkyEditorGUI.WithLabel("Packet Target", (() =>
            {
                GUILayout.BeginHorizontal();
                m_asset.PacketTarget = (ConfigTargets)EditorGUILayout.EnumPopup(m_asset.PacketTarget);
                GUILayout.EndHorizontal();
            }));

            //SkyEditorGUI.WithLabel("Entity Prefab", (() =>
            //{
            //    GUILayout.BeginHorizontal();
            //    SkyEditorGUI.PrefabsPopup(m_asset);
            //    GUILayout.EndHorizontal();
            //}));
        }

        if (m_asset is StateDefinition)
            EditPropertyList(m_asset, ((StateDefinition)m_asset).Properties);
        if (m_asset is EventDefinition)
            EditPropertyList(m_asset, ((EventDefinition)m_asset).Properties);
        if (m_asset is ObjDefinition)
            EditPropertyList(m_asset, ((ObjDefinition)m_asset).Properties);

        GUILayout.EndScrollView();
        GUILayout.EndArea();        
    }

    private void Header()
    {
        if (m_asset == null)
            return;

        EditHeader(m_asset);
    }

    private void EditHeader(AssetDefinition def)
    {
        StateDefinition stateDefinition = def as StateDefinition;
        ObjDefinition objectDefinition = def as ObjDefinition;
        EventDefinition eventDefinition = def as EventDefinition;
        GUILayout.BeginArea(new Rect(1f, 1f, position.width - 2f, 23f));
        GUILayout.BeginHorizontal(SkyEditorGUI.HeaderBackgorund, GUILayout.Height(23f));
        if (def is StateDefinition)
            SkyEditorGUI.IconButton("mc_state");
        if (def is ObjDefinition)
            SkyEditorGUI.IconButton("mc_struct");
        if (def is EventDefinition)
            SkyEditorGUI.IconButton("mc_event");
        def.Name = EditorGUILayout.TextField(def.Name, new GUILayoutOption[0]);
        if (GUILayout.Button("New Property", EditorStyles.miniButton, new GUILayoutOption[1]
        {
          GUILayout.Width(150f)
        }))
        {
            if (stateDefinition != null)
            {
                stateDefinition.Properties.Add(CreateProperty());
            }
            if (eventDefinition != null)
            {
                eventDefinition.Properties.Add(CreateProperty());
            }
            if (objectDefinition != null)
            {
                objectDefinition.Properties.Add(CreateProperty());
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private PropertyDefinition CreateProperty()
    {
        PropertyDefinition propertyDefinition = new PropertyDefinition()
        {
            Name = "NewProperty",
            Type = "Float"
        };
        return propertyDefinition;
    }

    private void EditPropertyList(AssetDefinition def, List<PropertyDefinition> list)
    {
        for (int index = 0; index < list.Count; ++index)
            EditProperty(def, list[index], index == 0, index == list.Count - 1);

        for (int index = 0; index < list.Count; ++index)
        {
            if (list[index].Deleted)
            {
                list.RemoveAt(index);
                --index;
            }
        }
        for (int index = 0; index < list.Count; ++index)
        {
            if (list[index].Adjust != 0)
            {
                PropertyDefinition propertyDefinition1 = list[index];
                PropertyDefinition propertyDefinition2 = list[index + list[index].Adjust];
                list[index + list[index].Adjust] = propertyDefinition1;
                list[index] = propertyDefinition2;
                propertyDefinition1.Adjust = 0;
                propertyDefinition2.Adjust = 0;
            }
        }
    }

    private void EditProperty(AssetDefinition def, PropertyDefinition p, bool first, bool last)
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal(SkyEditorGUI.HeaderBackgorund, GUILayout.Height(23f));
        if ((UnityEngine.Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control)
        {
            if (SkyEditorGUI.IconButton("mc_minus") && EditorUtility.DisplayDialog("Delete Property", string.Format("Do you want to delete '{0}' (Property)?", (object)p.Name), "Yes", "No"))
                p.Deleted = true;
        }
        p.Name = SkyEditorGUI.TextFieldOverlay(p.Name, "", GUILayout.Width(181f));        
        SkyEditorGUI.PropertyTypePopup(p, m_objects);

        if (SkyEditorGUI.IconButton("mc_arrow_down", !last))
            ++p.Adjust;
        if (SkyEditorGUI.IconButton("mc_arrow_up", !first))
            --p.Adjust;
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(2f);
        if(p.Type == "Transform")
        {
            PropertyTransform.Show(p);
        }
        if(p.Type == "Vector")
        {
            PropertyVector.Show(p);
        }
        if (p.Type == "Quaternion")
        {
            PropertyQuaternion.Show(p);
        }
        if (p.Type == "Float")
        {
            PropertyFloat.Show(p);
        }
        if (p.Type == "Integer")
        {
            PropertyInt.Show(p);
        }
        if (p.Type == "Array")
        {
            if(p.ArrayDefinition == null)
                p.ArrayDefinition = new ArrayDefinition();               

            PropertyArray.Show(p.ArrayDefinition, m_objects);
        }
        if (p.Type == "Object")
        {
            PropertyObject.Show(p, m_objects);
        }

        foreach (var obj in m_objects)
        {
            if(p.Type == obj.Value.Name)
            {
                p.objDefinition = obj.Value;
            }
        }

        EditorGUILayout.EndVertical();
    }
}


