using System;
using ScriptCompiler.Visitors;

namespace ScriptCompiler.CodeGeneration.Assembly {
    public class Register : Location, IDisposable {
        public readonly int Number;

        private readonly RegisterManager? _registerManager;

        public Register(int number, RegisterManager registerManager) : this(number) {
            _registerManager = registerManager;
        }

        private Register(int number) {
            Number = number;
        }

        public override string ToString() {
            return $"r{Number}";
        }

        public void Dispose() {
            _registerManager?.ClearRegister(this);
        }
    }
}
