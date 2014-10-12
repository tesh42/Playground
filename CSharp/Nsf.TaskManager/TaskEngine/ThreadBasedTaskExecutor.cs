using System;
using System.Threading;

namespace Nsf.TaskManager.TaskEngine
{
    /// <summary>
    /// Provides implementation of the thread-based task executor.
    /// </summary>
    public class ThreadBasedTaskExecutor : ITaskExecutor
    {
        /// <summary>
        /// Event that will be set to signaled state when termination of the working thread be requested.
        /// </summary>
        readonly AutoResetEvent stopEvent = new AutoResetEvent(false);

        /// <summary>
        /// Task producer.
        /// </summary>
        ITaskProducer taskProducer;

        /// <summary>
        /// Thread that executes tasks.
        /// </summary>
        Thread workingThread;

        public event EventHandler<EventArgs> Stoped;

        public event EventHandler<ExceptionOccuredEventArgs> ExceptionOccured;

        public void Start(ITaskProducer taskProducer)
        {
            this.taskProducer = taskProducer;
            workingThread = new Thread(TaskProcessingCallback);
            workingThread.Start();
        }

        public void Stop()
        {
            stopEvent.Set();
        }

        /// <summary>
        /// Raises the <see cref="Stoped"/> event.
        /// </summary>
        void OnStoped()
        {
            var handler = Stoped;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ExceptionOccured" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ExceptionOccuredEventArgs"/> instance containing the event data.</param>
        void OnExceptionOccured(ExceptionOccuredEventArgs e)
        {
            var handler = ExceptionOccured;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="task">Task that should be executed.</param>
        void ExecuteTask(ITask task)
        {
            try
            {
                task.Execute();
            }
            catch(Exception ex)
            {
                OnExceptionOccured(new ExceptionOccuredEventArgs(task, ex));
            }
        }

        /// <summary>
        /// Callback of <see cref="workingThread"/> that processes tasks.
        /// </summary>
        /// <param name="state">State that was passed to thread.</param>
        void TaskProcessingCallback(object state)
        {
            while (true)
            {
                var task = taskProducer.GetTask();
                if (task != null)
                    ExecuteTask(task);
                else if (stopEvent.WaitOne(1))
                {
                    OnStoped();
                    return;
                }
            }
        }
    }
}
