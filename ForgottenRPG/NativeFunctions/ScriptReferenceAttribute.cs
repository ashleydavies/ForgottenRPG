using System;

namespace ForgottenRPG.NativeFunctions {
    /// <summary>
    /// Marks a class as a reference type that can be used in scripts.
    /// The script will see this as a pointer type (e.g. Entity*).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class ScriptReferenceAttribute : Attribute {
        public string ScriptName { get; }

        public ScriptReferenceAttribute(string scriptName) {
            ScriptName = scriptName;
        }
    }
}
