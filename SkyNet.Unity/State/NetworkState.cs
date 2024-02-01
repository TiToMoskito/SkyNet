using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SkyNet.Generated")]
namespace SkyNet
{
    public abstract class NetworkState : IEntitySerializer, IState, IDisposable
    {
        internal Entity entity;
        internal NetworkState_Meta Meta;

        internal Dictionary<string, object> m_debug = new Dictionary<string, object>();

        internal PacketFlags flag;
        internal Targets targets;
        private NetBuffer packer;

        internal NetworkState(NetworkState_Meta meta) { Meta = meta; packer = new NetBuffer(); }

        void IEntitySerializer.DebugInfo()
        {
            SkyManager.DebugDrawer.LabelBold("");
            SkyManager.DebugDrawer.LabelBold("State Info");
            SkyManager.DebugDrawer.LabelField("Type", Factory.GetFactory(Meta.TypeId).TypeObject);
            SkyManager.DebugDrawer.LabelField("Type Id", Meta.TypeId);
            SkyManager.DebugDrawer.LabelBold("");
            SkyManager.DebugDrawer.LabelBold("State Properties");

            foreach (var item in m_debug) { SkyManager.DebugDrawer.LabelField(item.Key, item.Value); }
        }

        TypeId IEntitySerializer.ID { get { return Meta.TypeId; } }

        void IEntitySerializer.OnCreated(Entity _entity)
        {
            entity = _entity;
            OnCreated(entity);
        }

        void IEntitySerializer.OnSynchronize()
        {
            packer.Flush();
            Pack(packer);

            byte contentSize = packer.ReadByte();
            byte[] content = packer.ReadBytes(contentSize);

            entity.Source.SendState(content, entity.NetworkId, flag, targets);
        }

        void IEntitySerializer.OnParentChanging(Entity newParent, Entity oldParent) { }

        public void Dispose() { }
        public virtual void OnCreated(Entity _entity) { }
        public virtual void Unpack(NetBuffer _packer) { }
        public virtual void Pack(NetBuffer _packer) { }
        public virtual void OnNotOwner() { }
    }
}
