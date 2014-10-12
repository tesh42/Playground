using System;
using System.Collections.Generic;
using System.Threading;

namespace Nsf.TaskManager.TaskEngine
{
    /// <summary>
    /// Manages tasks execution.
    /// </summary>
    public class TaskManager : IDisposable
    {
        /// <summary>
        /// Mutex that is used for task manager termination.
        /// </summary>
        readonly StopSpinMutex stopMutex = new StopSpinMutex();

        /// <summary>
        /// Event that will switched to signaled state when workers stop.
        /// </summary>
        readonly AutoResetEvent stopedEvent = new AutoResetEvent(false);

        /// <summary>
        /// Task storage.
        /// </summary>
        ITaskStorage taskStorage;

        /// <summary>
        /// Workers that are used for processing of the tasks.
        /// </summary>
        List<ITaskExecutor> workers;

        /// <summary>
        /// Number or the active workers.
        /// </summary>
        int activeWorkersCount;

        /// <summary>
        /// Occurs when exception occured during task execution.
        /// </summary>
        public event EventHandler<ExceptionOccuredEventArgs> ExceptionOccured
        {
            add
            {
                foreach (var worker in workers)
                    worker.ExceptionOccured += value;
            }
            remove
            {
                foreach (var worker in workers)
                    worker.ExceptionOccured -= value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskManager"/> class.
        /// </summary>
        /// <param name="threadCount">Number of the threads that should be used for processing of the tasks.</param>
        /// <exception cref="System.ArgumentException">Argument should be greater than zero.;threadCount</exception>
        /// <remarks><see cref="ThreadBasedTaskExecutor"/> and <see cref="GeneralTaskStorage"/> are used by default.</remarks>
        public TaskManager(int threadCount)
        {
            if (threadCount <= 0)
                throw new ArgumentException("Argument should be greater than zero.", "threadCount");
            var workers = new List<ThreadBasedTaskExecutor>(threadCount);
            for (var i = 0; i < threadCount; ++i)
                workers.Add(new ThreadBasedTaskExecutor());
            Initialize(workers, new GeneralTaskStorage());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskManager"/> class.
        /// </summary>
        /// <param name="workers">Enumerable of the workers that should be used for processing of the tasks.</param>
        /// <param name="taskStorage">Desired task storage.</param>
        /// <exception cref="System.ArgumentNullException">One of the <paramref name="workers"/> or <paramref name="taskStorage"/> weren't set.</exception>
        public TaskManager(IEnumerable<ITaskExecutor> workers, ITaskStorage taskStorage)
        {
            if (workers == null)
                throw new ArgumentNullException("workers");
            if (taskStorage == null)
                throw new ArgumentNullException("taskStorage");
            Initialize(workers, taskStorage);
        }

        /// <summary>
        /// Adds task to the processing queue.
        /// </summary>
        /// <param name="task">Task that should be processed.</param>
        /// <param name="priority">Task priority.</param>
        /// <returns><c>true</c> if task was successfully added to the processing queue; overwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> wasn't set.</exception>
        /// <remarks>This method is threadsafe.</remarks>
        public bool AddTask(ITask task, TaskPriority priority)
        {
            if (task == null)
                throw new ArgumentNullException("task");
            if (!stopMutex.TryEnter())
                return false;
            try
            {
                taskStorage.AddTask(task, priority);
                return true;
            }
            finally 
            {
                stopMutex.Leave();
            }
        }

        /// <summary>
        /// Stops acception of the new tasks and waits until processing of the already added tasks will be finished.
        /// </summary>
        /// <remarks>Blocks thread until processing of all tasks will be finished.</remarks>
        public void Stop()
        {
            if (!stopMutex.Stop())
                return;
            foreach (var worker in workers)
                worker.Stop();
            stopedEvent.WaitOne();
        }

        /// <summary>
        /// Initializes the task manager.
        /// </summary>
        /// <param name="workers">Enumerable of the workers that should be used for processing of the tasks.</param>
        /// <param name="taskStorage">Desired task storage.</param>
        void Initialize(IEnumerable<ITaskExecutor> workers, ITaskStorage taskStorage)
        {
            this.taskStorage = taskStorage;
            this.workers = new List<ITaskExecutor>(workers);
            activeWorkersCount = this.workers.Count;
            foreach (var worker in this.workers)
            {
                worker.Stoped += OnTaskExecutorStoped;
                worker.Start(this.taskStorage);
            }
        }

        /// <summary>
        /// Processed an event when one of the workers stopped.
        /// </summary>
        void OnTaskExecutorStoped(object sender, EventArgs e)
        {
            var workerCount = Interlocked.Decrement(ref activeWorkersCount);
            if (workerCount == 0)
                stopedEvent.Set();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
