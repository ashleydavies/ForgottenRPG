using System;

namespace ScriptCompiler.CompileUtil {
    public class Register : IDisposable {
        public readonly int Number;
        private readonly Action _dispose;
        
        public Register(int number, Action dispose) {
            Number = number;
            _dispose = dispose;
        }

        public override string ToString() {
            return $"r{Number}";
        }

        public void Dispose() {
            _dispose();
        }
    }
}
