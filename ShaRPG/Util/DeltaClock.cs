#region

using SFML.System;

#endregion

namespace ShaRPG.Util {
    internal class DeltaClock {
        private readonly Clock _clock;

        public DeltaClock() {
            _clock = new Clock();
        }

        /// <summary>
        ///     Calculate the delta time since the last call
        /// </summary>
        /// <returns>Elapsed time since the last call to this method</returns>
        public float GetDelta() => _clock.Restart().AsSeconds();
    }
}
