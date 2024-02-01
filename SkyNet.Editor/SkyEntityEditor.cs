using SkyNet;
using SkyNet.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkyEntity))]
internal class SkyEntityEditor : Editor
{
    private static ISerializerFactory[] serializerFactories = typeof(ISerializerFactory).FindInterfaceImplementations().Select((x => Activator.CreateInstance(x))).Cast<ISerializerFactory>().ToArray();
    private static string[] serializerNames = (new string[1]
    {
    "NOT ASSIGNED"
    }).Concat(((IEnumerable<ISerializerFactory>)serializerFactories).Select((x => x.TypeObject.Name))).ToArray();
    private static UniqueId[] serializerIds = ((IEnumerable<ISerializerFactory>)serializerFactories).Select((x => x.TypeKey)).ToArray();

    protected virtual void Awake()
    {
        SetIcons();
    }    

    public override void OnInspectorGUI()
    {
        SkyEntity entity = (SkyEntity)target;

        GUILayout.Space(4f);

        GUIStyle guiStyle = new GUIStyle(EditorStyles.boldLabel);       
        guiStyle.normal.textColor = (EditorGUIUtility.isProSkin ? new Color(0.9490196f, 0.6784314f, 0.0f) : new Color(0.1176471f, 0.3882353f, 0.7176471f));
        GUILayout.Label("Prefab & State", guiStyle, new GUILayoutOption[0]);
        DrawPrefabInfo(entity);
        EditState(entity);

        GUILayout.Label("Settings", guiStyle, new GUILayoutOption[0]);
        GUILayout.BeginHorizontal();
        entity.m_updateRate = EditorGUILayout.Slider("Replication Rate", entity.m_updateRate, 0, 1);
        int sendRate = (int)(Config.instance.sendRate * entity.m_updateRate);
        GUILayout.Label(sendRate + " Packets per Second");
        GUILayout.EndHorizontal();        
        entity.m_persistThroughSceneLoads = EditorGUILayout.Toggle("Persistent", entity.m_persistThroughSceneLoads, new GUILayoutOption[0]);

        if (entity.m_entity != null)
            RuntimeInfoGUI(entity);
        else
            SaveEntity(entity);
    }

    private void DrawPrefabInfo(SkyEntity entity)
    {
        PrefabType prefabType = PrefabUtility.GetPrefabType(entity.gameObject);
        EditorGUILayout.LabelField("Type", prefabType.ToString(), new GUILayoutOption[0]);
        EditorGUILayout.LabelField("Scene Id", entity.sceneGuid.ToString(), new GUILayoutOption[0]);        
        EditorGUILayout.LabelField("Serializer Guid", entity.serializerGuid.ToString(), new GUILayoutOption[0]);
        switch (prefabType)
        {
            case PrefabType.None:
                if (entity.m_prefabId != 0)
                {
                    entity.m_prefabId = 0;
                    EditorUtility.SetDirty(this);
                }
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Prefab Id", entity.m_prefabId, new GUILayoutOption[0]);
                EditorGUI.EndDisabledGroup();
                break;
            case PrefabType.Prefab:
            case PrefabType.PrefabInstance:
                EditorGUILayout.LabelField("Prefab Id", entity.m_prefabId.ToString(), new GUILayoutOption[0]);
                if (entity.m_prefabId < 0)
                    EditorGUILayout.HelpBox("Prefab id not set, run the 'Assets/SkyNet Engine/Compile Assembly' menu option to correct", MessageType.Error);
                if (!PrefabDatabase.Contains(entity))
                {
                    EditorGUILayout.HelpBox("Prefab lookup not valid, run the 'Assets/SkyNet Engine/Compile Assembly' menu option to correct", MessageType.Error);
                    break;
                }                
                break;
            case PrefabType.DisconnectedPrefabInstance:
                entity.m_prefabId = EditorGUILayout.IntField("Prefab Id", entity.m_prefabId, new GUILayoutOption[0]);
                if (entity.m_prefabId < 0)
                {
                    EditorGUILayout.HelpBox("Prefab Id not set", MessageType.Error);
                    break;
                }                
                break;
        }
    }

    private void EditState(SkyEntity entity)
    {
        int num = EditorGUILayout.Popup("State", System.Math.Max(0, Array.IndexOf<UniqueId>(serializerIds, entity.serializerGuid) + 1), serializerNames, new GUILayoutOption[0]);
        if (num == 0)
        {
            entity.serializerGuid = UniqueId.None;
            EditorGUILayout.HelpBox("You must assign a state to this prefab before using it", MessageType.Error);
        }
        else
            entity.serializerGuid = serializerIds[num - 1];
    }

    private void SaveEntity(SkyEntity entity)
    {
        if (!Application.isPlaying && GUI.changed)
        {
            EditorUtility.SetDirty(entity.gameObject);
        }
    }

    private void RuntimeInfoGUI(SkyEntity entity)
    {
        SkyManager.DebugDrawer.IsEditor(true);
        GUILayout.Label("Runtime Info", EditorStyles.boldLabel, new GUILayoutOption[0]);        
        EditorGUILayout.LabelField("Network Id", entity.networkId.ToString(), new GUILayoutOption[0]);
        EditorGUILayout.Toggle("Is Owner", entity.isOwner, new GUILayoutOption[0]);
        GUILayout.Label("Serializer Debug Info", EditorStyles.boldLabel, new GUILayoutOption[0]);
        entity.Entity.Serializer.DebugInfo();
    }

    private void SetIcons()
    {
        // this sets the icon on the game object containing our behaviour
        (target as SkyEntity).gameObject.SetIcon(Resources.Load<Texture2D>("SkyEntity Icon"));

        // this sets the icon on the script (which normally shows the blank page icon)
        MonoScript.FromMonoBehaviour(target as SkyEntity).SetIcon(Resources.Load<Texture2D>("SkyEntity Icon"));
    }
}