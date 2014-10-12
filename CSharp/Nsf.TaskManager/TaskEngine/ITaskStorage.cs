namespace Nsf.TaskManager.TaskEngine
{
    /// <summary>
    /// Defines storage of the tasks.
    /// </summary>
    public interface ITaskStorage : ITaskProducer
    {
        /// <summary>
        /// Adds task to the storage.
        /// </summary>
        /// <param name="task">Task that should be added to the storage.</param>
        /// <param name="priority">Task priority.</param>
        /// <remarks>This method is threadsafe.</remarks>
        void AddTask(ITask task, TaskPriority priority);
    }
}
