#region

using ShaRPG.Util;

#endregion

namespace ShaRPG.Service.Null {
    public class NullImageStoreService : IImageStoreService {
        public Image GetImage(int id) {
            ServiceLocator.LogService.Log(LogType.NullObject, $"Attempt to get image {id} from null store service");
            return null;
        }
    }
}
