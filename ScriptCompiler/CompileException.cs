using System;

namespace ScriptCompiler {
    class CompileException : Exception {
        public CompileException(string message, int line, int position)
            : base($"{message} (at line {line}, position {position})") { }
    }
}
