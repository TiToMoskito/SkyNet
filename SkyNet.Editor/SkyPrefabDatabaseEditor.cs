using SkyNet;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using SkyNet.Editor;
using System;

[CustomEditor(typeof(PrefabDatabase))]
internal class SkyPrefabDatabaseEditor : Editor
{
    [MenuItem("SkyNet/Window/Prefabs", priority = 23)]
    private static void OpenPrefabDatabaseEditor()
    {
        Selection.activeObject = PrefabDatabase.Instance;
    }

    [MenuItem("SkyNet/Prefabs/Update Prefab Database")]
    public static void UpdatePrefabDatabase()
    {
        SkyCompiler.CompilePrefabs();
    }

    [MenuItem("SkyNet/Prefabs/Create Prefab Database")]
    public static void CreatePrefabDatabase()
    {
        PrefabDatabase asset = CreateInstance<PrefabDatabase>();

        AssetDatabase.CreateAsset(asset, "Assets/SkyNet/Resources/PrefabDatabase.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    private void Save()
    {
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
       // SkyCompiler.CompilePrefabs();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Space(4f);
        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
        GUILayout.Space(2f);

        GUI.DrawTexture(GUILayoutUtility.GetRect(128f, 128f, 64f, 64f, new GUILayoutOption[2]
        {
          GUILayout.ExpandHeight(false),
          GUILayout.ExpandWidth(false)
        }),(Resources.Load("SkyLogo") as Texture2D));

        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        PrefabDatabase pDB = (PrefabDatabase)target;

        //EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
        //for (int index = 1; index < pDB.Prefabs.Length; ++index)
        //{
        //    EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
        //    if (GUILayout.Button(" ", EditorStyles.miniButton , new GUILayoutOption[1]
        //    {
        //  GUILayout.Width(20f)
        //    }))
        //    {
        //        ArrayUtility.RemoveAt<GameObject>(ref pDB.Prefabs, index);
        //        this.Save();
        //        --index;
        //    }
        //    else
        //    {
        //        this.OverlayIcon("mc_minus_small", 1);
        //        pDB.Prefabs[index] = (GameObject)EditorGUILayout.ObjectField((UnityEngine.Object)pDB.Prefabs[index], typeof(GameObject), false, new GUILayoutOption[0]);
        //        EditorGUILayout.EndHorizontal();
        //    }
        //}
        //GUILayout.Space(6f);
        //if (GUILayout.Button("Add Prefab Slot", EditorStyles.miniButton, new GUILayoutOption[0]))
        //{
        //    Array.Resize<GameObject>(ref pDB.Prefabs, pDB.Prefabs.Length + 1);
        //    this.Save();
        //}
        //HashSet<int> intSet = new HashSet<int>();
        //for (int index = 1; index < pDB.Prefabs.Length; ++index)
        //{
        //    if ((bool)((UnityEngine.Object)pDB.Prefabs[index]))
        //    {
        //        if (intSet.Contains(pDB.Prefabs[index].GetInstanceID()))
        //        {
        //            Debug.LogError((object)string.Format("Removed Duplicate Prefab: {0}", (object)pDB.Prefabs[index].name));
        //            pDB.Prefabs[index] = (GameObject)null;
        //            this.Save();
        //        }
        //        else
        //            intSet.Add(pDB.Prefabs[index].GetInstanceID());
        //    }
        //}
        //if (GUI.changed)
        //    this.Save();
        //EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
        for (int index = 0; index < pDB.Prefabs.Count; ++index)
        {
            GUIStyle guiStyle = new GUIStyle(EditorStyles.miniButton);
            guiStyle.alignment = TextAnchor.MiddleLeft;
            if (GUILayout.Button((pDB.Prefabs[index].GetComponent<SkyEntity>()).prefabId.ToString() + " " + Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(pDB.Prefabs[index])), guiStyle, new GUILayoutOption[0]))
                Selection.activeGameObject = pDB.Prefabs[index];
        }

        EditorGUILayout.EndVertical();

        if (GUI.changed)
            Save();
    }

    private void OverlayIcon(string icon, int xOffset)
    {
        Rect lastRect = GUILayoutUtility.GetLastRect();
        lastRect.xMin = lastRect.xMax - 19f + xOffset;
        lastRect.xMax = lastRect.xMax - 3f + xOffset;
        lastRect.yMin = lastRect.yMin;
        lastRect.yMax = lastRect.yMax + 1f;
        GUI.color = Color.white;
        GUI.DrawTexture(lastRect, SkyEditorGUI.LoadIcon(icon));
        GUI.color = Color.white;
    }
}
