
namespace FuzzUtils
{
    public interface IFuzzTask
    {
        /// <summary>
        /// The identifier of the task.  This is linked to a help page on the github site
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Display name of the fuzzing item
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Whether or not the task is actively fuzzing.  This is used for diagnostic purposes
        /// </summary>
        bool IsActive { get; }
    }
}
