namespace Latibule.Core.Types;

public interface ICommand
{
    /// <summary>
    /// The name of the command.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The aliases of the command.
    /// </summary>
    public List<string> Aliases { get; }
    /// <summary>
    /// The usage of the command.
    /// </summary>
    public string Usage { get; }
    /// <summary>
    /// The method that is called when the command is executed.
    /// </summary>
    public Task Execute(string[] args);
}