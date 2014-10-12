namespace Nsf.TaskManager.TaskEngine
{
    /// <summary>
    /// Defines tasks producer.
    /// </summary>
    public interface ITaskProducer
    {
        /// <summary>
        /// Requests the task.
        /// </summary>
        /// <returns>Task that was found; otherwise, <c>null</c>.</returns>
        /// <remarks>This method is threadsafe.</remarks>
        ITask GetTask();
    }
}
