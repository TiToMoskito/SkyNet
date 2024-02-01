using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using Process = System.Diagnostics.Process;
using UnityEditor.SceneManagement;
using SkyNet.Editor;

internal class SkyScenesWindow : EditorWindow
{
    [MenuItem("SkyNet/Window/Scenes")]
    static void Initialize()
    {
        EditorWindow window = GetWindow(typeof(SkyScenesWindow), false, "Scenes");
        window.titleContent = new GUIContent("Scenes", SkyEditorGUI.LoadIcon("mc_scenes"));
    }

    [MenuItem("SkyNet/Scenes/Build Client")]
    static void EBuildClient()
    {
        EditorApplication.delayCall += BuildClient;
    }

    [MenuItem("SkyNet/Scenes/Build Server")]
    static void EBuildServer()
    {
        EditorApplication.delayCall += BuildServer;
    }

    [MenuItem("SkyNet/Scenes/Start Client")]
    static void EClientStart()
    {
        StartClient();
    }

    [MenuItem("SkyNet/Scenes/Start Server")]
    static void EServerStart()
    {
        StartServer();
    }

    #region VARS
    static string m_clientLocation = "Build/Client/Client.exe";
    static string m_serverLocation = "Build/Server/Server.exe";
    static bool m_isPlaying = false;
    static int m_clients = 2;
    Vector2 m_sceneScrollPosition;
    #endregion

    #region Unity
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 280, 250));
        GUILayout.Space(4);

        SkyEditorGUI.Header("Debug", "mc_debugplay");
        GUILayout.Space(4);

        GUILayout.BeginHorizontal(GUILayout.Height(16));
        GUILayout.Label("Clients");
        m_clients = SkyEditorGUI.IntFieldOverlay(m_clients, "Clients", GUILayout.Width(120));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Height(16));
        GUILayout.Label("Run Client");
        if (GUILayout.Button("Start", EditorStyles.miniButton, GUILayout.Width(120)))
        {
            StartClient();
        }
        GUILayout.EndHorizontal();        

        GUILayout.BeginHorizontal(GUILayout.Height(16));
        GUILayout.Label("Run Server");
        if (GUILayout.Button("Start", EditorStyles.miniButton, GUILayout.Width(120)))
        {
            StartServer();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        SkyEditorGUI.Header("Scenes", "mc_scenes");
        GUILayout.Space(4);
        GUIScenes();       

        if (GUI.changed)
        {
            AssetDatabase.SaveAssets();
        }
        GUILayout.EndArea();
    }

    void Update()
    {
        if (EditorApplication.isPlaying == false && EditorApplication.isPlayingOrWillChangePlaymode == false && m_isPlaying)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                try
                {
                    foreach (Process p in Process.GetProcesses())
                    {
                        try
                        {
                            if (p.ProcessName == "Client" || p.ProcessName == "Server")
                            {
                                p.Kill();
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }

            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                try
                {
                    foreach (Process p in Process.GetProcesses())
                    {
                        try
                        {
                            if (p.ProcessName == PlayerSettings.productName)
                            {
                                p.Kill();
                            }

                            if (p.ProcessName == "Client" || p.ProcessName == "Server")
                            {
                                p.Kill();
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
            m_isPlaying = false;
        }
    }
    #endregion

    #region Scenes Part
    void GUIScenes()
    {
        m_sceneScrollPosition = GUILayout.BeginScrollView(m_sceneScrollPosition);       

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scene.path);

                GUILayout.Space(2);
                GUILayout.BeginHorizontal(SkyEditorGUI.HeaderBackgorund, GUILayout.Height(23));

                bool isCurrent = EditorSceneManager.GetActiveScene().path == scene.path;
                GUILayout.Label(sceneName);

                if (sceneName == "MainMenu")
                {
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Build", EditorStyles.miniButton, GUILayout.Width(50)))
                    {
                        BuildClient();
                    }
                    GUI.backgroundColor = Color.white;

                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button("Run in Editor", EditorStyles.miniButton, GUILayout.Width(80)))
                    {
                        if(m_clients >= 2)
                            BuildClient();

                        BuildServer();

                        if (EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path).isLoaded)
                        {
                            EditorApplication.isPlaying = true;
                        }

                        m_isPlaying = true;
                    }
                    GUI.backgroundColor = Color.white;
                }

                if (sceneName == "Server")
                {
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Build", EditorStyles.miniButton, GUILayout.Width(50)))
                    {
                        BuildServer();
                    }
                    GUI.backgroundColor = Color.white;

                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button("Run in Editor", EditorStyles.miniButton, GUILayout.Width(80)))
                    {
                        BuildClient();

                        if (EditorSceneManager.OpenScene(EditorBuildSettings.scenes[1].path).isLoaded)
                        {
                            EditorApplication.isPlaying = true;
                        }

                        m_isPlaying = true;
                    }
                    GUI.backgroundColor = Color.white;
                }

                EditorGUI.BeginDisabledGroup(isCurrent);
                if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(50)))
                {
                    EditorSceneManager.SaveOpenScenes();
                    EditorSceneManager.OpenScene(scene.path);
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndScrollView();
    }
    #endregion

    static void BuildClient()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { EditorBuildSettings.scenes[0].path, EditorBuildSettings.scenes[2].path };
        buildPlayerOptions.locationPathName = m_clientLocation;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.Development;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    static void BuildServer()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { EditorBuildSettings.scenes[1].path, EditorBuildSettings.scenes[2].path };
        buildPlayerOptions.locationPathName = m_serverLocation;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.Development;
        BuildPipeline.BuildPlayer(buildPlayerOptions);

        StartServer();
    }

    static void StartClient()
    {
        for (int i = 1; i < m_clients; i++)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(Directory.GetCurrentDirectory() + "/Build/Client/Client.exe");
            processStartInfo.WorkingDirectory = Path.GetDirectoryName(Directory.GetCurrentDirectory() + "/Build/Client/Client.exe");
            Process.Start(processStartInfo);
        }

        if (EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path).isLoaded)
        {
            EditorApplication.isPlaying = true;
        }
        m_isPlaying = true;
    }

    static void StartServer()
    {
        if (File.Exists("Build/Server/Run_Server.bat"))
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(Directory.GetCurrentDirectory() + "/Build/Server/Run_Server.bat");
            processStartInfo.WorkingDirectory = Path.GetDirectoryName(Directory.GetCurrentDirectory() + "/Build/Server/Run_Server.bat");
            Process.Start(processStartInfo);
        }
        else
        {
            using (StreamWriter w = new StreamWriter("Build/Server/Run_Server.bat"))
            {
                w.WriteLine("@echo off");
                w.WriteLine("Server.exe -batchmode -nographics -port 9382 -maxPlayers 200 -serverName \"World Server\" -autoSave 2");
                w.Close();
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo(Directory.GetCurrentDirectory() + "/Build/Server/Run_Server.bat");
            processStartInfo.WorkingDirectory = Path.GetDirectoryName(Directory.GetCurrentDirectory() + "/Build/Server/Run_Server.bat");
            Process.Start(processStartInfo);
        }
    }
}
