namespace SkyNet
{
    interface IEntityBehaviour
    {
        bool invoke { get; }

        SkyEntity entity { get; set; }

        void Initialized();

        void Attached();

        void Detached();

        void SimulateOwner();

        void SimulateController();

        void SimulateRemote();

        void ControlLost();

        void ControlGained();
    }
}
