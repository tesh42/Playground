using System.Collections.Concurrent;

namespace Nsf.TaskManager.TaskEngine
{
    /// <summary>
    /// Provides implementation of the general task storage.
    /// </summary>
    /// <remarks>
    /// Rules for task extraction:
    /// * Each task that has <see cref="TaskPriority.Normal"/> priority is processed after three tasks with <see cref="TaskPriority.High"/> being processed.
    /// * Tasks that have <see cref="TaskPriority.Low"/> priority are processed when there are no tasks with higher priority.
    /// </remarks>
    public class GeneralTaskStorage : ITaskStorage
    {
        /// <summary>
        /// Number of the tasks with <see cref="TaskPriority.High"/> priority, which should be processed before one task with <see cref="TaskPriority.Normal"/> priority.
        /// </summary>
        const byte HighPriorityTaskCountOnOneNormal = 3;

        /// <summary>
        /// Queue of the tasks with the <see cref="TaskPriority.High"/> priority.
        /// </summary>
        readonly ConcurrentQueue<ITask> highPriorityTasks = new ConcurrentQueue<ITask>();

        /// <summary>
        /// Queue of the tasks with the <see cref="TaskPriority.Normal"/> priority.
        /// </summary>
        readonly ConcurrentQueue<ITask> normalPriorityTasks = new ConcurrentQueue<ITask>();

        /// <summary>
        /// Queue of the tasks with the <see cref="TaskPriority.Low"/> priority.
        /// </summary>
        readonly ConcurrentQueue<ITask> lowPriorityTasks = new ConcurrentQueue<ITask>();

        /// <summary>
        /// Number of the processed tasks that had <see cref="TaskPriority.High"/> priority.
        /// </summary>
        byte highPriorityProcessedTaskCounter;

        /// <summary>
        /// Object that is used for synchronization of the task extraction.
        /// </summary>
        readonly object getTaskSync = new object();

        public void AddTask(ITask task, TaskPriority priority)
        {
            ConcurrentQueue<ITask> queue;
            switch (priority)
            {
                case TaskPriority.High:
                    queue = highPriorityTasks;
                    break;
                case TaskPriority.Normal:
                    queue = normalPriorityTasks;
                    break;
                default:
                    queue = lowPriorityTasks;
                    break;
            }
            queue.Enqueue(task);
        }

        public ITask GetTask()
        {
            lock (getTaskSync)
            {
                ITask task;
                if (highPriorityProcessedTaskCounter < HighPriorityTaskCountOnOneNormal && highPriorityTasks.TryDequeue(out task))
                {
                    highPriorityProcessedTaskCounter += 1;
                    return task;
                }
                if (normalPriorityTasks.TryDequeue(out task))
                {
                    highPriorityProcessedTaskCounter = 0;
                    return task;
                }
                if (highPriorityProcessedTaskCounter >= HighPriorityTaskCountOnOneNormal && highPriorityTasks.TryDequeue(out task))
                    return task;
                lowPriorityTasks.TryDequeue(out task);
                return task;
            }
        }
    }
}
