using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.CodeGeneration.Assembly.Instructions;
using ScriptCompiler.Types;
using ScriptCompiler.Visitors;
using Register = ScriptCompiler.CodeGeneration.Assembly.Register;
using StackFrame = ScriptCompiler.CompileUtil.StackFrame;

namespace ScriptCompiler.CodeGeneration {
    public class StatementBlockGenerationVisitor : Visitor<List<Instruction>> {
        private readonly FunctionTypeRepository _functionTypeRepository;
        private readonly UserTypeRepository _userTypeRepository;
        private readonly Dictionary<string, StringLabel> _stringPoolAliases;
        private readonly RegisterManager _regManager = new RegisterManager();

        private StackFrame _stackFrame;

        private Register StackPointer => _regManager.StackPointer;

        private ExpressionGenerator ExpressionGenerator => new ExpressionGenerator(
            _functionTypeRepository, _userTypeRepository, _stringPoolAliases, _stackFrame, _regManager);

        private TypeIdentifier TypeIdentifier => new TypeIdentifier(
            _functionTypeRepository, _userTypeRepository, _stackFrame);

        public StatementBlockGenerationVisitor(FunctionTypeRepository functionTypeRepository,
                                               UserTypeRepository userTypeRepository,
                                               Dictionary<string, StringLabel> stringPoolAliases,
                                               StackFrame? stackFrame = null) {
            _functionTypeRepository = functionTypeRepository;
            _userTypeRepository     = userTypeRepository;
            _stringPoolAliases      = stringPoolAliases;
            _stackFrame             = stackFrame ?? new StackFrame();
        }

        public List<Instruction> VisitStatementBlock(List<StatementNode> statements) {
            return statements.SelectMany<StatementNode, Instruction>(p => Visit(p as dynamic)).ToList();
        }

        public override List<Instruction> Visit(ASTNode node) {
            throw new System.NotImplementedException();
        }

        public List<Instruction> Visit(DeclarationStatementNode node) {
            var instructions = new List<Instruction>();

            if (_stackFrame.ExistsLocalScope(node.Identifier)) {
                // TODO: Add line and col numbers (as well as other debug info) to all nodes, and report correctly here
                throw new CompileException($"Attempt to redefine identifier {node.Identifier}", 0, 0);
            }

            SType type = node.TypeNode.GetSType(_userTypeRepository);
            if (type.IsUnknownType()) {
                throw new CompileException($"Unable to discern type from {node.TypeNode}", 0, 0);
            }

            _stackFrame.AddIdentifier(type, node.Identifier);
            // Adjust stack pointer
            instructions.Add(new AddInstruction(StackPointer, type.Length)
                                 .WithComment($"Declaring {node.Identifier}"));

            // Set up with default value, if any
            if (node.InitialValue != null) {
                instructions.AddRange(ExpressionGenerator.Generate(node.InitialValue));

                // The result is now at the top of the stack so copy it where we want it
                // Set up a register offset from the stack pointer by type.Length
                using var writeLocation = _regManager.NewRegister();
                instructions.Add(new MovInstruction(writeLocation, StackPointer));
                instructions.Add(new SubInstruction(writeLocation, type.Length));
                for (int i = 0; i < type.Length; i++) {
                    // Use that register to copy across the object
                    instructions.Add(new SubInstruction(StackPointer, 1));
                    instructions.Add(new SubInstruction(writeLocation, 1));
                    instructions.Add(new MemCopyInstruction(writeLocation, StackPointer));
                }

                // We dealt with the stack push from the expression, so let the stack frame know
                _stackFrame.Popped(type);
            } else {
                instructions.Add(new SubInstruction(StackPointer, type.Length)
                                     .WithComment("Move to start of object to write zeros"));
                for (int i = 0; i < type.Length; i++) {
                    instructions.Add(new MemWriteInstruction(StackPointer, 0));
                    instructions.Add(new AddInstruction(StackPointer, 1));
                }
            }

            return instructions;
        }

        public List<Instruction> Visit(PrintStatementNode node) {
            // We can only currently print one word things...
            var expressionType = TypeIdentifier.Identify(node.Expression);
            if (expressionType.Length != 1) {
                throw new CompileException("Unable to print multi-word expression", 0, 0);
            }

            var instructions = ExpressionGenerator.Generate(node.Expression);
            instructions.Add(PopStack(expressionType));

            using var register = _regManager.NewRegister();
            instructions.Add(new MemReadInstruction(register, StackPointer));

            if (ReferenceEquals(expressionType, SType.SString)) {
                instructions.Add(new PrintInstruction(register));
            } else {
                instructions.Add(new PrintIntInstruction(register));
            }

            return instructions;
        }

        public List<Instruction> Visit(IfStatementNode node) {
            var instructions = new List<Instruction>();

            // We expect this to add a bool to the stack
            instructions.AddRange(ExpressionGenerator.Generate(node.Expression));

            var elseLabel = new Label(Guid.NewGuid().ToString());
            var endLabel  = new Label(Guid.NewGuid().ToString());

            instructions.Add(PopStack(SType.SBool));
            using var resultReg = _regManager.NewRegister();
            using var cmpReg    = _regManager.NewRegister();

            // Condition
            instructions.Add(new MemReadInstruction(resultReg, StackPointer));
            instructions.Add(new MovInstruction(cmpReg, 0));
            instructions.Add(new CmpInstruction(resultReg, cmpReg));
            instructions.Add(new JmpEqInstruction(node.ElseBlock != null ? elseLabel : endLabel));
            instructions.AddRange(VisitStatementBlock(node.IfBlock.Statements));
            if (node.ElseBlock != null) {
                instructions.Add(new JmpInstruction(endLabel));
                instructions.Add(new LabelInstruction(elseLabel));
                instructions.AddRange(VisitStatementBlock(node.ElseBlock.Statements));
            }
            instructions.Add(new LabelInstruction(endLabel));

            return instructions;
        }

        public List<Instruction> Visit(ReturnStatementNode node) {
            var instructions = ExpressionGenerator.Generate(node.Expression);

            // Copy the result over to the return position
            var (type, offset) = _stackFrame.Lookup(StackFrame.ReturnIdentifier);

            using var copyRegister = _regManager.NewRegister();
            // Ret1 | Ret2 | Ret3 | Stk1 | Stk2 | Stk3 | Res1 | Res2 | Res3 | 0
            //                                                                ^ Initial stack pointer
            // ^ StackPointer + offset
            //                       ^ StackPointer + offset + type.Length
            instructions.Add(new MovInstruction(copyRegister, StackPointer));
            instructions.Add(new AddInstruction(copyRegister, offset + type.Length));

            for (int i = 0; i < type.Length; i++) {
                instructions.Add(new SubInstruction(StackPointer, 1));
                instructions.Add(new SubInstruction(copyRegister, 1));
                instructions.Add(new MemCopyInstruction(copyRegister, StackPointer));
            }

            _stackFrame.Popped(type);
            return instructions;
        }

        public List<Instruction> Visit(ExpressionNode node) {
            // Process the expression and then discard the result
            var type         = TypeIdentifier.Identify(node);
            var instructions = ExpressionGenerator.Generate(node);
            instructions.Add(PopStack(TypeIdentifier.Identify(node)));
            return instructions;
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
