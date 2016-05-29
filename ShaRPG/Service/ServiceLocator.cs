#region

using ShaRPG.Service.Null;

#endregion

namespace ShaRPG.Service {
    public static class ServiceLocator {
        private static ILogService _logService;
        private static IAudioService _audioService;
        private static IImageStoreService _imageStoreService;
        private static IScriptStoreService _scriptStoreService;
        private static readonly ILogService NullLogService = new NullLogService();
        private static readonly IAudioService NullAudioService = new NullAudioService();
        private static readonly IImageStoreService NullImageStoreService = new NullImageStoreService();
        private static readonly IScriptStoreService NullScriptStoreService = new NullScriptStoreService();

        public static ILogService LogService {
            get { return _logService ?? NullLogService; }
            set { _logService = value; }
        }

        public static IAudioService AudioService {
            get { return _audioService ?? NullAudioService; }
            set { _audioService = value; }
        }

        public static IImageStoreService ImageStoreService {
            get { return _imageStoreService ?? NullImageStoreService; }
            set { _imageStoreService = value; }
        }

        public static IScriptStoreService ScriptStoreService {
            get { return _scriptStoreService ?? NullScriptStoreService; }
            set { _scriptStoreService = value; }
        }
    }
}
