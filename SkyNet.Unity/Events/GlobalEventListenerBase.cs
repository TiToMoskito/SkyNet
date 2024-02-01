using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkyNet
{  
    public class GlobalEventListenerBase : MonoBehaviour
    {
        private static readonly List<GlobalEventListenerBase> callbacks = new List<GlobalEventListenerBase>();

        public void OnEnable()
        {
            GlobalEventListenerBase.callbacks.Add(this);
        }

        public void OnDisable()
        {
            GlobalEventListenerBase.callbacks.Remove(this);
        }

        public virtual void StartBegin() { }
        internal static void StartBeginInvoke()
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.StartBegin();
            }
        }
        public virtual void StartDone() { }
        internal static void StartDoneInvoke()
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.StartDone();
            }
        }
        public virtual void StartFailed() { }
        internal static void StartFailedInvoke()
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.StartFailed();
            }
        }
        public virtual void Shutdown() { }
        internal static void ShutdownInvoke()
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.Shutdown();
            }
        }
        public virtual void StreamDataReceived(Connection _conn, byte[] _data) { }
        internal static void StreamDataReceivedInvoke(Connection _conn, byte[] _data)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.StreamDataReceived(_conn, _data);
            }
        }
        public virtual void SceneLoadLocalBegin(int _channelID, string _map) { }
        internal static void SceneLoadLocalBeginInvoke(int _channelID, string _map)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.SceneLoadLocalBegin(_channelID, _map);
            }
        }
        public virtual void SceneLoadLocalDone(int _channelID, string _map) { }
        internal static void SceneLoadLocalDoneInvoke(int _channelID, string _map)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.SceneLoadLocalDone(_channelID, _map);
            }
        }
        public virtual void SceneLoadRemoteBegin(Connection _conn, int _channelID, string _map) { }
        internal static void SceneLoadRemoteBeginInvoke(Connection _conn, int _channelID, string _map)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.SceneLoadRemoteBegin(_conn, _channelID, _map);
            }
        }
        public virtual void SceneLoadRemoteDone(Connection _conn, int _channelID, string _map) { }
        internal static void SceneLoadRemoteDoneInvoke(Connection _conn, int _channelID, string _map)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.SceneLoadRemoteDone(_conn, _channelID, _map);
            }
        }
        public virtual void Connected(Connection _conn) { }
        internal static void ConnectedInvoke(Connection _conn)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.Connected(_conn);
            }
        }
        public virtual void ConnectFailed(Connection _conn, IProtocolToken _token) { }
        internal static void ConnectFailedInvoke(Connection _conn, IProtocolToken _token)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.ConnectFailed(_conn, _token);
            }
        }
        public virtual void ConnectRequest(Connection _conn, IProtocolToken _token) { }
        internal static void ConnectRequestInvoke(Connection _conn, IProtocolToken _token)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.ConnectRequest(_conn, _token);
            }
        }
        public virtual void ConnectAttempt(Connection _conn, IProtocolToken _token) { }
        internal static void ConnectAttemptInvoke(Connection _conn, IProtocolToken _token)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.ConnectAttempt(_conn, _token);
            }
        }
        public virtual void ConnectRefused(Connection _conn, IProtocolToken _token) { }
        internal static void ConnectRefusedInvoke(Connection _conn, IProtocolToken _token)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.ConnectRefused(_conn, _token);
            }
        }
        public virtual void Disconnected(Connection _conn) { }
        internal static void DisconnectedInvoke(Connection _conn)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.Disconnected(_conn);
            }
        }
        public virtual void ChannelJoined(Connection _conn, int _channelID, string _msg) { }
        internal static void ChannelJoinedInvoke(Connection _conn, int _channelID, string _msg)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.ChannelJoined(_conn, _channelID, _msg);
            }
        }
        public virtual void ChannelLeft(Connection _conn, int _channelID, string _msg) { }
        internal static void ChannelLeftInvoke(Connection _conn, int _channelID, string _msg)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.ChannelLeft(_conn, _channelID, _msg);
            }
        }
        public virtual void EntityAttached(SkyEntity go) { }
        internal static void EntityAttachedInvoke(SkyEntity go)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.EntityAttached(go);
            }
        }
        public virtual void EntityDetached(SkyEntity go) { }
        internal static void EntityDetachedInvoke(SkyEntity go)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.EntityDetached(go);
            }
        }
        public virtual void ControlOfEntityGained(SkyEntity _entity) { }
        internal static void ControlOfEntityGainedInvoke(SkyEntity _entity)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.ControlOfEntityGained(_entity);
            }
        }
        public virtual void ControlOfEntityLost(SkyEntity _entity) { }
        internal static void ControlOfEntityLostInvoke(SkyEntity _entity)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.ControlOfEntityLost(_entity);
            }
        }
        public virtual void EntityFrozen(SkyEntity entity){ }
        internal static void EntityFrozenInvoke(SkyEntity entity)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                try
                {
                    callback.EntityFrozen(entity);
                }
                catch (Exception ex)
                {
                    SkyLog.Exception(ex);
                }
            }
        }
        public virtual void EntityThawed(SkyEntity entity)  { }
        internal static void EntityThawedInvoke(SkyEntity entity)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                try
                {
                    callback.EntityThawed(entity);
                }
                catch (Exception ex)
                {
                    SkyLog.Exception(ex);
                }
            }
        }
        public virtual void Error(string _msg) { }
        internal static void ErrorInvoke(string _msg)
        {
            foreach (GlobalEventListenerBase callback in GlobalEventListenerBase.callbacks)
            {
                callback.Error(_msg);
            }
        }
    }
}
