using UnityEngine;
using UnityEditor;
using SkyNet;
using System;
using SkyNet.Editor;
internal class SkySettingsWindow : EditorWindow
{
    private Vector2 m_scrollPos;
    private Config m_config;

    [MenuItem("SkyNet/Window/Settings")]
    static void Initialize()
    {
        EditorWindow window = GetWindow(typeof(SkySettingsWindow), false, "Settings");
        window.titleContent = new GUIContent("Settings", SkyEditorGUI.LoadIcon("mc_settings"));
    }

    private void Header(string text, string icon)
    {
        EditorGUILayout.BeginHorizontal(new GUIStyle(SkyEditorGUI.HeaderBackgorund)
        {
            padding = new RectOffset(5, 0, 4, 4)
        }, new GUILayoutOption[0]);
        SkyEditorGUI.Icon(icon);
        GUILayout.Label(text, new GUIStyle(EditorStyles.boldLabel)
        {
            margin = {
                top = 0
            }
        }, new GUILayoutOption[0]);
        EditorGUILayout.EndHorizontal();
    }

    private void OnEnable()
    {
        m_config = Config.instance;
    }

    private void OnGUI()
    {
        m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, new GUILayoutOption[0]);
        Header("Replication", "mc_replication");
        Replication();
        Header("Connection", "mc_connection");
        Connection();
        Header("Packet", "mc_replication");
        BitStream();
        Header("Master Server", "mc_masterserver");
        MasterServer();
        Header("Latency Simulation", "mc_ping_sim");
        Simulation();
        Header("Miscellaneous", "mc_settings");
        Miscellaneous();
        EditorGUILayout.EndScrollView();
        if (!GUI.changed)
            return;
        Save();
    }

    private void Save()
    {
        Config.instance = m_config;
        EditorUtility.SetDirty(PrefabDatabase.Instance);
        AssetDatabase.SaveAssets();
    }

    private void Replication()
    {
        SkyEditorGUI.WithLabel("Tick Rate", (() => m_config.tickRate = Mathf.Max(10, SkyEditorGUI.IntFieldOverlay(m_config.tickRate, "Ticks / Second"))));
        SkyEditorGUI.WithLabel("Send Rate", (() => m_config.sendRate = Mathf.Max(10, SkyEditorGUI.IntFieldOverlay(m_config.sendRate, "packets / Second"))));
    }

    private void BitStream()
    {
        SkyEditorGUI.WithLabel("Capacity", (() => m_config.capacity = Mathf.Max(8, SkyEditorGUI.IntFieldOverlay(m_config.capacity, "byte per Data Chunk"))));
        SkyEditorGUI.WithLabel("String Capacity", (() => m_config.stringCapacity = Mathf.Max(1024, SkyEditorGUI.IntFieldOverlay(m_config.stringCapacity, "max. String Length"))));
        SkyEditorGUI.WithLabel("Grow Multiplier", (() => m_config.growMultiplier = Mathf.Max(2, SkyEditorGUI.IntFieldOverlay(m_config.growMultiplier, "Multipler data structure expanded"))));
        SkyEditorGUI.WithLabel("Grow Addition", (() => m_config.growAddition = Mathf.Max(1, SkyEditorGUI.IntFieldOverlay(m_config.growAddition, "Constant if data structure expanded"))));
    }

    private void Connection()
    {
        SkyEditorGUI.WithLabel("Limit", (() => m_config.serverConnectionLimit = SkyEditorGUI.IntFieldOverlay(m_config.serverConnectionLimit, "")));
        SkyEditorGUI.WithLabel("Timeout", (() => m_config.connectionTimeout = SkyEditorGUI.IntFieldOverlay(m_config.connectionTimeout, "ms")));
        SkyEditorGUI.WithLabel("Connect Timeout", (() => m_config.connectionRequestTimeout = SkyEditorGUI.IntFieldOverlay(m_config.connectionRequestTimeout, "ms")));
        SkyEditorGUI.WithLabel("Connect Attempts", (() => m_config.connectionRequestAttempts = SkyEditorGUI.IntFieldOverlay(m_config.connectionRequestAttempts, "")));
        SkyEditorGUI.WithLabel("Accept Mode", (() => m_config.serverConnectionAcceptMode = (ConnectionAcceptMode)EditorGUILayout.EnumPopup((Enum)m_config.serverConnectionAcceptMode, new GUILayoutOption[0])));
        EditorGUI.BeginDisabledGroup(m_config.serverConnectionAcceptMode != ConnectionAcceptMode.Manual);
        EditorGUI.EndDisabledGroup();    
    }

    private void MasterServer()
    {
        SkyEditorGUI.WithLabel("Game Id", (() =>
        {
            if (m_config.masterServerGameId == null || m_config.masterServerGameId.Trim().Length == 0)
            {
                m_config.masterServerGameId = Guid.NewGuid().ToString().ToUpperInvariant();
                this.Save();
            }
            GUILayout.BeginVertical();
            m_config.masterServerGameId = EditorGUILayout.TextField(m_config.masterServerGameId, new GUILayoutOption[0]);
            try
            {
                if (new Guid(m_config.masterServerGameId) == Guid.Empty)
                    EditorGUILayout.HelpBox("The game id must non-zero", (MessageType)3);
            }
            catch
            {
                EditorGUILayout.HelpBox("The game id must be a valid GUID in this format: 00000000-0000-0000-0000-000000000000", (MessageType)3);
            }
            GUILayout.EndVertical();
        }));
        SkyEditorGUI.WithLabel("Endpoint", (() => m_config.masterServerEndPoint = EditorGUILayout.TextField(m_config.masterServerEndPoint, new GUILayoutOption[0])));
        SkyEditorGUI.WithLabel("Connect", (() => m_config.masterServerAutoConnect = SkyEditorGUI.ToggleDropdown("Automatic", "Manual", m_config.masterServerAutoConnect)));
        SkyEditorGUI.WithLabel("Disconnect", (() => m_config.masterServerAutoDisconnect = SkyEditorGUI.ToggleDropdown("Automatic", "Manual", m_config.masterServerAutoDisconnect)));
    }

    private void Simulation()
    {
        EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
        SkyEditorGUI.WithLabel("Enabled", (() => m_config.useNetworkSimulation = EditorGUILayout.Toggle(m_config.useNetworkSimulation, new GUILayoutOption[0])));
        EditorGUI.BeginDisabledGroup(!m_config.useNetworkSimulation);
        SkyEditorGUI.WithLabel("Packet Loss", (() => m_config.simulatedLoss = Mathf.Clamp01(SkyEditorGUI.IntFieldOverlay(Mathf.Clamp(Mathf.RoundToInt(m_config.simulatedLoss * 100f), 0, 100), "Percent") / 100f)));
        SkyEditorGUI.WithLabel("Ping", (() =>
        {
            m_config.simulatedPingMean = SkyEditorGUI.IntFieldOverlay(m_config.simulatedPingMean, "Mean");
            m_config.simulatedPingJitter = SkyEditorGUI.IntFieldOverlay(m_config.simulatedPingJitter, "Jitter");
        }));
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();
    }

    private void Miscellaneous()
    {
        SkyEditorGUI.WithLabel("Log Targets", (() => m_config.logTargets = (LogTargets)EditorGUILayout.EnumFlagsField(m_config.logTargets, new GUILayoutOption[0])));
    }
}
