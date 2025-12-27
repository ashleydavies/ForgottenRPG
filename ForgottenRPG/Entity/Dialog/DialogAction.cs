using System;
using System.Collections.Generic;
using ForgottenRPG.VM;
using ForgottenRPG.Service;

namespace ForgottenRPG.Entity.Dialog {
    public abstract class DialogAction {
        public abstract void Execute(Dialog dialog);
    }

    public class DialogActionChangeNode : DialogAction {
        private readonly int _id;

        public DialogActionChangeNode(int id) {
            _id = id;
        }

        public override void Execute(Dialog dialog) {
            dialog.ChangeNode(_id);
        }
    }

    public class DialogActionEndDiscussion : DialogAction {
        public override void Execute(Dialog dialog) {
            dialog.EndDialog();
        }
    }

    public class DialogActionCode : DialogAction {
        private readonly List<uint> _code;

        public DialogActionCode(List<uint> code) {
            _code = code;
        }

        public override void Execute(Dialog dialog) {
            var stdlibBytes = ServiceLocator.StdlibBytes;
            if (stdlibBytes == null) {
                throw new InvalidOperationException("Stdlib bytecode is not loaded.");
            }
            ServiceLocator.LogService.Log(LogType.Info, $"DialogActionCode stdlib words: {stdlibBytes.Count}");
            if (stdlibBytes.Count >= 5) {
                ServiceLocator.LogService.Log(LogType.Info,
                    $"DialogActionCode stdlib exportOff: {stdlibBytes[3]} relocOff: {stdlibBytes[2]}");
            }
            ServiceLocator.LogService.Log(LogType.Info, $"DialogActionCode script words: {_code.Count}");
            var vm = new ScriptVm(stdlibBytes, new List<uint>(_code));
            
            // Register native functions
            var registry = new ForgottenRPG.NativeFunctions.NativeFunctionRegistry();
            registry.ScanType(typeof(ForgottenRPG.Scripting.GameScriptLibrary));
            vm.RegisterNativeFunctions(registry);
            
            vm.Execute();
        }
    }

    public class DialogActionScript : DialogAction {
        private readonly string _scriptId;

        public DialogActionScript(string scriptId) {
            _scriptId = scriptId;
        }

        public override void Execute(Dialog dialog) {
            Scripting.ScriptRuntime.Default.ExecuteScriptId(_scriptId);
        }
    }
}
