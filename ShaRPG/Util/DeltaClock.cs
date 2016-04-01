using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;

namespace ShaRPG.Util {
    class DeltaClock
    {
        private readonly Clock _clock;

        public DeltaClock()
        {
            _clock = new Clock();
        }

        /// <summary>
        /// Calculate the delta time since the last call
        /// </summary>
        /// <returns>Elapsed time since the last call to this method</returns>
        public float GetDelta()
        {
            return _clock.Restart().AsSeconds();
        }
    }
}
