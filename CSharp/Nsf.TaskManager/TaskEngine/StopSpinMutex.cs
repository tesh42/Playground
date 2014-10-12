using System.Threading;

namespace Nsf.TaskManager.TaskEngine
{
    /// <summary>
    /// Represents a lock that is used to manage operations that could be stopped.
    /// Any amount of the operations may enter the mutex when stop isn't requested. If stop is requested, none operation can enter the mutex.
    /// </summary>
    public class StopSpinMutex
    {
        /// <summary>
        /// State of the processing. 
        /// It's greater than 0 while operations are processing, equal to zero if there is no operations and equal to -1 after successful stop.
        /// </summary>
        int state;

        /// <summary>
        /// Indicates that stop was requested. 
        /// This flag was added to prevent the situation when new operations enter mutex quicker than <see cref="Stop"/> works.
        /// </summary>
        int stopRequested;

        /// <summary>
        /// Attempts to enter the mutex.
        /// Increases active operations counter by 1.
        /// </summary>
        /// <returns><c>true</c> if attempt was successful; overwise, <c>false</c>.</returns>
        public bool TryEnter()
        {
            while (true)
            {
                var oldState = Thread.VolatileRead(ref state);
                if (oldState < 0 || Thread.VolatileRead(ref stopRequested) == 1)
                    return false;
                if (Interlocked.CompareExchange(ref state, oldState + 1, oldState) == oldState)
                    return true;
            }
        }

        /// <summary>
        /// Leaves the mutex.
        /// Decreases active operations counter by 1.
        /// </summary>
        public void Leave()
        {
            while (true)
            {
                var oldState = Thread.VolatileRead(ref state);
                if (oldState < 0 || Interlocked.CompareExchange(ref state, oldState - 1, oldState) == oldState)
                    return;
            }
        }

        /// <summary>
        /// Stops processing of the operations.
        /// Prevent other operations from acquiring this mutex.
        /// </summary>
        /// <returns><c>true</c> if processing was stopped on current thread; overwise, <c>false</c>.</returns>
        public bool Stop()
        {
            Thread.VolatileWrite(ref stopRequested, 1);
            while (true)
            {
                var oldState = Interlocked.CompareExchange(ref state, -1, 0);
                if (oldState == -1)
                    return false;
                if (oldState == 0)
                    return true;
                Thread.SpinWait(1);
            }
        }
    }
}
