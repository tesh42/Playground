using System;
using System.Threading;
using Moq;
using Nsf.TaskManager.TaskEngine;
using NUnit.Framework;

namespace Nsf.TaskManager.TaskEngineTests
{
    /// <summary>
    /// Tests for <see cref="ThreadBasedTaskExecutor"/>.
    /// </summary>
    [TestFixture]
    public class ThreadBasedTaskExecutorTests
    {
        /// <summary>
        /// Tests raising of the <see cref="ThreadBasedTaskExecutor.Stoped"/> event.
        /// </summary>
        [Test]
        public void StopedEventTest()
        {
            var stopEventIsOK = false;
            var taskProducer = new Mock<ITaskStorage>();
            var executor = new ThreadBasedTaskExecutor();

            executor.Stoped += (sender, args) => { stopEventIsOK = sender == executor && args == EventArgs.Empty; };
            
            executor.Start(taskProducer.Object);
            Thread.Sleep(50);
            executor.Stop();
            Thread.Sleep(50);

            Assert.IsTrue(stopEventIsOK);
        }

        /// <summary>
        /// Tests task retrieval.
        /// </summary>
        [Test]
        public void GetingTaskTest()
        {
            var taskProducer = new Mock<ITaskStorage>();
            var executor = new ThreadBasedTaskExecutor();

            executor.Start(taskProducer.Object);
            Thread.Sleep(50);
            executor.Stop();

            taskProducer.Verify(obj => obj.GetTask(), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests task execution.
        /// </summary>
        [Test]
        public void ExecuteTaskTest()
        {
            var tasksReturned = 0;
            var task = new Mock<ITask>();
            var taskProducer = new Mock<ITaskStorage>();
            taskProducer.Setup(obj => obj.GetTask()).Returns(() => ++tasksReturned == 1 ? task.Object : null);

            var executor = new ThreadBasedTaskExecutor();

            executor.Start(taskProducer.Object);
            Thread.Sleep(50);
            executor.Stop();

            task.Verify(obj => obj.Execute(), Times.Once());
        }

        /// <summary>
        /// Tests raising of the <see cref="ThreadBasedTaskExecutor.ExceptionOccured"/> event.
        /// </summary>
        [Test]
        public void ExceptionOccuredEventTest()
        {
            var tasksReturned = 0;
            var exceptionOcuredEventIsOK = false;
            var exception = new Exception();

            var task = new Mock<ITask>();
            task.Setup(obj => obj.Execute()).Throws(exception);

            var taskProducer = new Mock<ITaskStorage>();
            taskProducer.Setup(obj => obj.GetTask()).Returns(() => ++tasksReturned == 1 ? task.Object : null);

            var executor = new ThreadBasedTaskExecutor();
            executor.ExceptionOccured += (sender, e) => { exceptionOcuredEventIsOK = sender == executor && e.Exception == exception && e.Task == task.Object; };

            executor.Start(taskProducer.Object);
            Thread.Sleep(50);
            executor.Stop();
            
            Assert.IsTrue(exceptionOcuredEventIsOK);
        }
    }
}
