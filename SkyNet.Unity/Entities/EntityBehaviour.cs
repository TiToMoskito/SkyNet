using UnityEngine;

namespace SkyNet
{
    public abstract class EntityBehaviour : MonoBehaviour, IEntityBehaviour
    {
        internal SkyEntity _entity;

        public SkyEntity entity
        {
            get
            {
                if (_entity == null)
                    _entity = transform.GetComponent<SkyEntity>();

                if (_entity == null)
                    _entity = transform.GetComponentInParent<SkyEntity>();

                if (_entity == null)
                    SkyLog.Error("Could not find a SkyEntity component attached to '{0}' or any of its parents", gameObject.name);

                return _entity;
            }
            set
            {
                _entity = value;
            }
        }

        bool IEntityBehaviour.invoke { get{ return enabled; } }
        public virtual void Initialized() { }
        public virtual void Attached() { }
        public virtual void Detached() { }
        public virtual void SimulateOwner() { }
        public virtual void SimulateController() { }
        public virtual void SimulateRemote() { }
        public virtual void ControlGained() { }
        public virtual void ControlLost() { }
    }
}
