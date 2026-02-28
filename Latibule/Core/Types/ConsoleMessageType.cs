namespace Latibule.Core.Types;

/// <summary>
/// The type of message that is being logged.
/// </summary>
public enum ConsoleMessageType
{
    /// <summary>
    /// Used for general information.
    /// </summary>
    Info,

    /// <summary>
    /// Used for warnings.
    /// </summary>
    Warning,

    /// <summary>
    /// Used for errors.
    /// </summary>
    Error,

    /// <summary>
    /// Used for debug messages.
    /// </summary>
    Debug,

    /// <summary>
    /// Used inside of commands to log output in the console.
    /// </summary>
    CommandOutput
}