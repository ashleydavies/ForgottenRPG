using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ForgottenRPG.NativeFunctions {
    internal static class ScriptValueTypeMarshaller {
        private static readonly Dictionary<Type, ScriptValueTypeInfo> Cache = new();

        internal static ScriptValueTypeInfo GetInfo(Type type) {
            if (Cache.TryGetValue(type, out var info)) {
                return info;
            }

            var attr = type.GetCustomAttribute<ScriptValueTypeAttribute>();
            if (attr == null) {
                throw new InvalidOperationException($"Type {type.Name} is not a script value type.");
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                             .OrderBy(f => f.MetadataToken)
                             .ToList();
            if (fields.Count == 0) {
                throw new InvalidOperationException($"Type {type.Name} has no public fields to marshal.");
            }

            int wordSize = 0;
            foreach (var field in fields) {
                wordSize += GetWordSize(field.FieldType);
            }

            info = new ScriptValueTypeInfo(attr.ScriptName, fields, wordSize);
            Cache[type] = info;
            return info;
        }

        internal static int GetWordSize(Type type) {
            if (type == typeof(int) || type == typeof(uint) || type == typeof(float) || type == typeof(bool)) {
                return 1;
            }

            if (type.GetCustomAttribute<ScriptValueTypeAttribute>() != null) {
                return GetInfo(type).WordSize;
            }

            throw new InvalidOperationException($"Unsupported field type {type.Name} in script value type.");
        }

        internal static void WriteValue(ScriptNativeContext ctx, uint address, object value) {
            if (value == null) {
                throw new InvalidOperationException("Cannot marshal null value type.");
            }

            var info = GetInfo(value.GetType());
            uint addr = address;

            foreach (var field in info.Fields) {
                var fieldType = field.FieldType;
                var fieldValue = field.GetValue(value);

                if (fieldType == typeof(int)) {
                    ctx.WriteMemory(addr++, (uint)(int)fieldValue);
                } else if (fieldType == typeof(uint)) {
                    ctx.WriteMemory(addr++, (uint)fieldValue);
                } else if (fieldType == typeof(float)) {
                    ctx.WriteMemory(addr++, (uint)BitConverter.SingleToInt32Bits((float)fieldValue));
                } else if (fieldType == typeof(bool)) {
                    ctx.WriteMemory(addr++, (bool)fieldValue ? 1u : 0u);
                } else if (fieldType.GetCustomAttribute<ScriptValueTypeAttribute>() != null) {
                    WriteValue(ctx, addr, fieldValue);
                    addr += (uint)GetInfo(fieldType).WordSize;
                } else {
                    throw new InvalidOperationException(
                        $"Unsupported field type {fieldType.Name} in script value type {info.ScriptName}.");
                }
            }
        }

        internal sealed class ScriptValueTypeInfo {
            public string ScriptName { get; }
            public IReadOnlyList<FieldInfo> Fields { get; }
            public int WordSize { get; }

            public ScriptValueTypeInfo(string scriptName, IReadOnlyList<FieldInfo> fields, int wordSize) {
                ScriptName = scriptName;
                Fields = fields;
                WordSize = wordSize;
            }
        }
    }
}
