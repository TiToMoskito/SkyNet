namespace SkyNet
{
    public abstract class EntityBehaviour<TState> : EntityBehaviour
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
