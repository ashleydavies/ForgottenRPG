using System;
using System.Collections.Generic;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Util {
    public class ClickManager {
        private readonly Dictionary<ClickPriority, List<IClickObserver>> _observers
            = new Dictionary<ClickPriority, List<IClickObserver>>();

        public ClickManager() {
            foreach (ClickPriority clickPriority in Enum.GetValues(typeof(ClickPriority))) {
                _observers[clickPriority] = new List<IClickObserver>();
            }
        }

        public void Register(ClickPriority priority, IClickObserver observer) {
            _observers[priority].Add(observer);
        }

        public void Clicked(ScreenCoordinate coordinate) {
            foreach (ClickPriority clickPriority in Enum.GetValues(typeof(ClickPriority))) {
                foreach (IClickObserver observer in _observers[clickPriority]) {
                    if (observer.IsMouseOver(coordinate)) {
                        observer.Clicked(coordinate);
                        return;
                    }
                }
            }
        }
    }
}
