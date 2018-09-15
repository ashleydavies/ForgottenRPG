using System;

namespace ScriptCompiler {
    public class CompileException : Exception {
        internal CompileException(string message, int line, int position)
            : base($"{message} (at line {line}, position {position})") { }
    }
}
