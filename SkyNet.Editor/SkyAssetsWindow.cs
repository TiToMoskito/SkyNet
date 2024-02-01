using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using SkyNet.Unity.Utils;
using System.IO;
using SkyNet.Compiler;
using SkyNet.Editor;
using SkyNet;

internal class SkyAssetsWindow : SkyWindow
{
    private Vector2 m_scroll;
    [SerializeField]
    private string selectedAssetGuid;

    [MenuItem("SkyNet/Window/Assets")]
    static void Initialize()
    {
        EditorWindow window = GetWindow(typeof(SkyAssetsWindow), false, "Assets");
        window.titleContent = new GUIContent("Assets", SkyEditorGUI.LoadIcon("mc_server"));        
    }

    private void OnEnable()
    {
        Directory.CreateDirectory(Util.SkyNetGenFilesPath);
        Directory.CreateDirectory(Util.SkyNetGenStatesPath);
        Directory.CreateDirectory(Util.SkyNetGenEventsPath);
        Directory.CreateDirectory(Util.SkyNetGenObjectsPath);
        LoadAssets();
    }

    private new void Update()
    {
        base.Update();
    }

    private new void OnGUI()
    {
        base.OnGUI();

        Rect position1 = position;
        double width = position1.width;
        position1 = position;
        double height = position1.height - 22.0;
        GUILayout.BeginArea(new Rect(0, 0, (float)width, (float)height));
        m_scroll = GUILayout.BeginScrollView(m_scroll, false, false, new GUILayoutOption[0]);

        Header("States", "mc_state");
        DisplayAssetList(m_states.Values.Cast<AssetDefinition>());
        Header("Objects", "mc_struct");
        DisplayAssetList(m_objects.Values.Cast<AssetDefinition>());
        Header("Events", "mc_event");
        DisplayAssetList(m_events.Values.Cast<AssetDefinition>());

        if (SkyEditorGUI.IsRightClick)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("New State"), false, NewState);
            menu.AddItem(new GUIContent("New Object"), false, NewObject);
            menu.AddItem(new GUIContent("New Event"), false, NewEvent);
            menu.ShowAsContext();
        }

        ClearAllFocus();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        Rect position2 = position;
        position2 = position;
        GUILayout.BeginArea(new Rect(4, (position2.height - 20.0f), (position2.width - 8.0f), 16));
        Footer();
        GUILayout.EndArea();        
    }

    private void DisplayAssetList(IEnumerable<AssetDefinition> _assets)
    {
        bool flag = (UnityEngine.Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control;
        foreach (var asset in _assets)
        {
            GUILayout.BeginHorizontal();
            GUIStyle style = new GUIStyle(EditorStyles.miniButtonLeft);
            style.alignment = TextAnchor.MiddleLeft;
            if (IsSelected(asset))
                style.normal.textColor = SkyEditorGUI.HighlightColor;
            if (GUILayout.Button(new GUIContent(asset.Name), style, new GUILayoutOption[0]))
            {
                Select(asset, true);
                SkyEditorWindow.Open();
            }                
            if (GUILayout.Button(" ", EditorStyles.miniButtonRight, new GUILayoutOption[1] { GUILayout.Width(20f) }))
            {
                if (flag)
                {
                    if (EditorUtility.DisplayDialog("Delete Asset", string.Format("Do you want to delete {0} ({1})?", asset.Name, asset.GetType().Name.Replace("Definition", "")), "Yes", "No"))
                    {
                        asset.Delete = true;
                        if (IsSelected(asset))
                            Select(null, false);
                        Save();
                    }
                }
            }
            if (flag)
                OverlayIcon("mc_minus_small", 1);
            else
                OverlayIcon("mc_group", 0);
            GUILayout.EndHorizontal();
        }        
    }

    private void NewState()
    {
        StateDefinition state = new StateDefinition();
        state.Name = "NewState";
        state.FileName = Util.MakePath(Util.SavePath, "state." + state.Name + ".xml");
        state.UniqueId = UniqueId.New().IdString;
        m_states.Add(state.FileName, state);
        m_asset = state;
        SkyEditorWindow.Open();
    }

    private void NewEvent()
    {
        EventDefinition evnt = new EventDefinition();
        evnt.Name = "NewEvent";
        evnt.FileName = Util.MakePath(Util.SavePath, "event." + evnt.Name + ".xml");
        evnt.UniqueId = UniqueId.New().IdString;
        m_events.Add(evnt.FileName, evnt);
        m_asset = evnt;
        SkyEditorWindow.Open();
    }

    private void NewObject()
    {
        ObjDefinition obj = new ObjDefinition();
        obj.Name = "NewObject";
        obj.FileName = Util.MakePath(Util.SavePath, "obj." + obj.Name + ".xml");
        obj.UniqueId = UniqueId.New().IdString;
        m_objects.Add(obj.FileName, obj);
        m_asset = obj;
        SkyEditorWindow.Open();
    }

    private void Header(string text, string icon)
    {
        SkyEditorGUI.Header(text, icon);
    }

    private void Footer()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("{0} ({1})", "DEBUG", "v0.1"), EditorStyles.miniLabel, new GUILayoutOption[0]);
        GUILayout.FlexibleSpace();
        if (SkyEditorGUI.IconButton("mc_refresh"))
        {
            Debug.Log("Reload project... ");
            LoadAssets();            
        }
        GUILayout.Space(5);
        if (SkyEditorGUI.IconButton("mc_compile_assembly"))
        {
            Debug.Log("Compiling project... ");
            CompileAssets();
        }
        GUILayout.EndHorizontal();
    }

    private void OverlayIcon(string icon, int xOffset)
    {
        Rect lastRect = GUILayoutUtility.GetLastRect();
        lastRect.xMin = lastRect.xMax - 19f + xOffset;
        lastRect.xMax = lastRect.xMax - 3f + xOffset;
        lastRect.yMin = lastRect.yMin;
        ++lastRect.yMax;
        GUI.color = SkyEditorGUI.HighlightColor;
        GUI.DrawTexture(lastRect, (Texture)SkyEditorGUI.LoadIcon(icon));
        GUI.color = Color.white;
    }

    private bool IsSelected(object obj)
    {
        return ReferenceEquals(obj, m_asset);
    }

    private void Select(AssetDefinition asset, bool focusEditor)
    {
        selectedAssetGuid = asset != null ? asset.FileName.ToString() : null;
        m_repaints = 10;
        m_asset = asset;
        BeginClearFocus();
        SkyEditorGUI.UseEvent();
        if (!focusEditor)
            return;
        SkyEditorWindow.Open();
    }

    private void Save()
    {
        CheckDelete();
        LoadAssets();
    }
}
