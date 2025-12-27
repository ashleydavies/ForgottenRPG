using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ForgottenRPG.NativeFunctions {
    public static class SymbolHasher {
        private const uint FnvOffsetBasis = 2166136261;
        private const uint FnvPrime = 16777619;

        public static uint HashFunction(string scriptName, MethodInfo method,
                                        IReadOnlyDictionary<string, Type> registeredTypes) {
            string signature = BuildSignature(scriptName, method, registeredTypes);
            return Hash(signature);
        }

        public static string BuildSignature(string scriptName, MethodInfo method,
                                             IReadOnlyDictionary<string, Type> registeredTypes) {
            var parameters = method.GetParameters()
                                   .Where(p => p.ParameterType != typeof(ScriptNativeContext))
                                   .Select(p => CanonicalTypeName(p.ParameterType, registeredTypes));
            string paramList = string.Join(",", parameters);
            string returnName = CanonicalTypeName(method.ReturnType, registeredTypes);
            return $"{scriptName}({paramList})->{returnName}";
        }

        public static string CanonicalTypeName(Type type, IReadOnlyDictionary<string, Type> registeredTypes) {
            if (type == typeof(void)) return "void";
            if (type == typeof(int)) return "int";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(float)) return "float";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(char)) return "char";
            if (type == typeof(string)) return "@char";

            if (type.IsArray) {
                return "@" + CanonicalTypeName(type.GetElementType()!, registeredTypes);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
                return "@" + CanonicalTypeName(type.GetGenericArguments()[0], registeredTypes);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ScriptValuePtr<>)) {
                string? valueTypeName = TryGetScriptValueTypeName(type.GetGenericArguments()[0]);
                if (!string.IsNullOrWhiteSpace(valueTypeName)) {
                    return "@" + valueTypeName;
                }
            }

            string? valueTypeScriptName = TryGetScriptValueTypeName(type);
            if (!string.IsNullOrWhiteSpace(valueTypeScriptName)) {
                return valueTypeScriptName;
            }

            string? scriptName = registeredTypes.FirstOrDefault(kvp => kvp.Value == type ||
                                                                       kvp.Value.IsAssignableFrom(type)).Key;
            if (!string.IsNullOrWhiteSpace(scriptName)) {
                return "@" + scriptName;
            }

            return "@int";
        }

        private static string? TryGetScriptValueTypeName(Type type) {
            var attr = type.GetCustomAttribute<ScriptValueTypeAttribute>();
            return attr?.ScriptName;
        }

        private static uint Hash(string text) {
            uint hash = FnvOffsetBasis;
            foreach (char c in text) {
                hash ^= c;
                hash *= FnvPrime;
            }

            return hash;
        }
    }
}
