using System;

namespace Nsf.TaskManager.TaskEngine
{
    /// <summary>
    /// Contains information about the error that occured during task execution.
    /// </summary>
    public class ExceptionOccuredEventArgs : EventArgs
    {
        /// <summary>
        /// Information about exception.
        /// </summary>
        public Exception Exception { get; private set;}

        /// <summary>
        /// Information about failed task.
        /// </summary>
        public ITask Task { get; private set;}

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionOccuredEventArgs"/> class.
        /// </summary>
        /// <param name="task">Information about failed task.</param>
        /// <param name="exception">Information about exception.</param>
        public ExceptionOccuredEventArgs(ITask task, Exception exception)
        {
            Task = task;
            Exception = exception;
        }
    }
}