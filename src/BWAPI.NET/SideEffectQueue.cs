using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BWAPI.NET
{
    /// <summary>
    /// Queue of intended bot interactions with the game, to be flushed as JBWAPI returns control to StarCraft after a frame.
    /// </summary>
    public class SideEffectQueue
    {
        private List<SideEffect> _queue = new List<SideEffect>();

        /// <summary>
        /// Includes a side effect to be sent back to BWAPI in the future.
        /// </summary>
        /// <param name="sideEffect">
        /// A side effect to be applied to the game state the next time the queue is flushed.</param>
        public void Enqueue(SideEffect sideEffect)
        {
            lock (this)
            {
                _queue.Add(sideEffect);
            }
        }

        /// <summary>
        /// Applies all enqueued side effects to the current BWAPI frame.
        /// </summary>
        /// <param name="liveGameData">
        /// The live game frame's data, using the BWAPI shared memory.</param>
        public void FlushTo(ClientData.TGameData liveGameData)
        {
            lock (this)
            {
                _queue.ForEach((x) => x.Apply(liveGameData));
                _queue.Clear();
            }
        }
    }
}