using System;

namespace ForgottenRPG.NativeFunctions {
    /// <summary>
    /// Marks a static method as a native function that can be called from scripts.
    /// The method must have signature: void MethodName(ScriptNativeContext ctx)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NativeFunctionAttribute : Attribute {
        /// <summary>
        /// The name of the function as it appears in scripts
        /// </summary>
        public string ScriptName { get; }

        public NativeFunctionAttribute(string scriptName) {
            ScriptName = scriptName;
        }
    }
}
