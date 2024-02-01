[System.Flags]
public enum LogTargets
{
    None = 0,
    Unity = 1 << 0,
    File = 1 << 1,
    SystemOut = 1 << 2,
    All = ~0,
}