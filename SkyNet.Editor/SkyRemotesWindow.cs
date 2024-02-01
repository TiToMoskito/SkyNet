using SkyNet;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

internal class SkyRemotesWindow : SkyWindow
{
    //private Vector2 scroll;

    //[MenuItem("Window/SkyNet/Remotes")]
    //static void Initialize()
    //{
    //    EditorWindow window = GetWindow(typeof(SkyRemotesWindow), false, "Remotes");
    //    window.titleContent = new GUIContent("Remotes", SkyEditorGUI.LoadIcon("mc_connection"));
    //}

    //private new void Update()
    //{
    //    if (Application.isPlaying)
    //        m_repaints = Mathf.Max(m_repaints, 1);
    //    base.Update();
    //}

    //private void Header(string icon, string text)
    //{
    //    GUILayout.BeginHorizontal(SkyEditorGUI.HeaderBackgorund, GUILayout.Height(23f));
    //    SkyEditorGUI.IconButton(icon);
    //    GUILayout.Label(text);
    //    GUILayout.EndHorizontal();
    //}

    //private new void OnGUI()
    //{
    //    base.OnGUI();
    //    GUILayout.BeginArea(new Rect(1, 1, this.position.width - 2, this.position.height - 2));
    //    scroll = GUILayout.BeginScrollView(scroll);
    //    Header("mc_connection", "Connections");

    //    GUILayout.Space(2f);
    //    GUILayout.BeginHorizontal(new GUIStyle(GUIStyle.none)
    //    {
    //        padding = new RectOffset(5, 5, 2, 2)
    //    }, new GUILayoutOption[0]);
    //    MakeHeader("mc_ipaddress", "Address");
    //    MakeHeader("mc_ping", "Ping (Network)");
    //    MakeHeader("mc_download", "Download");
    //    MakeHeader("mc_upload", "Upload");
    //    GUILayout.EndHorizontal();
    //    GUILayout.Space(4f);

    //    Connections();       
    //    GUILayout.EndArea();
    //    GUILayout.EndScrollView();
    //}

    //private void Connections()
    //{
    //    GUILayout.Space(2f);
    //    GUILayout.BeginHorizontal(new GUIStyle(GUIStyle.none)
    //    {
    //        padding = new RectOffset(25, 5, 2, 2)
    //    }, new GUILayoutOption[0]);

    //    if(SkyManager.isConnected)
    //    {
    //        StatsLabel(SkyManager.Connection.PlayerID+"(local)");
    //        StatsLabel((Mathf.FloorToInt(SkyManager.Connection.RTT * 1000f).ToString() + " ms"));
    //        StatsLabel((Math.Round(SkyManager.Connection.sentPacketsSize / 8.0 / 1000.0, 2).ToString() + " kb/s"));
    //        StatsLabel((Math.Round(SkyManager.Connection.sentPacketsSize / 8.0 / 1000.0, 2).ToString() + " kb/s"));
    //    }
    //    foreach (var c in SkyManager.Connections)
    //    {
    //        StatsLabel(c.PlayerID);
    //        StatsLabel((Mathf.FloorToInt(c.RTT * 1000f).ToString() + " ms"));
    //        StatsLabel((Math.Round(c.sentPacketsSize / 8.0 / 1000.0, 2).ToString() + " kb/s"));
    //        StatsLabel((Math.Round(c.sentPacketsSize / 8.0 / 1000.0, 2).ToString() + " kb/s"));
    //    }

    //    GUILayout.EndHorizontal();
    //    GUILayout.Space(4f);
    //}
    
    //private void MakeHeader(string icon, string text)
    //{
    //    GUILayout.BeginHorizontal();
    //    SkyEditorGUI.IconButton(icon);
    //    GUILayout.Label(text, new GUIStyle(EditorStyles.miniLabel)
    //    {
    //        padding = new RectOffset(),
    //        margin = new RectOffset(5, 0, 3, 0)
    //    }, new GUILayoutOption[0]);
    //    GUILayout.EndHorizontal();
    //}
    
    //private void StatsLabel(object text)
    //{
    //    if (!GUILayout.Button(text.ToString(), new GUIStyle("Label")
    //    {
    //        padding = new RectOffset(),
    //        margin = new RectOffset(0, 0, 0, 2)
    //    }, new GUILayoutOption[0])) ;
    //}
}
