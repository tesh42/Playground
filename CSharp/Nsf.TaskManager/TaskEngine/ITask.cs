namespace Nsf.TaskManager.TaskEngine
{
    /// <summary>
    /// Defines a task, which is unit of executable code.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// Executes this task.
        /// </summary>
        void Execute();
    }
}
