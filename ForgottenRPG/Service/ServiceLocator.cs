using ForgottenRPG.Service.Null;

namespace ForgottenRPG.Service {
    public static class ServiceLocator {
        private static ILogService _logService;
        private static IAudioService _audioService;
        private static IScriptStoreService _scriptStoreService;
        private static readonly ILogService NullLogService = new NullLogService();
        private static readonly IAudioService NullAudioService = new NullAudioService();
        private static readonly IScriptStoreService NullScriptStoreService = new NullScriptStoreService();

        public static ILogService LogService {
            get => _logService ?? NullLogService;
            set => _logService = value;
        }

        public static IAudioService AudioService {
            get => _audioService ?? NullAudioService;
            set => _audioService = value;
        }

        public static IScriptStoreService ScriptStoreService {
            get => _scriptStoreService ?? NullScriptStoreService;
            set => _scriptStoreService = value;
        }
    }
}
