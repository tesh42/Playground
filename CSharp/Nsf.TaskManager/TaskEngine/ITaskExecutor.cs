using System;

namespace Nsf.TaskManager.TaskEngine
{
    /// <summary>
    /// Defines a task executor.
    /// </summary>
    public interface ITaskExecutor
    {
        /// <summary>
        /// Occurs when task execution is stopped.
        /// </summary>
        event EventHandler<EventArgs> Stoped;

        /// <summary>
        /// Occurs when exception occured during task execution.
        /// </summary>
        event EventHandler<ExceptionOccuredEventArgs> ExceptionOccured;

        /// <summary>
        /// Starts processing of tasks.
        /// </summary>
        /// <param name="taskProducer">Task producer.</param>
        void Start(ITaskProducer taskProducer);

        /// <summary>
        /// Stops processing of tasks.
        /// </summary>
        void Stop();
    }
}
