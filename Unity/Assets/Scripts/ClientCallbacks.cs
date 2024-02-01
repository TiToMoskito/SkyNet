using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkyNet;

public class ClientCallbacks : GlobalEventListener
{
   
    public override void ConnectAttempt(Connection _conn, IProtocolToken _token)
    {
        SkyLog.Info("ConnectAttempt");
    }

    public override void ConnectFailed(Connection _conn, IProtocolToken _token)
    {
        SkyLog.Info("ConnectFailed");
    }

    public override void ConnectRefused(Connection _conn, IProtocolToken _token)
    {
        SkyLog.Info("ConnectRefused");
    }

    public override void ConnectRequest(Connection _conn, IProtocolToken _token)
    {
        SkyLog.Info("ConnectRequest");
    }

    public override void ControlOfEntityGained(SkyEntity _entity)
    {
        SkyLog.Info("ControlOfEntityGained");
    }

    public override void ControlOfEntityLost(SkyEntity _entity)
    {
        SkyLog.Info("ControlOfEntityLost");
    }

    public override void EntityFrozen(SkyEntity entity)
    {
        SkyLog.Info("EntityFrozen");
    }

    public override void EntityThawed(SkyEntity entity)
    {
        SkyLog.Info("EntityThawed");
    }

    public override void Error(string _msg)
    {
        SkyLog.Info("Error "+ _msg);
    }

    public override void Shutdown()
    {
        SkyLog.Info("Shutdown");
    }

    public override void StartBegin()
    {
        SkyLog.Info("StartBegin");
    }

    public override void StartDone()
    {
        SkyLog.Info("StartDone");                
    }

    public override void StartFailed()
    {
        SkyLog.Info("StartFailed");
    }

    public override void StreamDataReceived(Connection _conn, byte[] _data)
    {
        SkyLog.Info("StreamDataReceived");
    }

    public override void OnEvent(LogEvent evnt)
    {
        SkyLog.Info("LogEvent " + evnt.msg);
    }
}
