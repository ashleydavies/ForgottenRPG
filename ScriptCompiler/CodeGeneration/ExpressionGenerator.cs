using System;
using System.Collections.Generic;
using ForgottenRPG.VM;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.AST.Statements.Expressions.Arithmetic;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.CodeGeneration.Assembly.Instructions;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Types;
using ScriptCompiler.Visitors;
using SFML.Window;
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
            throw new System.NotImplementedException();
        }

        public List<Instruction> Visit(VariableAccessNode node) {
            var instructions = new List<Instruction>();
            (SType type, int offset) = _stackFrame.Lookup(node.Identifier);

            // Copy the variable value onto the stack
            using var readLocation = _regManager.NewRegister();
            instructions.Add(new MovInstruction(readLocation, StackPointer).WithComment($"Begin access {node.Identifier}"));
            instructions.Add(new AddInstruction(readLocation, offset));
            for (int i = 0; i < type.Length; i++) {
                // Use that register to copy across the object
                instructions.Add(new MemCopyInstruction(StackPointer, readLocation));
                instructions.Add(new AddInstruction(readLocation, 1));
                instructions.Add(PushStack(SType.SInteger));
            }

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
