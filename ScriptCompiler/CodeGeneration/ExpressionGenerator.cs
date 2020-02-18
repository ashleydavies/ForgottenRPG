using System.Collections.Generic;
using ForgottenRPG.VM;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements.Expressions;
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
            instructions.Add(new MovInstruction(readLocation, StackPointer));
            instructions.Add(new SubInstruction(readLocation, offset));
            for (int i = 0; i < type.Length; i++) {
                // Use that register to copy across the object
                instructions.Add(PushStack(SType.SInteger));
                instructions.Add(new AddInstruction(readLocation, 1));
                instructions.Add(new MemCopyInstruction(StackPointer, readLocation));
            }
            
            return instructions;
        }

        public List<Instruction> Visit(IntegerLiteralNode node) {
            return new List<Instruction> {
                new MemWriteInstruction(StackPointer, node.Value),
                PushStack(SType.SInteger)
            };
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
