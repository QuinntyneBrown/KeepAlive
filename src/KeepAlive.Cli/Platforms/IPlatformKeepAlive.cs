/// <summary>
/// Operating-system specific operations used by <see cref="KeepAliveWorker"/>.
/// </summary>
internal interface IPlatformKeepAlive : IDisposable
{
    /// <summary>Human-readable platform name for logging.</summary>
    string Name { get; }

    /// <summary>Tell the OS not to idle-sleep the system or display.</summary>
    void PreventSleep();

    /// <summary>Restore normal power behaviour.</summary>
    void AllowSleep();

    /// <summary>Reset the OS idle timer so apps such as Teams see the user as active.</summary>
    void SimulateActivity();
}
