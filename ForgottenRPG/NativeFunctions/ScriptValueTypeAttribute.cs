using System;

namespace ForgottenRPG.NativeFunctions {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ScriptValueTypeAttribute : Attribute {
        public string ScriptName { get; }

        public ScriptValueTypeAttribute(string scriptName) {
            ScriptName = scriptName;
        }
    }
}
