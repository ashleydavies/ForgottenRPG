using System;
using System.Collections.Generic;

namespace ScriptCompiler.CodeGeneration {
    internal class StaticVariableRepository {
        public readonly Dictionary<Guid, int[]> Values = new Dictionary<Guid, int[]>();
        
        public Guid CreateNew(int[] initialValue) {
            var guid = Guid.NewGuid();
            Values[guid] = initialValue;
            return guid;
        }
    }
}
