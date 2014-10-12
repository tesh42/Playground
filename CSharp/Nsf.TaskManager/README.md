Nsf.TaskManager
=========

This project contains implementation of the task manager that could execute tasks, which have high, normal and low priorities.

* Each task that has Normal priority is executed after three tasks with High priority being processed.
* Tasks that have Low priority are executed when there are no tasks with higher priority.

When stop is requested, the task manager waits for completion of the tasks that was added to the processing queue.

##How to use
Creates task manager that is using 4 task executors and general task storage:

            var taskManager = new TaskManager(4);
            taskManager.AddTask(task, TaskPriority.High);

Creates task manager that is using desired task executors and task storage. Task manager is stopped when tasks processing finished.

            using (var taskManager = new TaskManager(taskExecutors, taskStorage))
            {
                taskManager.AddTask(task1, TaskPriority.High);
                taskManager.AddTask(task2, TaskPriority.High);
            }
