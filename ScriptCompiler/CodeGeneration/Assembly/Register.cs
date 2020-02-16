namespace ScriptCompiler.CodeGeneration.Assembly {
    public class Register : Location {
        public static Register InstructionPointer = new Register(0);
        public static Register StackPointer = new Register(1);

        private readonly int _number;

        public Register(int number) {
            _number = number;
        }

        public override string ToString() {
            return $"r{_number}";
        }
    }
}
