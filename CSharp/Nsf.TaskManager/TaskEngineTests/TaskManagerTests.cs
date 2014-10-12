using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using Nsf.TaskManager.TaskEngine;
using NUnit.Framework;

namespace Nsf.TaskManager.TaskEngineTests
{
    /// <summary>
    /// Tests for <see cref="TaskManager"/>.
    /// </summary>
    [TestFixture]
    public class TaskManagerTests
    {
        /// <summary>
        /// Tests method <see cref="TaskManager.AddTask"/>.
        /// </summary>
        [Test]
        public void AddTaskTest()
        {
            var taskStorage = new Mock<ITaskStorage>();
            var taskExecutor = new Mock<ITaskExecutor>();
            var taskManager = new TaskEngine.TaskManager(new List<ITaskExecutor> {taskExecutor.Object}, taskStorage.Object);
            var task = new Mock<ITask>();
            var result = taskManager.AddTask(task.Object, TaskPriority.High);
            Assert.IsTrue(result);
            taskStorage.Verify(obj => obj.AddTask(It.Is<ITask>(taskParam => taskParam == task.Object), TaskPriority.High), Times.Once());
        }

        /// <summary>
        /// Tests method <see cref="AddTaskTest"/> in case of task manager was stopped.
        /// </summary>
        [Test]
        public void AddTaskToStopedTest()
        {
            var taskStorage = new Mock<ITaskStorage>();
            var taskExecutor = new Mock<ITaskExecutor>();
            taskExecutor.Setup(obj => obj.Stop()).Raises(obj => obj.Stoped += null, EventArgs.Empty);
            var taskManager = new TaskEngine.TaskManager(new List<ITaskExecutor> { taskExecutor.Object }, taskStorage.Object);
            var task = new Mock<ITask>();
            taskManager.Stop();
            var result = taskManager.AddTask(task.Object, TaskPriority.High);
            Assert.IsFalse(result);
            taskStorage.Verify(obj => obj.AddTask(It.IsAny<ITask>(), It.IsAny<TaskPriority>()), Times.Never());
        }

        /// <summary>
        /// Tests that manager starts and stops executor correctly.
        /// </summary>
        [Test]
        public void ExecutorStartStopTest()
        {
            var taskStorage = new Mock<ITaskStorage>();
            var taskExecutor = new Mock<ITaskExecutor>();
            taskExecutor.Setup(obj => obj.Stop()).Raises(obj => obj.Stoped += null, EventArgs.Empty);
            var taskManager = new TaskEngine.TaskManager(new List<ITaskExecutor> { taskExecutor.Object }, taskStorage.Object);
            taskManager.Stop();
            taskExecutor.Verify(obj => obj.Start(It.Is<ITaskProducer>(producer => producer == taskStorage.Object)), Times.Once());
            taskExecutor.Verify(obj => obj.Stop(), Times.Once());
        }

        /// <summary>
        /// Tests correct notification about exception during task execution.
        /// </summary>
        [Test]
        public void ExceptionOccuredEventTest()
        {
            var exceptionOcuredEventIsOK = false;
            var exception = new Exception();
            var task = new Mock<ITask>();
            task.Setup(obj => obj.Execute()).Throws(exception);
            var taskProducer = new GeneralTaskStorage();
            var executor = new ThreadBasedTaskExecutor();
            var taskManager = new TaskEngine.TaskManager(new List<ITaskExecutor> {executor}, taskProducer);
            taskManager.ExceptionOccured += (sender, e) => { exceptionOcuredEventIsOK = sender == executor && e.Exception == exception && e.Task == task.Object; };
            taskManager.AddTask(task.Object, TaskPriority.High);
            Thread.Sleep(50);
            taskManager.Stop();
            Assert.IsTrue(exceptionOcuredEventIsOK);
        }


        /// <summary>
        /// Tests validation of the parameters.
        /// </summary>
        [Test]
        public void ValidationTest()
        {
            var taskStorage = new Mock<ITaskStorage>();
            var taskExecutor = new Mock<ITaskExecutor>();
            taskExecutor.Setup(obj => obj.Stop()).Raises(obj => obj.Stoped += null, EventArgs.Empty);
            var taskExecutors = new List<ITaskExecutor>() {taskExecutor.Object};
            Assert.Throws<ArgumentNullException>(() => new TaskEngine.TaskManager(null, taskStorage.Object));
            Assert.Throws<ArgumentNullException>(() => new TaskEngine.TaskManager(taskExecutors, null));
            using(var taskManager = new TaskEngine.TaskManager(taskExecutors, taskStorage.Object))
                Assert.Throws<ArgumentNullException>(() => taskManager.AddTask(null, TaskPriority.High));
        }
    }
}
