using SkyNet;
using SkyNet.Compiler;
using SkyNet.Unity.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal static class SkyCompiler
{
    private static int m_typeID = 250;

    #region Prefabs 
    public static void CompilePrefabs()
    {
        UpdatePrefabsDatabase();

        for (int index = 0; index < PrefabDatabase.Instance.Prefabs.Count; ++index)
        {
            GameObject prefab = PrefabDatabase.Instance.Prefabs[index];
            SkyEntity component = prefab.GetComponent<SkyEntity>();
            if (component && component.sceneGuid != UniqueId.None)
            {
                component.m_sceneGuid = "";
                EditorUtility.SetDirty(prefab);
                EditorUtility.SetDirty(component);
                AssetDatabase.SaveAssets();
            }
        }

        SkySourceFile file = new SkySourceFile(Util.MakePath(Util.SkyNetGenFilesPath, "SkyPrefabs.cs"));
        file.EmitScope("public static class SkyPrefabs", (() =>
        {
            for (int index = 0; index < PrefabDatabase.Instance.Prefabs.Count; ++index)
            {
                GameObject prefab = PrefabDatabase.Instance.Prefabs[index];
                if (prefab)
                    file.EmitLine("public static readonly SkyNet.PrefabId {0} = new SkyNet.PrefabId({1});", prefab.name, prefab.GetComponent<SkyEntity>().m_prefabId);
            }
        }));
        file.Save();
    }

    //private static List<SkyPrefab> FindPrefabs()
    //{
    //    List<SkyPrefab> prefabs = new List<SkyPrefab>();
    //    int id = 1;
    //    string[] files = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
    //    for (int i = 0; i < files.Length; ++i)
    //    {
    //        SkyEntity entity = AssetDatabase.LoadAssetAtPath(files[i], typeof(SkyEntity)) as SkyEntity;
    //        if (entity != null)
    //        {
    //            entity.m_prefabId = id;
    //            entity.m_sceneGuid = null;
    //            EditorUtility.SetDirty(entity.gameObject);
    //            EditorUtility.SetDirty(entity);
    //            prefabs.Add(
    //                new SkyPrefab()
    //                {
    //                    go = entity.gameObject,
    //                    id = id,
    //                    name = entity.gameObject.name
    //                });
    //            ++id;
    //        }
    //        EditorUtility.DisplayProgressBar("Updating SkyNet Prefab Database", "Scanning for prefabs ...", Mathf.Clamp01((float)i / (float)files.Length));
    //    }

    //    return prefabs;
    //}

    private static void UpdatePrefabsDatabase()
    {
        PrefabDatabase.Instance.Prefabs = new List<GameObject>();

        int id = 0;
        string[] files = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; ++i)
        {
            SkyEntity entity = AssetDatabase.LoadAssetAtPath(files[i], typeof(SkyEntity)) as SkyEntity;
            if (entity != null)
            {
                entity.m_prefabId = id;
                entity.m_sceneGuid = null;
                EditorUtility.SetDirty(entity.gameObject);
                EditorUtility.SetDirty(entity);
                PrefabDatabase.Instance.Prefabs.Add(entity.gameObject);
                Debug.Log(string.Format("Assigned [PrefabId:{0}] to '{1}'", id, AssetDatabase.GetAssetPath(entity.gameObject)));
                ++id;
            }
            EditorUtility.DisplayProgressBar("Updating Prefab Database", "Scanning for prefabs ...", Mathf.Clamp01((float)i / (float)files.Length));
        }

        EditorUtility.SetDirty(PrefabDatabase.Instance);
        EditorUtility.ClearProgressBar();
    }
    #endregion

    #region CreateAssets

    public static void CreateAssets(Dictionary<string, StateDefinition> _states, Dictionary<string, EventDefinition> _events, Dictionary<string, ObjDefinition> _objects)
    {
        m_typeID = 250;

        Createobjects(_objects);

        foreach (var item in _states)
        {
            CreateState(item.Value);
        }
        foreach (var item in _events)
        {
            CreateEvent(item.Value);
        }        

        CreateFactoryRegister(_states, _events);
        CreateGlobalEventListener(_events);
        CreateEntityEventListener(_events);
        CreateEntityEventListenerState(_events);
        CompilePrefabs();
    }

    private static void CreateState(StateDefinition _state)
    {
        m_typeID++;

        SkySourceFile file1 = new SkySourceFile(Util.MakePath(Util.SkyNetGenStatesPath, "I" + _state.Name + ".cs"));
        file1.EmitLine("using SkyNet;");
        file1.EmitLine("using System;");
        file1.EmitLine("using UnityEngine;");

        file1.EmitScope("public interface I" + _state.Name + ": IState, IDisposable", (() =>
        {
            foreach (var item in _state.Properties)
            {
                if (NormalVar(item))
                    file1.EmitLine(item.Type.ToLower() + " " + item.Name + " { get; set; }");

                if(item.Type == "Array")
                    file1.EmitLine(item.ArrayDefinition.Type.ToLower() + "[] " + item.Name + " { get; set; }");
            }
        }));
        file1.Save();

        SkySourceFile file3 = new SkySourceFile(Util.MakePath(Util.SkyNetGenStatesPath, _state.Name + ".cs"));
        file3.EmitLine("using SkyNet;");
        file3.EmitLine("using System;");
        file3.EmitLine("using System.Collections.Generic;");        
        file3.EmitLine("using UnityEngine;");

        file3.EmitScope("internal class " + _state.Name + ": NetworkState, I" + _state.Name + ", IState, IDisposable", (() =>
        {
            file3.EmitLine("internal static " + _state.Name + " Instance = new " + _state.Name + "();");

            CreateVars(file3, _state.Properties);

            file3.EmitScope("public " + _state.Name + "() : base(" + _state.Name + "_Data.Instance)", (() =>
            {
                file3.EmitLine("flag = PacketFlags." + _state.PacketFlag.ToString() + ";");
                file3.EmitLine("targets = Targets." + _state.PacketTarget.ToString() + ";");

                InitVars(file3, _state.Properties, false);
            }));

            file3.EmitScope("public override void OnCreated(Entity _entity)", (() =>
            {
                InitTransform(file3, _state.Properties);                
            }));

            file3.EmitScope("public override void Pack(SkyNet.NetBuffer _stream)", (() =>
            {
                PackVars(file3, _state.Properties);
                file3.EmitLine("Debug();");
            }));

            file3.EmitScope("public override void Unpack(SkyNet.NetBuffer _stream)", (() =>
            {
                UnpackVars(file3, _state.Properties);                
                file3.EmitLine("Debug();");
            }));

            file3.EmitScope("public override void OnNotOwner()", (() =>
            {
                OnRemote(file3, _state.Properties);                
            }));

            file3.EmitScope("protected void Debug()", (() =>
            {
                DebugShow(file3, _state.Properties);               
            }));
        }));
        file3.Save();

        SkySourceFile file2 = new SkySourceFile(Util.MakePath(Util.SkyNetGenStatesPath, _state.Name + "_Data.cs"));
        file2.EmitLine("using SkyNet;");
        file2.EmitLine("using System;");
        file2.EmitScope("internal class " + _state.Name + "_Data: NetworkState_Meta, ISerializerFactory, IFactory", (() =>
        {
            file2.EmitLine("internal static {0}_Data Instance = new {0}_Data();", _state.Name);
            file2.EmitLine("internal ObjectPool<{0}> _pool = new ObjectPool<{0}>();", _state.Name);

            file2.EmitScope("static " + _state.Name + "_Data()", (() =>
            {
                file2.EmitLine("Instance.InitData();");
            }));

            file2.EmitScope("internal void InitData()", (() =>
            {
                file2.EmitLine("TypeId = new TypeId({0});", m_typeID);
            }));

            file2.EmitScope("Type IFactory.TypeObject", (() =>
            {
                file2.EmitScope("get", (() =>
                {
                    file2.EmitLine("return typeof(I" + _state.Name + ");");
                }));
            }));

            file2.EmitScope("TypeId IFactory.TypeID", (() =>
            {
                file2.EmitScope("get", (() =>
                {
                    file2.EmitLine("return TypeId;");
                }));
            }));

            file2.EmitScope("UniqueId IFactory.TypeKey", (() =>
            {
                file2.EmitScope("get", (() =>
                {
                    file2.EmitLine("return new UniqueId(\"{0}\");", _state.UniqueId);
                }));
            }));

            file2.EmitScope("object IFactory.Create()", (() =>
            {
                file2.EmitLine("return _pool.Get();");
            }));
        }));
        file2.Save();
    }

    private static void CreateEvent(EventDefinition _evnt)
    {
        m_typeID++;

        SkySourceFile file1 = new SkySourceFile(Util.MakePath(Util.SkyNetGenEventsPath, "I" + _evnt.Name + "Listener.cs"));
        file1.EmitLine("using SkyNet;");
        file1.EmitLine("using System;");

        file1.EmitScope("public interface I" + _evnt.Name + "Listener", (() =>
        {
            file1.EmitLine("void OnEvent(" + _evnt.Name + " _evnt);");
        }));
        file1.Save();

        SkySourceFile file2 = new SkySourceFile(Util.MakePath(Util.SkyNetGenEventsPath, _evnt.Name + ".cs"));
        file2.EmitLine("using SkyNet;");
        file2.EmitLine("using System;");
        file2.EmitLine("using UnityEngine;");

        file2.EmitScope("public class " + _evnt.Name + " : SkyNet.Event", (() =>
        {
            file2.EmitLine("internal static " + _evnt.Name + " Instance = new " + _evnt.Name + "();");

            CreateVars(file2, _evnt.Properties);

            file2.EmitScope("public " + _evnt.Name + "() : base((" + _evnt.Name + "_Data) " + _evnt.Name + "_Data.Instance)", (() =>
            {
                file2.EmitLine("m_flag = PacketFlags." + _evnt.PacketFlag + ";");

                InitVars(file2, _evnt.Properties, true);

                InitTransform(file2, _evnt.Properties);
            }));

            file2.EmitScope("internal override void Unpack(SkyNet.NetBuffer _stream)", (() =>
            {
                UnpackVars(file2, _evnt.Properties);
            }));

            file2.EmitScope("internal override void Pack(SkyNet.NetBuffer _stream)", (() =>
            {
                PackVars(file2, _evnt.Properties);
            }));

            file2.EmitScope("public void Send(Targets _targets)", (() =>
            {
                file2.EmitLine("SkyManager.Send(this, m_flag, _targets);");
            }));

            file2.EmitScope("public void Send(Targets _targets, PacketFlags _flag)", (() =>
            {
                file2.EmitLine("SkyManager.Send(this, _flag, _targets);");
            }));

            file2.EmitScope("public void Send(Connection _target)", (() =>
            {
                file2.EmitLine("SkyManager.Send(this, m_flag, _target);");
            }));

            file2.EmitScope("public void Send(Connection _target, PacketFlags _flag)", (() =>
            {
                file2.EmitLine("SkyManager.Send(this, _flag, _target);");
            }));
        }));
        file2.Save();

        SkySourceFile file3 = new SkySourceFile(Util.MakePath(Util.SkyNetGenEventsPath, _evnt.Name + "_Data.cs"));
        file3.EmitLine("using SkyNet;");
        file3.EmitLine("using System;");

        file3.EmitScope("internal class " + _evnt.Name + "_Data : Event_Data, IEventFactory, IFactory", (() =>
        {
            file3.EmitLine("internal static " + _evnt.Name + "_Data Instance = new " + _evnt.Name + "_Data();");
            file3.EmitLine("internal ObjectPool<" + _evnt.Name + "> _pool = new ObjectPool<" + _evnt.Name + ">();");

            file3.EmitScope("static " + _evnt.Name + "_Data()", (() =>
            {
                file3.EmitLine("Instance.InitData();");
            }));

            file3.EmitScope("internal void InitData()", (() =>
            {
                file3.EmitLine("TypeId = new TypeId(" + m_typeID + ");");
            }));

            file3.EmitScope("Type IFactory.TypeObject", (() =>
            {
                file3.EmitLine("get { return typeof(" + _evnt.Name + "); }");
            }));

            file3.EmitScope("TypeId IFactory.TypeID", (() =>
            {
                file3.EmitLine("get { return TypeId; }");
            }));

            file3.EmitScope("UniqueId IFactory.TypeKey", (() =>
            {
                file3.EmitLine("get { return new UniqueId(\"" + _evnt.UniqueId + "\"); }");
            }));

            file3.EmitLine("object IFactory.Create() { return _pool.Get(); }");

            file3.EmitScope("public void Dispatch(Event _evnt, object _target)", (() =>
            {
                file3.EmitLine("I" + _evnt.Name + "Listener eventListener = _target as I" + _evnt.Name + "Listener;");
                file3.EmitLine("if (eventListener == null) return;");
                file3.EmitLine("eventListener.OnEvent((" + _evnt.Name + ")_evnt);");
            }));
        }));
        file3.Save();
    }

    private static void Createobjects(Dictionary<string, ObjDefinition> _objects)
    {
        if (_objects == null || _objects.Count == 0) return;
        SkySourceFile file1 = new SkySourceFile(Util.MakePath(Util.SkyNetGenObjectsPath, "Objects.cs"));
        file1.EmitLine("using SkyNet;");
        file1.EmitLine("using System;");
        file1.EmitLine("using UnityEngine;");

        foreach (var obj in _objects)
        {
            file1.EmitScope("public class " + obj.Value.Name+ " : INetworkPackage", (() =>
            {
                CreateVars(file1, obj.Value.Properties);

                file1.EmitScope("public " + obj.Value.Name + "()", (() =>
                {
                    InitVars(file1, obj.Value.Properties, true);

                    InitTransform(file1, obj.Value.Properties);
                }));

                file1.EmitScope("public void Unpack(SkyNet.NetBuffer _stream)", (() =>
                {
                    UnpackVars(file1, obj.Value.Properties);
                }));

                file1.EmitScope("public void Pack(SkyNet.NetBuffer _stream)", (() =>
                {
                    PackVars(file1, obj.Value.Properties);
                }));
            }));
        }
        
        file1.Save();
    }

    private static void CreateFactoryRegister(Dictionary<string, StateDefinition> _states, Dictionary<string, EventDefinition> _events)
    {
        SkySourceFile file1 = new SkySourceFile(Util.MakePath(Util.SkyNetGenFilesPath, "FactoryRegister.cs"));
        file1.EmitScope("namespace SkyNet", (() =>
        {
            file1.EmitScope("public class FactoryRegister : IFactoryRegister", (() =>
            {
                file1.EmitScope("public void EnvironmentSetup()", (() =>
                {
                    foreach (var item in _states) 
                    {
                        file1.EmitLine("Factory.Register(" + item.Value.Name + "_Data.Instance);");
                    }

                    foreach (var item in _events)
                    {
                        file1.EmitLine("Factory.Register(" + item.Value.Name + "_Data.Instance);");
                    }
                }));
            }));
        }));
        file1.Save();
    }

    private static void CreateGlobalEventListener(Dictionary<string, EventDefinition> _events)
    {
        string evnts = "";

        foreach (var item in _events)
            evnts += ",I" + item.Value.Name + "Listener";

        SkySourceFile file1 = new SkySourceFile(Util.MakePath(Util.SkyNetGenFilesPath, "GlobalEventListener.cs"));
        file1.EmitScope("namespace SkyNet", (() =>
        {
            file1.EmitScope("public class GlobalEventListener : GlobalEventListenerBase" + evnts, (() =>
            {
                foreach (var item in _events)
                    file1.EmitLine("public virtual void OnEvent(" + item.Value.Name + " evnt) {}");
            }));
        }));
        file1.Save();
    }

    private static void CreateEntityEventListener(Dictionary<string, EventDefinition> _events)
    {
        string evnts = "";

        foreach (var item in _events)
        {
            if (!item.Value.GlobalTarget)
                evnts += ",I" + item.Value.Name + "Listener";
        }

        SkySourceFile file1 = new SkySourceFile(Util.MakePath(Util.SkyNetGenFilesPath, "EntityEventListener.cs"));
        file1.EmitScope("namespace SkyNet", (() =>
        {
            file1.EmitScope("public class EntityEventListener : EntityEventListenerBase" + evnts, (() =>
            {
                foreach (var item in _events)
                {
                    if (!item.Value.GlobalTarget)
                        file1.EmitLine("public virtual void OnEvent(" + item.Value.Name + " evnt) {}");
                }
            }));
        }));
        file1.Save();
    }

    private static void CreateEntityEventListenerState(Dictionary<string, EventDefinition> _events)
    {
        string evnts = "";

        foreach (var item in _events)
        {
            if (!item.Value.GlobalTarget)
                evnts += ",I" + item.Value.Name + "Listener";
        }

        SkySourceFile file1 = new SkySourceFile(Util.MakePath(Util.SkyNetGenFilesPath, "EntityEventListenerState.cs"));
        file1.EmitScope("namespace SkyNet", (() =>
        {
            file1.EmitScope("public class EntityEventListener<TState> : EntityEventListenerBase<TState>" + evnts, (() =>
            {
                foreach (var item in _events)
                {
                    if (!item.Value.GlobalTarget)
                        file1.EmitLine("public virtual void OnEvent(" + item.Value.Name + " evnt) {}");
                }
            }));
        }));
        file1.Save();
    }

    private static void CreateVars(SkySourceFile _file, List<PropertyDefinition> _props)
    {
        foreach (var item in _props)
        {
            if (item.Type == "Transform")
            {
                _file.EmitLine("private NetworkTransform m_netTransform_" + item.Name + ";");

                _file.EmitLine("private List<CompressorFloat> m_posCompressor_" + item.Name + " = new List<CompressorFloat>();");
                _file.EmitLine("private List<CompressorFloat> m_rotCompressor_" + item.Name + " = new List<CompressorFloat>();");
                _file.EmitLine("private List<CompressorFloat> m_velCompressor_" + item.Name + " = new List<CompressorFloat>();");
                _file.EmitLine("private List<CompressorFloat> m_aVelCompressor_" + item.Name + " = new List<CompressorFloat>();");
            }
            else if (item.Type == "Vector")
            {
                _file.EmitLine("private List<CompressorFloat> m_v3Compressor_" + item.Name + " = new List<CompressorFloat>();");
            }
            else if (item.Type == "Quaternion")
            {
                _file.EmitLine("private List<CompressorFloat> m_qCompressor_" + item.Name + " = new List<CompressorFloat>();");
            }
            else if (item.Type == "Float" && item.FloatCompression.Enabled)
            {
                _file.EmitLine("private CompressorFloat m_fCompressor_" + item.Name + " = new CompressorFloat(" + item.FloatCompression.minValue + "f, " + item.FloatCompression.maxValue + "f, " + item.FloatCompression.precision.ToString().Replace(',', '.') + "f, " + item.FloatCompression.Enabled.ToString().ToLower() + ");");
            }
            else if (item.Type == "Integer" && item.IntCompression.Enabled)
            {
                _file.EmitLine("private CompressorInt m_iCompressor_" + item.Name + " = new CompressorInt(" + item.IntCompression.minValue + "f, " + item.IntCompression.maxValue + "f, " + item.IntCompression.Enabled.ToString().ToLower() + ");");
            }
            else
            {
                if(item.Type == "Object")
                    _file.EmitLine("public " + item.objDefinition.Name + " " + item.Name + " { get; set; }");
                else if (item.Type == "Array")
                    _file.EmitLine("public " + item.ArrayDefinition.Type.ToLower() + "[] " + item.Name + " { get; set; }");
                else if (item.Type == "Entity")
                    _file.EmitLine("public SkyEntity " + item.Name + " { get; set; }");
                else if (item.Type == "NetworkId")
                    _file.EmitLine("public NetworkId " + item.Name + " { get; set; }");
                else
                    _file.EmitLine("public " + item.Type.ToLower() + " " + item.Name + " { get; set; }");
            }
        }
    }

    private static void InitVars(SkySourceFile _file, List<PropertyDefinition> _props, bool _objectDef)
    {
        foreach (var item in _props)
        {
            if (item.objDefinition != null) continue;

            if (item.Type == "Transform")
            {
                if(!_objectDef)
                {
                    _file.EmitLine("m_debug.Add(\"Position " + item.Name + "\", null);");
                    _file.EmitLine("m_debug.Add(\"Rotation " + item.Name + "\", null);");
                }                

                for (int i = 0; i < item.PositionCompression.Length; i++)
                    _file.EmitLine("m_posCompressor_" + item.Name + ".Add(new CompressorFloat(" + item.PositionCompression[i].minValue + "f, " + item.PositionCompression[i].maxValue + "f, " + item.PositionCompression[i].precision.ToString().Replace(',', '.') + "f, " + item.PositionCompression[i].Enabled.ToString().ToLower() + ")); ");

                for (int i = 0; i < item.RotationCompression.Length; i++)
                    _file.EmitLine("m_rotCompressor_" + item.Name + ".Add(new CompressorFloat(" + item.RotationCompression[i].minValue + "f, " + item.RotationCompression[i].maxValue + "f, " + item.RotationCompression[i].precision.ToString().Replace(',', '.') + "f, " + item.RotationCompression[i].Enabled.ToString().ToLower() + ")); ");

                for (int i = 0; i < item.VelocityCompression.Length; i++)
                    _file.EmitLine("m_velCompressor_" + item.Name + ".Add(new CompressorFloat( " + item.VelocityCompression[i].minValue + "f, " + item.VelocityCompression[i].maxValue + "f, " + item.VelocityCompression[i].precision.ToString().Replace(',', '.') + "f, " + item.VelocityCompression[i].Enabled.ToString().ToLower() + ")); ");

                for (int i = 0; i < item.AngularVelocityCompression.Length; i++)
                    _file.EmitLine("m_aVelCompressor_" + item.Name + ".Add(new CompressorFloat( " + item.AngularVelocityCompression[i].minValue + "f, " + item.AngularVelocityCompression[i].maxValue + "f, " + item.AngularVelocityCompression[i].precision.ToString().Replace(',', '.') + "f, " + item.AngularVelocityCompression[i].Enabled.ToString().ToLower() + ")); ");
            }
            else if (item.Type == "Vector")
            {
                for (int i = 0; i < item.PositionCompression.Length; i++)
                    _file.EmitLine("m_v3Compressor_" + item.Name + ".Add(new CompressorFloat(" + item.PositionCompression[i].minValue + "f, " + item.PositionCompression[i].maxValue + "f, " + item.PositionCompression[i].precision.ToString().Replace(',', '.') + "f, " + item.PositionCompression[i].Enabled.ToString().ToLower() + ")); ");
            }
            else if (item.Type == "Quaternion")
            {
                for (int i = 0; i < item.RotationCompression.Length; i++)
                    _file.EmitLine("m_qCompressor_" + item.Name + ".Add(new CompressorFloat(" + item.RotationCompression[i].minValue + "f, " + item.RotationCompression[i].maxValue + "f, " + item.RotationCompression[i].precision.ToString().Replace(',', '.') + "f, " + item.RotationCompression[i].Enabled.ToString().ToLower() + ")); ");
            }
            else if (item.Type == "Object")
            {
                _file.EmitLine(item.Name + " = new " + item.objDefinition.Name + "();");
            }
            else if (item.Type == "Array")
            {
                _file.EmitLine(item.Name + " = new " + item.ArrayDefinition.Type.ToLower() + "["+ item.ArrayDefinition.Count+"]; ");
            }
            else
            {
                if (!_objectDef)
                {
                    _file.EmitLine("m_debug.Add(\"" + item.Name + "\", " + item.Name + ");");
                }
            }
        }
    }

    private static void InitTransform(SkySourceFile _file, List<PropertyDefinition> _props)
    {
        foreach (var item in _props)
        {
            if (item.Type != "Transform") return;

            _file.EmitLine("m_netTransform_" + item.Name + " = new NetworkTransform();");
            _file.EmitLine("m_netTransform_" + item.Name + ".m_interpolationBackTime = " + item.InterpolationBackTime.ToString().Replace(',', '.') + "f; ");
            _file.EmitLine("m_netTransform_" + item.Name + ".m_extrapolationLimit = " + item.ExtrapolationLimit.ToString().Replace(',', '.') + "f;");
            _file.EmitLine("m_netTransform_" + item.Name + ".extrapolationDistanceLimit = " + item.ExtrapolationDistanceLimit.ToString().Replace(',', '.') + "f;");
            _file.EmitLine("m_netTransform_" + item.Name + ".positionSnapThreshold = " + item.PositionSnapThreshold + ";");
            _file.EmitLine("m_netTransform_" + item.Name + ".rotationSnapThreshold = " + item.RotationSnapThreshold + ";");
            _file.EmitLine("m_netTransform_" + item.Name + ".positionLerpSpeed = " + item.PositionLerpSpeed.ToString().Replace(',', '.') + "f;");
            _file.EmitLine("m_netTransform_" + item.Name + ".rotationLerpSpeed = " + item.RotationLerpSpeed.ToString().Replace(',', '.') + "f;");
            _file.EmitLine("m_netTransform_" + item.Name + ".syncPosition = SkyNet.Compiler.AxisSelection." + item.PosAxe + ";");
            _file.EmitLine("m_netTransform_" + item.Name + ".syncRotation = SkyNet.Compiler.AxisSelection." + item.RotAxe + ";");
            _file.EmitLine("m_netTransform_" + item.Name + ".syncVelocity = SkyNet.Compiler.AxisSelection." + item.VelAxe + ";");
            _file.EmitLine("m_netTransform_" + item.Name + ".syncAngularVelocity = SkyNet.Compiler.AxisSelection." + item.AVelAxe + ";");
            _file.EmitLine("m_netTransform_" + item.Name + ".m_posCompressor = m_posCompressor_" + item.Name + ";");
            _file.EmitLine("m_netTransform_" + item.Name + ".m_rotCompressor = m_rotCompressor_" + item.Name + ";");
            _file.EmitLine("m_netTransform_" + item.Name + ".m_velCompressor = m_velCompressor_" + item.Name + ";");
            _file.EmitLine("m_netTransform_" + item.Name + ".m_aVelCompressor = m_aVelCompressor_" + item.Name + ";");
            _file.EmitLine("m_netTransform_" + item.Name + ".realObjectToSync = _entity.UnityObject.gameObject;");
            _file.EmitLine("m_netTransform_" + item.Name + ".sendRate = Config.instance.sendRate * _entity.UpdateRate;");
            _file.EmitLine("m_netTransform_" + item.Name + ".Initialize();");
        }
    }

    private static void PackVars(SkySourceFile _file, List<PropertyDefinition> _props)
    {
        foreach (var item in _props)
        {
            if (item.Type == "Transform")
            {
                _file.EmitLine("if (m_netTransform_" + item.Name + " == null) return;");
                _file.EmitLine("m_netTransform_" + item.Name + ".Pack(_stream);");
            }
            else if (item.Type == "Float")
            {
                //if (item.FloatCompression.Enabled)
                //    _file.EmitLine("_stream.Write(" + item.Name + ", m_fCompressor_" + item.Name + ");");
                //else
                    _file.EmitLine("_stream.Write(" + item.Name + ");");
            }
            else if (item.Type == "Integer")
            {
                //if (item.IntCompression.Enabled)
                //    _file.EmitLine("_stream.WriteInt(" + item.Name + ", m_iCompressor_" + item.Name + ");");
                //else
                    _file.EmitLine("_stream.Write(" + item.Name + ");");
            }
            else if (item.Type == "Object")
            {
                _file.EmitLine(item.Name + ".Serialize(_stream);");
            }
            else if (item.Type == "Array")
            {
                _file.EmitScope("for (int i = 0; i < "+ item.Name + ".Length; i++)", (() =>
                {
                    _file.EmitLine("_stream." + GetWriter(item.ArrayDefinition.Type) + "(" + item.Name + "[i]);");
                }));
            }
            else if (item.Type == "Entity")
            {
                _file.EmitLine("_stream." + GetWriter(item.Type) + "(" + item.Name + ".networkId);");
            }
            else
            {
                _file.EmitLine("_stream." + GetWriter(item.Type) + "(" + item.Name + ");");
            }
        }
    }

    private static void UnpackVars(SkySourceFile _file, List<PropertyDefinition> _props)
    {
        foreach (var item in _props)
        {
            if (item.Type == "Transform")
            {
                _file.EmitLine("if (m_netTransform_" + item.Name + " == null) return;");
                _file.EmitLine("m_netTransform_" + item.Name + ".Unpack(_stream);");
            }
            else if (item.Type == "Float")
            {
                //if (item.FloatCompression.Enabled)
                //    _file.EmitLine(item.Name + " = _stream.ReadSingle(m_fCompressor_" + item.Name + ");");
                //else
                    _file.EmitLine(item.Name + " = _stream.ReadSingle();");
            }
            else if (item.Type == "Integer")
            {
                //if (item.IntCompression.Enabled)
                //    _file.EmitLine(item.Name + " = _stream.ReadInt(m_iCompressor_" + item.Name + ");");
                //else
                    _file.EmitLine(item.Name + " = _stream.ReadInt32();");
            }
            else if (item.Type == "Object")
            {
                _file.EmitLine(item.Name + ".Deserialize(_stream);");
            }
            else if (item.Type == "Array")
            {
                _file.EmitScope("for (int i = 0; i < " + item.Name + ".Length; i++)", (() =>
                {
                    _file.EmitLine(item.Name + "[i] = _stream." + GetReader(item.ArrayDefinition.Type) + "();");
                }));
            }
            else if (item.Type == "Entity")
            {
                _file.EmitLine(item.Name + " = SkyManager.FindEntity(_stream." + GetReader(item.Type) + "());");
            }
            else
            {
                _file.EmitLine(item.Name + " = _stream." + GetReader(item.Type) + "();");
            }
        }
    }

    private static void OnRemote(SkySourceFile _file, List<PropertyDefinition> _props)
    {
        foreach (var item in _props)
        {
            if (item.Type == "Transform")
            {
                _file.EmitLine("if (m_netTransform_" + item.Name + " == null) return;");
                _file.EmitLine("m_netTransform_" + item.Name + ".InterpolationOrExtrapolation();");

                if (item.objDefinition != null)
                {
                    foreach (var prop in item.objDefinition.Properties)
                    {
                        if (prop.Type == "Transform")
                        {
                            _file.EmitLine("if (m_netTransform_" + prop.Name + " == null) return;");
                            _file.EmitLine("m_netTransform_" + prop.Name + ".InterpolationOrExtrapolation();");
                        }
                    }
                }
            }            
        }
    }

    private static void DebugShow(SkySourceFile _file, List<PropertyDefinition> _props)
    {
        foreach (var item in _props)
        {
            if (item.Type == "Transform")
            {
                _file.EmitLine("if (m_netTransform_" + item.Name + " == null) return;");
                _file.EmitLine("m_debug[\"Position " + item.Name + "\"] = m_netTransform_" + item.Name + ".getPosition();");
                _file.EmitLine("m_debug[\"Rotation " + item.Name + "\"] = m_netTransform_" + item.Name + ".getRotation();");

                if (item.objDefinition != null)
                {
                    foreach (var prop in item.objDefinition.Properties)
                    {
                        if (prop.Type == "Transform")
                        {
                            _file.EmitLine("if (m_netTransform_" + prop.Name + " == null) return;");
                            _file.EmitLine("m_debug[\"Position " + prop.Name + "\"] = m_netTransform_" + prop.Name + ".getPosition();");
                            _file.EmitLine("m_debug[\"Rotation " + prop.Name + "\"] = m_netTransform_" + prop.Name + ".getRotation();");
                        }
                        else
                        {
                            _file.EmitLine("m_debug[\"" + item.Name + "\"] = " + item.Name + ";");
                        }
                    }
                }
            }
            else
            {
                _file.EmitLine("m_debug[\"" + item.Name + "\"] = " + item.Name + ";");
            }
        }
    }

    private static bool NormalVar(PropertyDefinition _prop)
    {
        if (_prop.Type == "Transform" || _prop.objDefinition != null || _prop.Type == "Array")
        {
            return false;
        }
        return true;
    }

    private static string GetReader(string _type)
    {
        string msg = "";
        switch (_type)
        {
            case "Bool":
                msg = "ReadBoolean";
                break;
            case "Float":
                msg = "ReadSingle";
                break;
            case "Integer":
                msg = "ReadInt32";
                break;
            case "String":
                msg = "ReadString";
                break;
            case "Vector":
                msg = "ReadVector3";
                break;
            case "Quaternion":
                msg = "ReadQuaternion";
                break;
            case "Entity":
            case "NetworkId":
                msg = "ReadNetworkId";
                break;
        }
        return msg;
    }

    private static string GetWriter(string _type)
    {
        string msg = "";
        switch (_type)
        {
            case "Bool":
                msg = "Write";
                break;
            case "Float":
                msg = "Write";
                break;
            case "Integer":
                msg = "Write";
                break;
            case "String":
                msg = "Write";
                break;
            case "Vector":
                msg = "WriteVector3";
                break;
            case "Quaternion":
                msg = "WriteQuaternion";
                break;
            case "Entity":
            case "NetworkId":
                msg = "WriteNetworkId";
                break;
        }
        return msg;
    }
    #endregion
}