using ShaRPG.Util.Coordinate;

namespace ShaRPG.Util {
    public enum ClickPriority {
        Gui = 0,
        Entity = 50,
        Map = 100,
    }
    
    public interface IClickObserver {
        bool IsMouseOver(ScreenCoordinate coordinates);
        void Clicked(ScreenCoordinate coordinates);
    }
}
