
namespace FuzzUtils
{
    public interface IFuzzTask
    {
        /// <summary>
        /// Name of the fuzzing item
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Whether or not the task is actively fuzzing.  This is used for diagnostic purposes
        /// </summary>
        bool IsActive { get; }
    }
}
