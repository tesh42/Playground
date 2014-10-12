using Moq;
using Nsf.TaskManager.TaskEngine;
using NUnit.Framework;

namespace Nsf.TaskManager.TaskEngineTests
{
    /// <summary>
    /// Tests for <see cref="GeneralTaskStorage"/>.
    /// </summary>
    [TestFixture]
    public class GeneralTaskStorageTests
    {
        /// <summary>
        /// Tests working with all task priorities.
        /// </summary>
        [Test]
        public void AllPriorities()
        {
            var highTask = new Mock<ITask>();
            var normalTask = new Mock<ITask>();
            var lowTask = new Mock<ITask>();

            var storage = new GeneralTaskStorage();
            storage.AddTask(normalTask.Object, TaskPriority.Normal);
            storage.AddTask(highTask.Object, TaskPriority.High);
            storage.AddTask(highTask.Object, TaskPriority.High);
            storage.AddTask(lowTask.Object, TaskPriority.Low);
            storage.AddTask(highTask.Object, TaskPriority.High);
            storage.AddTask(highTask.Object, TaskPriority.High);
            storage.AddTask(normalTask.Object, TaskPriority.Normal);

            Assert.AreSame(storage.GetTask(), highTask.Object);
            Assert.AreSame(storage.GetTask(), highTask.Object);
            Assert.AreSame(storage.GetTask(), highTask.Object);
            Assert.AreSame(storage.GetTask(), normalTask.Object);
            Assert.AreSame(storage.GetTask(), highTask.Object);
            Assert.AreSame(storage.GetTask(), normalTask.Object);
            Assert.AreSame(storage.GetTask(), lowTask.Object);
            Assert.IsNull(storage.GetTask());
        }

        /// <summary>
        /// Tests working with High and Low task priorities.
        /// </summary>
        [Test]
        public void HighAndLowPriorities()
        {
            var highTask = new Mock<ITask>();
            var lowTask = new Mock<ITask>();

            var storage = new GeneralTaskStorage();
            storage.AddTask(highTask.Object, TaskPriority.High);
            storage.AddTask(highTask.Object, TaskPriority.High);
            storage.AddTask(lowTask.Object, TaskPriority.Low);
            storage.AddTask(highTask.Object, TaskPriority.High);
            storage.AddTask(highTask.Object, TaskPriority.High);

            Assert.AreSame(storage.GetTask(), highTask.Object);
            Assert.AreSame(storage.GetTask(), highTask.Object);
            Assert.AreSame(storage.GetTask(), highTask.Object);

            Assert.AreSame(storage.GetTask(), highTask.Object);
            Assert.AreSame(storage.GetTask(), lowTask.Object);
            Assert.IsNull(storage.GetTask());
        }

        /// <summary>
        /// Tests working with Normal and Low priorities.
        /// </summary>
        [Test]
        public void NormalAndLowPriorities()
        {
            var normalTask = new Mock<ITask>();
            var lowTask = new Mock<ITask>();

            var storage = new GeneralTaskStorage();
            storage.AddTask(lowTask.Object, TaskPriority.Low);
            storage.AddTask(normalTask.Object, TaskPriority.Normal);
            storage.AddTask(normalTask.Object, TaskPriority.Normal);

            Assert.AreSame(storage.GetTask(), normalTask.Object);
            Assert.AreSame(storage.GetTask(), normalTask.Object);
            Assert.AreSame(storage.GetTask(), lowTask.Object);
            Assert.IsNull(storage.GetTask());
        }
    }
}
