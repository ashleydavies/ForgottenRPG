namespace ScriptCompiler.Types {
    public sealed record FunctionReference(SType? Type, string Identifier);
}

namespace System.Runtime.CompilerServices {
    public class IsExternalInit{}
}