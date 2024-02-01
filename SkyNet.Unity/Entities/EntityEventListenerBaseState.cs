namespace SkyNet
{
    public abstract class EntityEventListenerBase<TState> : EntityEventListenerBase
    {
        public TState state
        {
            get
            {
                return entity.GetState<TState>();
            }
        }
    }
}
