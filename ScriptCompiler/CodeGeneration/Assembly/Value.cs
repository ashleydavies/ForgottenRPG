using System.Net.Http.Headers;

namespace ScriptCompiler.CodeGeneration.Assembly {
    public abstract class Value {
        public abstract override string ToString();
        
        public static implicit operator Value(int v) {
            return new NumericConstant(v);
        }
    }
}
