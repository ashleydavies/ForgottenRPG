using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ForgottenRPG.NativeFunctions;
using ForgottenRPG.Service;
using ForgottenRPG.VM;

namespace ForgottenRPG.Scripting {
    public sealed class ScriptRuntime {
        private static readonly Lazy<ScriptRuntime> DefaultInstance =
            new Lazy<ScriptRuntime>(() => new ScriptRuntime());

        private readonly Dictionary<string, List<uint>> _scriptCache = new();
        private NativeFunctionRegistry _nativeRegistry;

        public static ScriptRuntime Default => DefaultInstance.Value;

        public string ScriptsRoot { get; set; } =
            Path.Combine(AppContext.BaseDirectory, "Resources", "Data", "Scripts");

        public string CompiledRoot => Path.Combine(ScriptsRoot, "Compiled");

        public string StdlibScriptId { get; set; } = "stdlib/stdlib";

        public void ExecuteScriptId(string scriptId) {
            var stdlibBytes = EnsureStdlibLoaded();
            var code = LoadScriptBytes(scriptId);
            ServiceLocator.LogService.Log(LogType.Info, $"ScriptRuntime stdlib words: {stdlibBytes.Count}");
            if (stdlibBytes.Count >= 5) {
                ServiceLocator.LogService.Log(LogType.Info,
                    $"ScriptRuntime stdlib exportOff: {stdlibBytes[3]} relocOff: {stdlibBytes[2]}");
            }
            ServiceLocator.LogService.Log(LogType.Info, $"ScriptRuntime script words: {code.Count}");

            var vm = new ScriptVm(stdlibBytes, new List<uint>(code));
            vm.RegisterNativeFunctions(GetNativeRegistry());
            vm.Execute();
        }

        private List<uint> EnsureStdlibLoaded() {
            if (ServiceLocator.StdlibBytes != null) {
                return ServiceLocator.StdlibBytes;
            }

            var stdlibBytes = LoadScriptBytes(StdlibScriptId);
            ServiceLocator.StdlibBytes = stdlibBytes;
            return stdlibBytes;
        }

        private NativeFunctionRegistry GetNativeRegistry() {
            if (_nativeRegistry != null) return _nativeRegistry;

            var registry = new NativeFunctionRegistry();
            registry.ScanType(typeof(GameScriptLibrary));
            _nativeRegistry = registry;
            return _nativeRegistry;
        }

        private List<uint> LoadScriptBytes(string scriptId) {
            if (_scriptCache.TryGetValue(scriptId, out var cached)) {
                return cached;
            }

            string path = Path.Combine(CompiledRoot, scriptId + ".fscrbyte");
            if (!File.Exists(path)) {
                throw new InvalidOperationException($"Missing compiled script: {path}");
            }

            string contents = File.ReadAllText(path).Trim();
            if (contents.Length == 0) {
                throw new InvalidOperationException($"Compiled script is empty: {path}");
            }

            var bytes = contents.Split(',')
                                .Select(x => x.Trim())
                                .Where(x => x.Length > 0)
                                .Select(uint.Parse)
                                .ToList();
            _scriptCache[scriptId] = bytes;
            return bytes;
        }
    }
}
