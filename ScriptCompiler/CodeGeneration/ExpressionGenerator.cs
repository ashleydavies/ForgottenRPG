using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.AST.Statements.Expressions.Arithmetic;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.CodeGeneration.Assembly.Instructions;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Types;
using ScriptCompiler.Visitors;
using Register = ScriptCompiler.CodeGeneration.Assembly.Register;

namespace ScriptCompiler.CodeGeneration {
    // The semantics of the ExpressionGenerator is to write the result of the expression onto the stack, and
    // bump the stack pointer accordingly.
    public class ExpressionGenerator : Visitor<List<Instruction>> {
        private readonly FunctionTypeRepository _functionTypeRepository;
        private readonly UserTypeRepository _userTypeRepository;
        private readonly Dictionary<string, StringLabel> _stringPoolAliases;
        private readonly RegisterManager _regManager;

        private StackFrame _stackFrame;

        private Register StackPointer => _regManager.StackPointer;

        private TypeIdentifier TypeIdentifier => new TypeIdentifier(
            _functionTypeRepository, _userTypeRepository, _stackFrame);

        private AddressabilityChecker AddressabilityChecker => new AddressabilityChecker(
            _functionTypeRepository, _userTypeRepository, _stackFrame);

        private LValueIdentifier LValueIdentifier => new LValueIdentifier(
            _functionTypeRepository, _userTypeRepository, _stackFrame, _regManager);

        public ExpressionGenerator(FunctionTypeRepository functionTypeRepository, UserTypeRepository userTypeRepository,
                                   Dictionary<string, StringLabel> stringPoolAliases, StackFrame stackFrame,
                                   RegisterManager regManager) {
            _functionTypeRepository = functionTypeRepository;
            _userTypeRepository     = userTypeRepository;
            _stringPoolAliases      = stringPoolAliases;
            _stackFrame             = stackFrame;
            _regManager             = regManager;
        }

        public List<Instruction> Generate(ExpressionNode node) {
            return Visit(node as dynamic);
        }

        public override List<Instruction> Visit(ASTNode node) {
            throw new System.NotImplementedException(node.GetType().FullName);
        }

        public List<Instruction> Visit(VariableAccessNode node) {
            var instructions = new List<Instruction>();
            (SType type, int offset) = _stackFrame.Lookup(node.Identifier);

            // Copy the variable value onto the stack
            using var readLocation = _regManager.NewRegister();
            instructions.Add(
                new MovInstruction(readLocation, StackPointer).WithComment($"Begin access {node.Identifier}"));
            instructions.Add(new AddInstruction(readLocation, offset));
            for (int i = 0; i < type.Length; i++) {
                // Use that register to copy across the object
                instructions.Add(new MemCopyInstruction(StackPointer, readLocation));
                instructions.Add(new AddInstruction(readLocation, 1));
                instructions.Add(PushStack(SType.SInteger));
            }

            return instructions;
        }

        public List<Instruction> Visit(FunctionCallNode node) {
            var instructions = new List<Instruction>();

            // Push space for the return value
            instructions.Add(PushStack(_functionTypeRepository.ReturnType(node.FunctionName)));

            // Push the parameters
            foreach (var expressionNode in node.Args) {
                // Our contract with Generate is they will push the result onto the stack... exactly what we want!
                instructions.AddRange(Generate(expressionNode));
            }

            // Push the current instruction pointer
            var returnLabel = new Label($"return_{Guid.NewGuid()}");
            // Push the return address to the stack
            instructions.Add(new MemWriteInstruction(_regManager.StackPointer, returnLabel));
            // TODO: Check this logic? Make sure all stack operations make sense
            instructions.Add(new AddInstruction(_regManager.StackPointer, 1));
            instructions.Add(new JmpInstruction(new Label($"func_{node.FunctionName}")));
            // The return address
            instructions.Add(new LabelInstruction(returnLabel));
            // Pop the arguments
            foreach (var expressionNode in node.Args) {
                // Our contract with Generate is they will push the result onto the stack... exactly what we want!
                instructions.Add(PopStack(TypeIdentifier.Identify(expressionNode)));
            }

            // Conveniently, our contract is to put the result on the top of the stack... Where it already is :)
            return instructions;
        }

        public List<Instruction> Visit(IntegerLiteralNode node) {
            return new List<Instruction> {
                new MemWriteInstruction(StackPointer, node.Value),
                PushStack(SType.SInteger)
            };
        }

        public List<Instruction> Visit(StringLiteralNode node) {
            if (!_stringPoolAliases.ContainsKey(node.String)) {
                // TODO: Exception upgrade
                throw new ArgumentException();
            }

            return new List<Instruction> {
                new MemWriteInstruction(StackPointer, _stringPoolAliases[node.String]),
                PushStack(SType.SInteger)
            };
        }

        public List<Instruction> Visit(AssignmentNode node) {
            var nodeType = TypeIdentifier.Identify(node);
            var (instructions, pointerReg) = LValueIdentifier.Generate(node.Destination);
            instructions[0].WithComment("Begin assignment");
            using (pointerReg) {
                instructions.AddRange(Generate(node.Value));
                instructions.Add(new SubInstruction(StackPointer, nodeType.Length));
                for (int i = 0; i < nodeType.Length; i++) {
                    instructions.Add(new MemCopyInstruction(pointerReg, StackPointer));
                    instructions.Add(new AddInstruction(pointerReg, 1));
                    instructions.Add(new AddInstruction(StackPointer, 1));
                }
            }

            return instructions;
        }

        public List<Instruction> Visit(StructAccessNode node) {
            var instructions = Generate(node.Left);
            var structType   = TypeIdentifier.Identify(node.Left);

            if (!(structType is UserType userStructType)) {
                throw new CompileException($"Unable to access field {node.Field} of non-struct type {structType}", 0,
                                           0);
            }

            // The entire struct is now on the stack
            instructions.Add(PopStack(userStructType));

            using var copyRegister = _regManager.NewRegister();

            instructions.Add(new MovInstruction(copyRegister, StackPointer));
            instructions.Add(new AddInstruction(copyRegister, userStructType.OffsetOfField(node.Field)));

            var typeOfField = userStructType.TypeOfField(node.Field);
            for (int i = 0; i < typeOfField.Length; i++) {
                instructions.Add(new MemCopyInstruction(StackPointer, copyRegister));
                instructions.Add(new AddInstruction(StackPointer, 1));
                instructions.Add(new AddInstruction(copyRegister, 1));
            }
            
            _stackFrame.Pushed(typeOfField);
            return instructions;
        }

        public List<Instruction> Visit(AddressOfNode node) {
            if (!AddressabilityChecker.Check(node.Expression)) {
                throw new CompileException("Attempt to take the address of an unaddressable expression", 0, 0);
            }

            // TODO: Implement
            return new List<Instruction>();
        }

        public List<Instruction> Visit(DereferenceNode node) {
            // TODO: Implement
            return new List<Instruction>();
        }

        public List<Instruction> Visit(AdditionNode node) {
            var instructions = new List<Instruction>();
            var (opInstructions, left, right) = GenerateSingleWordBinOpSetup(node);
            instructions.AddRange(opInstructions);
            instructions.Add(new AddInstruction(left, right));
            instructions.Add(new MemWriteInstruction(StackPointer, left));
            instructions.Add(PushStack(SType.SInteger));
            left.Dispose();
            right.Dispose();
            return instructions;
        }

        public List<Instruction> Visit(SubtractionNode node) {
            var instructions = new List<Instruction>();
            var (opInstructions, left, right) = GenerateSingleWordBinOpSetup(node);
            instructions.AddRange(opInstructions);
            instructions.Add(new SubInstruction(left, right));
            instructions.Add(new MemWriteInstruction(StackPointer, left));
            instructions.Add(PushStack(SType.SInteger));
            left.Dispose();
            right.Dispose();
            return instructions;
        }

        public List<Instruction> Visit(MultiplicationNode node) {
            var instructions = new List<Instruction>();
            var (opInstructions, left, right) = GenerateSingleWordBinOpSetup(node);
            instructions.AddRange(opInstructions);
            instructions.Add(new MulInstruction(left, right));
            instructions.Add(new MemWriteInstruction(StackPointer, left));
            instructions.Add(PushStack(SType.SInteger));
            left.Dispose();
            right.Dispose();
            return instructions;
        }

        public List<Instruction> Visit(DivisionNode node) {
            var instructions = new List<Instruction>();
            var (opInstructions, left, right) = GenerateSingleWordBinOpSetup(node);
            instructions.AddRange(opInstructions);
            instructions.Add(new DivInstruction(left, right));
            instructions.Add(new MemWriteInstruction(StackPointer, left));
            instructions.Add(PushStack(SType.SInteger));
            left.Dispose();
            right.Dispose();
            return instructions;
        }

        /// <summary>
        /// Generates the setup for a binary operator for a single word operation
        /// </summary>
        public (List<Instruction>, Register, Register) GenerateSingleWordBinOpSetup(BinaryOperatorNode node) {
            List<Instruction> instructions  = new List<Instruction>();
            var               leftRegister  = _regManager.NewRegister();
            var               rightRegister = _regManager.NewRegister();
            instructions.AddRange(Generate(node.Left));
            instructions.Add(PopStack(SType.SInteger));
            instructions.Add(new MemReadInstruction(leftRegister, StackPointer));
            instructions.AddRange(Generate(node.Right));
            instructions.Add(PopStack(SType.SInteger));
            instructions.Add(new MemReadInstruction(rightRegister, StackPointer));
            return (instructions, leftRegister, rightRegister);
        }

        private Instruction PushStack(SType type) {
            _stackFrame.Pushed(type);
            return new AddInstruction(StackPointer, type.Length);
        }

        private Instruction PopStack(SType type) {
            _stackFrame.Popped(type);
            return new SubInstruction(StackPointer, type.Length);
        }
    }
}
