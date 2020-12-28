using ScriptCompiler.Types;

namespace ScriptCompiler.CodeGeneration.Assembly.Instructions {
    public abstract class BinaryArithmeticInstruction : Instruction {
        public readonly Value Value;
        protected readonly Location ToLocation;
        private readonly SType _operandType;

        public BinaryArithmeticInstruction(Location toLocation, Value value, SType? operandType) {
            operandType  ??= SType.SInteger;
            _operandType =   operandType;
            ToLocation   =   toLocation;
            Value        =   value;
        }

        public bool SameLocation(BinaryArithmeticInstruction other) {
            return ToLocation == other.ToLocation;
        }

        public abstract bool IsNoop();

        // TODO Polymorphism
        public bool TryCombine(Instruction other, out BinaryArithmeticInstruction? result) {
            if (!(other is BinaryArithmeticInstruction otherArithmetic && SameLocation(otherArithmetic) &&
                  Value is NumericConstant ncLeft &&
                  otherArithmetic.Value is NumericConstant ncRight)) {
                result = null;
                return false;
            }

            if (this is AddInstruction) {
                switch (other) {
                    case AddInstruction _:
                        result = new AddInstruction(ToLocation, ncLeft + ncRight, _operandType);
                        return true;
                    case SubInstruction _:
                        var subResult = ncLeft - ncRight;
                        if (subResult >= 0) {
                            result = new AddInstruction(ToLocation, subResult, _operandType);
                        } else {
                            result = new SubInstruction(ToLocation, -subResult, _operandType);
                        }

                        return true;
                }
            }

            if (this is SubInstruction) {
                switch (other) {
                    case AddInstruction _:
                        var addResult = ncLeft - ncRight;
                        if (addResult >= 0) {
                            result = new SubInstruction(ToLocation, addResult, _operandType);
                        } else {
                            result = new AddInstruction(ToLocation, -addResult, _operandType);
                        }

                        return true;
                    case SubInstruction _:
                        result = new SubInstruction(ToLocation, ncLeft + ncRight, _operandType);
                        return true;
                }
            }

            // TODO: Optimisations for mul and div
            result = null;
            return false;
        }

        protected string InstructionSuffix() {
            if (ReferenceEquals(_operandType, SType.SFloat)) {
                return "F";
            }

            return "";
        }
    }
}
