using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ForgottenRPG.NativeFunctions {
    /// <summary>
    /// Registry for native functions that can be called from scripts.
    /// Discovers functions via reflection and automatically marshals arguments/return values.
    /// </summary>
    public class NativeFunctionRegistry {
        private readonly Dictionary<string, NativeFunctionInfo> _functionsByName = new Dictionary<string, NativeFunctionInfo>();
        private readonly Dictionary<int, NativeFunctionInfo> _functionsById = new Dictionary<int, NativeFunctionInfo>();
        private int _nextId = 0;

        public class NativeFunctionInfo {
            public int Id { get; set; }
            public string ScriptName { get; set; }
            public MethodInfo Method { get; set; }
            public Action<ScriptNativeContext> MarshalledDelegate { get; set; }
            public uint SymbolHash { get; set; }
        }

        /// <summary>
        /// Scan a type for static methods marked with [NativeFunction] and register them
        /// </summary>
        private readonly Dictionary<string, Type> _registeredTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Scan a type for static methods marked with [NativeFunction] and register them.
        /// Also scans for [ScriptReference] attribute on the type itself.
        /// </summary>
        public void ScanType(Type type) {
            // Check for ScriptReference attribute
            var typeAttr = type.GetCustomAttribute<ScriptReferenceAttribute>();
            if (typeAttr != null) {
                _registeredTypes[typeAttr.ScriptName] = type;
            }

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                              .OrderBy(m => m.Name)
                              .ToArray();
            
            foreach (var method in methods) {
                var attr = method.GetCustomAttribute<NativeFunctionAttribute>();
                if (attr == null) continue;

                // Create a marshalling wrapper that handles stack operations automatically
                var marshalledDelegate = CreateMarshallingWrapper(method);
                RegisterFunction(attr.ScriptName, method, marshalledDelegate);
            }
        }

        /// <summary>
        /// Get all registered reference types (Script Name -> C# Type)
        /// </summary>
        public IReadOnlyDictionary<string, Type> GetRegisteredTypes() {
            return _registeredTypes;
        }

        /// <summary>
        /// Create a wrapper that automatically marshals arguments from stack and return value to stack
        /// </summary>
        private Action<ScriptNativeContext> CreateMarshallingWrapper(MethodInfo method) {
            var parameters = method.GetParameters();
            var returnType = method.ReturnType;

            return ctx => {
                // Pop arguments from stack in reverse order (last parameter is on top)
                object[] args = new object[parameters.Length];
                for (int i = parameters.Length - 1; i >= 0; i--) {
                    var paramType = parameters[i].ParameterType;
                    if (paramType == typeof(ScriptNativeContext)) {
                        args[i] = ctx;
                    } else {
                        args[i] = PopValueFromStack(ctx, paramType);
                    }
                }

                // Invoke the method
                object result = method.Invoke(null, args);

                // Push return value to stack if not void
                if (returnType == typeof(void)) return;

                if (IsReferenceType(returnType)) {
                    // Generalized dynamic marshalling protocol:
                    // 1. Cache the object
                    ctx.PendingData.Push(result);
                    
                    // 2. Calculate size in words: 1 (length) + count * element_size
                    uint size = 1;
                    if (result is string s) {
                        size += (uint)s.Length;
                    } else if (result is System.Collections.ICollection collection) {
                        // For now we assume elements are 1 word (int, handle, etc.)
                        // This matches the compiler's current behavior for List<T> and T[]
                        size += (uint)collection.Count;
                    }
                    
                    // 3. Push size to stack (malloc will use this)
                    ctx.PushUInt(size);
                } else {
                    PushValueToStack(ctx, result, returnType);
                }
            };
        }

        private bool IsReferenceType(Type type) {
            if (type == typeof(string)) return true;
            if (type.IsArray) return true;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return true;
            return false;
        }

        /// <summary>
        /// Pop a value of the given type from the stack
        /// </summary>
        private object PopValueFromStack(ScriptNativeContext ctx, Type type) {
            if (type == typeof(int)) {
                return ctx.PopInt();
            } else if (type == typeof(uint)) {
                return ctx.PopUInt();
            } else if (type == typeof(float)) {
                return ctx.PopFloat();
            } else if (type == typeof(bool)) {
                return ctx.PopInt() != 0;
            } else if (type == typeof(string)) {
                uint address = ctx.PopUInt();
                return ctx.ReadString(address);
            } else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ScriptValuePtr<>)) {
                int address = ctx.PopInt();
                return Activator.CreateInstance(type, address);
            } else {
                // For reference types, assume it's an object handle (int) that needs lookup
                int id = ctx.PopInt();
                if (id == 0) return null;
                
                // We need to return the actual object, not the handle
                object obj = ctx.GetObject(id);
                
                if (obj != null && !type.IsInstanceOfType(obj)) {
                    throw new InvalidCastException($"Native function parameter type mismatch: Expected {type.Name}, got {obj.GetType().Name} (ID: {id})");
                }
                
                return obj;
            }
        }

        /// <summary>
        /// Push a value of the given type to the stack
        /// </summary>
        private void PushValueToStack(ScriptNativeContext ctx, object value, Type type) {
            if (type == typeof(int)) {
                ctx.PushInt((int)value);
            } else if (type == typeof(uint)) {
                ctx.PushUInt((uint)value);
            } else if (type == typeof(float)) {
                ctx.PushFloat((float)value);
            } else if (type == typeof(bool)) {
                ctx.PushInt((bool)value ? 1 : 0);
            } else {
                // For reference types, register the object and push its ID
                int id = ctx.RegisterObject(value);
                ctx.PushInt(id);
            }
        }

        /// <summary>
        /// Register a native function with a given script name
        /// </summary>
        private void RegisterFunction(string scriptName, MethodInfo method, Action<ScriptNativeContext> marshalledDelegate) {
            if (_functionsByName.ContainsKey(scriptName)) {
                throw new InvalidOperationException($"Function '{scriptName}' is already registered");
            }

            int id = _nextId++;
            uint symbolHash = SymbolHasher.HashFunction(scriptName, method, _registeredTypes);
            var info = new NativeFunctionInfo {
                Id = id,
                ScriptName = scriptName,
                Method = method,
                MarshalledDelegate = marshalledDelegate,
                SymbolHash = symbolHash
            };

            _functionsByName[scriptName] = info;
            _functionsById[id] = info;
        }

        /// <summary>
        /// Get a function by its script name
        /// </summary>
        public NativeFunctionInfo GetByName(string scriptName) {
            if (_functionsByName.TryGetValue(scriptName, out var info)) {
                return info;
            }
            return null;
        }

        public uint? GetSymbolHash(string scriptName) {
            var info = GetByName(scriptName);
            return info?.SymbolHash;
        }

        /// <summary>
        /// Get a function by its ID
        /// </summary>
        public NativeFunctionInfo GetById(int id) {
            if (_functionsById.TryGetValue(id, out var info)) {
                return info;
            }
            return null;
        }

        /// <summary>
        /// Check if a function is registered
        /// </summary>
        public bool HasFunction(string scriptName) {
            return _functionsByName.ContainsKey(scriptName);
        }

        /// <summary>
        /// Get all registered functions
        /// </summary>
        public IEnumerable<NativeFunctionInfo> AllFunctions => _functionsById.Values;
    }
}
