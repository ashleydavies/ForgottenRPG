using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using ScriptCompiler.Types;

namespace ScriptCompiler.CompileUtil {
    public class StackFrame {
        // Sometimes we want to push inaccessible variables to the stack, such as a return address during preparation
        //  for entering a function. These affect the access to stack variables (they are all now further down the
        //  stack) and therefore we need to keep track of how much we've "nudged" these things onto the stack
        public int Length { get; private set; }
        private readonly StackFrame _parent;
        private readonly Dictionary<string, (SType, int)> _variableTable;

        public StackFrame(StackFrame parent) : this() {
            _parent = parent;
        }

        public StackFrame() {
            _variableTable = new Dictionary<string, (SType, int)>();
        }

        public bool ExistsLocalScope(string identifier) {
            return _variableTable.ContainsKey(identifier);
        }
        
        /// <summary>
        /// Gets the offset of a given identifier from the top of the stack, along with the type, providing it exists.
        /// Otherwise, returns a NoType type, with undefined behaviour for the offset in this case.
        /// </summary>
        public (SType type, int position) Lookup(string identifier) {
            if (ExistsLocalScope(identifier)) {
                var (type, pos) = _variableTable[identifier];
                return (type, -Length + pos);
            }
            
            if (_parent != null) {
                var (type, pos) = _parent.Lookup(identifier);
                return (type, pos - Length);
            }

            return (SType.SNoType, 0);
        }

        /// <summary>
        /// Adds an identifier with a given type to the stack, and adjusts the stack length (and all consequent
        /// accesses)
        /// </summary>
        public void AddIdentifier(SType type, string identifier) {
            _variableTable[identifier] = (type, Length);
            Length += type.Length;
        }

        /// <summary>
        /// Given some data, allows future accesses to stack variables to account for changes on the stack
        /// outside of the initial stack frame.
        /// </summary>
        public void Pushed(SType data) {
            Length += data.Length;
        }

        /// <summary>
        /// Undoes the changes made by Pushed for a given type
        /// </summary>
        public void Popped(SType data) {
            Length -= data.Length;
        }
    }
}
