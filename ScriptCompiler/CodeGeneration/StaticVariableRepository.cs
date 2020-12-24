using System;
using System.Collections.Generic;

namespace ScriptCompiler.CodeGeneration {
    internal class StaticVariableRepository {
        public readonly Dictionary<Guid, uint[]> Values = new Dictionary<Guid, uint[]>();
        
        public Guid CreateNew(uint[] initialValue) {
            var guid = Guid.NewGuid();
            Values[guid] = initialValue;
            return guid;
        }
    }
}
