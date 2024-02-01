using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("SkyNet.Editor")]
namespace SkyNet
{
    internal interface IEntitySerializer
    {
        TypeId ID { get; }
		
        void OnCreated(Entity entity);

        void OnSynchronize();

        void DebugInfo();

        void OnParentChanging(Entity newParent, Entity oldParent);

        void Unpack(NetBuffer _packer);

        void Pack(NetBuffer _packer);

        void OnNotOwner();
    }
}
