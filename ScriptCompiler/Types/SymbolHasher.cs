using System;
using System.Collections.Generic;

namespace ScriptCompiler.Types {
    public static class SymbolHasher {
        private const uint FnvOffsetBasis = 2166136261;
        private const uint FnvPrime = 16777619;

        public static uint HashFunction(FunctionReference reference, SType returnType, IReadOnlyList<SType> parameters) {
            string signature = BuildSignature(reference, returnType, parameters);
            return Hash(signature);
        }

        public static string BuildSignature(FunctionReference reference, SType returnType,
                                             IReadOnlyList<SType> parameters) {
            string owner = reference.Type != null ? CanonicalTypeName(reference.Type) : string.Empty;
            string name = reference.Identifier;
            string paramList = string.Join(",", ParametersToNames(parameters));
            string returnName = CanonicalTypeName(returnType);

            if (owner.Length == 0) {
                return $"{name}({paramList})->{returnName}";
            }

            return $"{owner}::{name}({paramList})->{returnName}";
        }

        public static string CanonicalTypeName(SType type) {
            if (type is ReferenceType referenceType) {
                return "@" + CanonicalTypeName(referenceType.ContainedType);
            }

            return type.Name;
        }

        private static IEnumerable<string> ParametersToNames(IReadOnlyList<SType> parameters) {
            foreach (var parameter in parameters) {
                yield return CanonicalTypeName(parameter);
            }
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
