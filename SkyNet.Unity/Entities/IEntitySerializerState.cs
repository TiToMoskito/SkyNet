namespace SkyNet
{
    internal interface IEntitySerializer<TState> : IEntitySerializer where TState : IState
    {
        TState state { get; }
    }
}
