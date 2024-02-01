using UnityEngine;
using UnityEditor;
using SkyNet.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using SkyNet.Unity.Utils;
using SkyNet;

internal abstract class SkyWindow : EditorWindow
{
    private float m_repaintTime;
    protected int m_repaints;
    private bool m_clear;    

    protected static AssetDefinition m_asset;

    protected static Dictionary<string, StateDefinition> m_states = new Dictionary<string, StateDefinition>();
    protected static Dictionary<string, EventDefinition> m_events = new Dictionary<string, EventDefinition>();
    protected static Dictionary<string, ObjDefinition> m_objects = new Dictionary<string, ObjDefinition>();

    protected void Update()
    {
        if (m_repaints <= 0 && m_repaintTime + 0.05f >= Time.realtimeSinceStartup)
            return;
        Repaint();
        m_repaintTime = Time.realtimeSinceStartup;
    }

    protected void OnGUI()
    {
        if (UnityEngine.Event.current.keyCode == KeyCode.Return && UnityEngine.Event.current.type == EventType.KeyDown && (UnityEngine.Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control)
        {
            UnityEngine.Event.current.Use();
            
        }
        if (UnityEngine.Event.current.type == EventType.Repaint)
            m_repaints = Mathf.Max(0, m_repaints - 1);
        if (m_asset == null || !m_asset.Delete)
            return;
        m_asset = null;
    }

    protected void ClearAllFocus()
    {
        if (UnityEngine.Event.current.type != EventType.Repaint)
            return;
        if (m_repaints == 0)
        {
            m_clear = false;
        }
        else
        {
            if (!m_clear)
                return;
            GUI.SetNextControlName("ClearFocusFix");
            GUI.Button(new Rect(0.0f, 0.0f, 0.0f, 0.0f), "", GUIStyle.none);
            GUI.FocusControl("ClearFocusFix");
        }
    }

    protected void BeginClearFocus()
    {
        m_clear = true;
        m_repaints = 10;
        GUIUtility.keyboardControl = 0;
    }

    protected void LoadAssets()
    {
        m_states.Clear();
        m_events.Clear();
        m_objects.Clear();

        string[] files = Directory.GetFiles(Util.SavePath, "*.xml", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            string type = Path.GetFileName(files[i]).Split('.')[0];
            XmlReader xmlReader = XmlReader.Create(files[i]);

            if (type == "state")
            {
                XmlSerializer xs = new XmlSerializer(typeof(StateDefinition));
                var state = (StateDefinition)xs.Deserialize(xmlReader);
                state.FileName = files[i];
                m_states.Add(files[i], state);
            }

            if (type == "event")
            {
                XmlSerializer xs = new XmlSerializer(typeof(EventDefinition));
                var evnt = (EventDefinition)xs.Deserialize(xmlReader);
                evnt.FileName = files[i];
                m_events.Add(files[i], evnt);
            }

            if (type == "obj")
            {
                XmlSerializer xs = new XmlSerializer(typeof(ObjDefinition));
                var obj = (ObjDefinition)xs.Deserialize(xmlReader);
                obj.FileName = files[i];
                m_objects.Add(files[i], obj);                
            }

            xmlReader.Close();
        }
    }

    protected void SaveAssets()
    {
        string[] files = Directory.GetFiles(Util.SavePath, "*.xml", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
            File.Delete(files[i]);

        foreach (var item in m_states)
        {
            item.Value.FileName = Util.MakePath(Util.SavePath, "state." + item.Value.Name + ".xml");
            XmlSerializer xs = new XmlSerializer(typeof(StateDefinition));
            XmlWriter writer = XmlWriter.Create(item.Value.FileName);
            xs.Serialize(writer, item.Value);
            writer.Close();
        }
        foreach (var item in m_events)
        {
            item.Value.FileName = Util.MakePath(Util.SavePath, "event." + item.Value.Name + ".xml");
            XmlSerializer xs = new XmlSerializer(typeof(EventDefinition));
            XmlWriter writer = XmlWriter.Create(item.Value.FileName);
            xs.Serialize(writer, item.Value);
            writer.Close();
        }
        foreach (var item in m_objects)
        {
            item.Value.FileName = Util.MakePath(Util.SavePath, "obj." + item.Value.Name + ".xml");
            XmlSerializer xs = new XmlSerializer(typeof(ObjDefinition));
            XmlWriter writer = XmlWriter.Create(item.Value.FileName);
            xs.Serialize(writer, item.Value);
            writer.Close();
        }
    }

    protected void CompileAssets()
    {
        SaveAssets();
        SkyCompiler.CreateAssets(m_states, m_events, m_objects);       
        SkyGeneratedCompiler.Run().WaitOne();        
        LoadAssets();
        AssetDatabase.Refresh();
    }

    protected void CheckDelete()
    {
        List<string> stateRemove = new List<string>();
        List<string> eventRemove = new List<string>();
        List<string> objRemove = new List<string>();

        foreach (var item in m_states)
        {           
            if (item.Value.Delete)
            {
                stateRemove.Add(item.Key);
            }
        }
        foreach (var item in m_events)
        {
            if (item.Value.Delete)
            {
                eventRemove.Add(item.Key);
            }
        }
        foreach (var item in m_objects)
        {
            if (item.Value.Delete)
            {
                objRemove.Add(item.Key);
            }
        }

        for (int i = 0; i < stateRemove.Count; i++)
        {
            m_states.Remove(stateRemove[i]);
        }
        for (int i = 0; i < eventRemove.Count; i++)
        {
            m_events.Remove(eventRemove[i]);
        }
        for (int i = 0; i < objRemove.Count; i++)
        {
            m_objects.Remove(objRemove[i]);
        }
        LoadAssets();
        AssetDatabase.Refresh();
    }    
}