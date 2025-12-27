using System;
using System.Collections.Generic;
using static System.BitConverter;

namespace ForgottenRPG.NativeFunctions {
    /// <summary>
    /// Context object passed to native functions, providing access to the VM's stack
    /// and entity registry for reading arguments and returning values.
    /// </summary>
    public class ScriptNativeContext {
        private readonly Stack<uint> _stack;
        private readonly Dictionary<int, object> _entityRegistry;
        private readonly Stack<object> _pendingData;
        private readonly Action<uint, uint> _memoryWriter;
        private readonly Func<uint, uint> _memoryReader;

        public ScriptNativeContext(Stack<uint> stack, Dictionary<int, object> entityRegistry, 
                                 Stack<object> pendingData = null,
                                 Action<uint, uint> memoryWriter = null,
                                 Func<uint, uint> memoryReader = null) {
            _stack = stack;
            _entityRegistry = entityRegistry;
            _pendingData = pendingData;
            _memoryWriter = memoryWriter;
            _memoryReader = memoryReader;
        }

        public Stack<object> PendingData => _pendingData;

        public void WriteMemory(uint address, uint value) {
            if (_memoryWriter == null) throw new InvalidOperationException("Memory writing not supported in this context");
            _memoryWriter(address, value);
        }

        public uint ReadMemory(uint address) {
            if (_memoryReader == null) throw new InvalidOperationException("Memory reading not supported in this context");
            return _memoryReader(address);
        }

        public string ReadString(uint address) {
            uint length = ReadMemory(address);
            char[] chars = new char[length];
            for (uint i = 0; i < length; i++) {
                chars[i] = Convert.ToChar(ReadMemory(address + 1 + i));
            }

            return new string(chars);
        }

        public void WriteValue(uint address, object value) {
            ScriptValueTypeMarshaller.WriteValue(this, address, value);
        }

        // Stack operations for arguments and return values
        public int PopInt() => (int)_stack.Pop();
        public uint PopUInt() => _stack.Pop();
        public float PopFloat() => Int32BitsToSingle((int)_stack.Pop());
        
        public int PeekInt() => (int)_stack.Peek();
        public uint PeekUInt() => _stack.Peek();
        public float PeekFloat() => Int32BitsToSingle((int)_stack.Peek());
        
        public void PushInt(int value) => _stack.Push((uint)value);
        public void PushUInt(uint value) => _stack.Push(value);
        public void PushFloat(float value) => _stack.Push((uint)SingleToInt32Bits(value));

        // Object registry operations
        public object GetObject(int id) {
            if (_entityRegistry.TryGetValue(id, out var obj)) {
                return obj;
            }
            throw new InvalidOperationException($"Object with ID {id} not found in registry");
        }

        public T GetObject<T>(int id) {
            var obj = GetObject(id);
            if (obj is T typedObj) {
                return typedObj;
            }
            throw new InvalidOperationException($"Object with ID {id} is not of type {typeof(T).Name}");
        }

        public bool TryGetObject(int id, out object obj) {
            return _entityRegistry.TryGetValue(id, out obj);
        }

        public int RegisterObject(object obj) {
            // Find next available ID
            int id = _entityRegistry.Count + 1;
            while (_entityRegistry.ContainsKey(id)) {
                id++;
            }
            _entityRegistry[id] = obj;
            return id;
        }

        public bool HasObject(int id) {
            return _entityRegistry.ContainsKey(id);
        }
    }
}
