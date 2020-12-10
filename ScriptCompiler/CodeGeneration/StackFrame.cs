using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCompiler.Types;

namespace ScriptCompiler.CompileUtil {
    public class StackFrame {
        // Used to track the portion of the stack frame reserved for the return value for a function to populate.
        // Accessed by the code generator for the `return` statement.
        public const string ReturnIdentifier = "!RETURN";

        // Sometimes we want to push inaccessible variables to the stack, such as a return address during preparation
        //  for entering a function. These affect the access to stack variables (they are all now further down the
        //  stack) and therefore we need to keep track of how much we've "nudged" these things onto the stack
        public int Length { get; private set; }
        private readonly StackFrame? _parent;

        private readonly Dictionary<string, (SType type, int position)> _variableTable
            = new Dictionary<string, (SType, int)>();

        private readonly Dictionary<string, (SType type, Guid guid)> _staticTable
            = new Dictionary<string, (SType, Guid)>();

        public StackFrame() { }

        public StackFrame(StackFrame parent) {
            _parent = parent;
        }

        public (StackFrame? parent, int length) Purge() {
            var expectedLength = _variableTable.Values.Sum(v => v.type.Length);

            if (Length != expectedLength) {
                throw new CompileException(
                    $"Unexpected call to StackFrame::Purge() - not all locals popped? {Length - expectedLength} words left",
                    0, 0);
            }

            return (_parent, expectedLength);
        }

        public bool ExistsLocalAny(string identifier) {
            return _variableTable.ContainsKey(identifier) || _staticTable.ContainsKey(identifier);
        }

        public bool ExistsLocalScope(string identifier) {
            return _variableTable.ContainsKey(identifier);
        }

        public bool ExistsLocalStatic(string identifier) {
            return _staticTable.ContainsKey(identifier);
        }

        /// <summary>
        /// Gets the location of a given identifier from the top of the stack, along with the type, providing it exists.
        /// Otherwise, returns a NoType type, with undefined behaviour for the offset in this case.
        /// If the type exists, it returns either the stack offset (for locals) or Guid (for statics), but not both.
        /// </summary>
        public (SType type, int? position, Guid? guid) Lookup(string identifier) {
            if (ExistsLocalScope(identifier)) {
                var (type, pos) = _variableTable[identifier];
                return (type, -Length + pos, null);
            } else if (ExistsLocalStatic(identifier)) {
                var (type, guid) = _staticTable[identifier];
                return (type, null, guid);
            }

            if (_parent != null) {
                var (type, pos, guid) = _parent.Lookup(identifier);
                if (pos != null) {
                    return (type, pos - Length, null);
                } else {
                    return (type, null, guid);
                }
            }

            return (SType.SNoType, null, null);
        }

        /// <summary>
        /// Adds an identifier with a given type to the stack, and adjusts the stack length (and all consequent
        /// accesses)
        /// </summary>
        public void AddIdentifier(SType type, string identifier) {
            _variableTable[identifier] =  (type, Length);
            Length                     += type.Length;
        }

        /// <summary>
        /// Adds a static identifier with given type to the local scope
        /// </summary>
        public void AddStaticIdentifier(SType type, string identifier, Guid guid) {
            _staticTable[identifier] = (type, guid);
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
