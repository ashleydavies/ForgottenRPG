using System;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using ScriptCompiler.CodeGeneration;
using StackFrame = ScriptCompiler.CompileUtil.StackFrame;

namespace ScriptCompiler.AST.Statements.Expressions {
    // A context for evaluating a constexpr within
    public readonly struct CalcContext {
        // If set, the evaluation context is static (and can therefore be initialised based on other static variables)
        public readonly bool Static;
        private readonly StackFrame _stackFrame;
        private readonly StaticVariableRepository _staticRepository;

        public CalcContext(bool @static, StackFrame stackFrame, StaticVariableRepository staticRepository) {
            Static           = @static;
            _stackFrame       = stackFrame;
            _staticRepository = staticRepository;
        }

        public uint[]? EvaluateVariable(string identifier) {
            return _stackFrame.Lookup(identifier) is (_, null, { } guid) ? _staticRepository.Values[guid] : null;
        }
    }
}
