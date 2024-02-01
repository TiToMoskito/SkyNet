using UnityEngine;

namespace SkyNet
{
    public abstract class EntityEventListenerBase : EntityBehaviour
    {
        public override sealed void Initialized()
        {
            entity.Entity.AddEventListener(this);
        }
    }
}
