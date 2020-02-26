namespace ScriptCompiler.CodeGeneration.Assembly {
    public class NumericConstant : Value {
        public readonly int Amount;

        public NumericConstant(int amount) {
            Amount = amount;
        }

        public override string ToString() {
            return $"{Amount}";
        }
        
        public static implicit operator int(NumericConstant nc) {
            return nc.Amount;
        }
    }
}
