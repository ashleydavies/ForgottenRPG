using System;
using System.Collections.Generic;
using System.Linq;
using ForgottenRPG.Service;
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
        private readonly StaticVariableRepository _staticVariableRepository;
        private readonly Dictionary<string, StringLabel> _stringPoolAliases;
        private readonly RegisterManager _regManager = new RegisterManager();

        private StackFrame _stackFrame;

        private Register StackPointer => _regManager.StackPointer;

        private ExpressionGenerator ExpressionGenerator => new ExpressionGenerator(
            _functionTypeRepository, _userTypeRepository, _stringPoolAliases, _stackFrame, _regManager);

        private TypeIdentifier TypeIdentifier => new TypeIdentifier(
            _functionTypeRepository, _userTypeRepository, _stackFrame);

        internal StatementBlockGenerationVisitor(FunctionTypeRepository functionTypeRepository,
                                               UserTypeRepository userTypeRepository,
                                               StaticVariableRepository staticVariableRepository,
                                               Dictionary<string, StringLabel> stringPoolAliases,
                                               StackFrame? stackFrame = null) {
            _functionTypeRepository   = functionTypeRepository;
            _userTypeRepository       = userTypeRepository;
            _staticVariableRepository = staticVariableRepository;
            _stringPoolAliases        = stringPoolAliases;
            _stackFrame               = stackFrame ?? new StackFrame();
        }

        public List<Instruction> VisitStatementBlock(List<StatementNode> statements) {
            return statements.SelectMany<StatementNode, Instruction>(p => Visit(p as dynamic)).ToList();
        }

        public override List<Instruction> Visit(ASTNode node) {
            throw new System.NotImplementedException();
        }

        public List<Instruction> Visit(DeclarationStatementNode node) {
            if (_stackFrame.ExistsLocalAny(node.Identifier)) {
                throw new CompileException($"Attempt to redefine identifier {node.Identifier}", 0, 0);
            }

            SType type = node.TypeNode.GetSType(_userTypeRepository);
            if (type.IsUnknownType()) {
                throw new CompileException($"Unable to discern type from {node.TypeNode}", 0, 0);
            }

            var instructions = new List<Instruction>();

            if (node.Static) {
                var initialValue = new uint[type.Length];
                if (node.InitialValue != null) {
                    initialValue = node.InitialValue.Calculate(new CalcContext(true, _stackFrame, _staticVariableRepository))
                                ?? throw new CompileException("Failed to evaluate constexpr assignment", 0, 0);
                }
                _stackFrame.AddStaticIdentifier(type, node.Identifier, _staticVariableRepository.CreateNew(initialValue));
                
                return instructions;
            }
            
            _stackFrame.AddIdentifier(type, node.Identifier);
            // Adjust stack pointer to allocate space
            instructions.AddRange(AddStackPointer(type.Length));

            // Set up with default value, if any
            if (node.InitialValue != null) {
                instructions.AddRange(ExpressionGenerator.Generate(node.InitialValue));

                // Value is on Operand Stack.
                // We need to write it to the allocated space.
                // Dest = StackPointer - type.Length
                
                // Loop to pop and write.
                // Operand Stack: [W0, W1, ...]. Top is W(Last).
                // Memory: [W0, W1, ...].
                // We want to write W(Last) to Dest + Length - 1.
                // W(Last-1) to Dest + Length - 2.
                
                for (int i = 0; i < type.Length; i++) {
                    int offset = type.Length - 1 - i;
                    
                    // Pop Value to Temp Reg
                    var tempReg = _regManager.NewRegister();
                    using (tempReg) {
                        instructions.Add(new StackToRegisterInstruction(tempReg.Number));
                        
                        // Push Dest Address (StackPointer - type.Length + offset)
                        // = StackPointer - (type.Length - offset)
                        // = StackPointer - (i + 1)
                        
                        instructions.Add(new RegisterToStackInstruction(StackPointer.Number));
                        instructions.Add(new LiteralInstruction((uint)(i + 1)));
                        instructions.Add(new SubInstruction());
                        
                        // Push Value
                        instructions.Add(new RegisterToStackInstruction(tempReg.Number));
                        
                        instructions.Add(new MemWriteInstruction());
                    }
                }
            } else {
                // Move to start of object to write zeros
                // Actually, we can just loop and write zeros.
                // Dest = StackPointer - type.Length
                
                for (int i = 0; i < type.Length; i++) {
                    // Write 0 to StackPointer - 1 - i
                    instructions.Add(new RegisterToStackInstruction(StackPointer.Number));
                    instructions.Add(new LiteralInstruction((uint)(i + 1)));
                    instructions.Add(new SubInstruction());
                    
                    instructions.Add(new LiteralInstruction(0));
                    instructions.Add(new MemWriteInstruction());
                }
            }

            return instructions;
        }

        public List<Instruction> Visit(PrintStatementNode node) {
            var expressionType = TypeIdentifier.Identify(node.Expression);
            if (expressionType.Length != 1) {
                throw new CompileException("Unable to print multi-word expression", 0, 0);
            }

            var instructions = ExpressionGenerator.Generate(node.Expression);
            
            if (ReferenceEquals(expressionType, SType.SString)) {
                instructions.Add(new PrintInstruction());
            } else if (ReferenceEquals(expressionType, SType.SUInteger)) {
                instructions.Add(new PrintUIntInstruction());
            } else if (ReferenceEquals(expressionType, SType.SFloat)) {
                instructions.Add(new PrintFloatInstruction());
            } else {
                instructions.Add(new PrintIntInstruction());
            }

            return instructions;
        }

        public List<Instruction> Visit(IfStatementNode node) {
            var instructions = new List<Instruction>();

            instructions.AddRange(ExpressionGenerator.Generate(node.Expression));

            var elseLabel = new Label(Guid.NewGuid().ToString());
            var endLabel  = new Label(Guid.NewGuid().ToString());

            // Condition is on stack (0 or 1)
            // Push 0 to compare
            instructions.Add(new LiteralInstruction(0));
            
            // If 0 (False), Jump to Else
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
        
        public List<Instruction> Visit(ForStatementNode node) {
            var instructions = new List<Instruction>();
            
            var startLabel = new Label(Guid.NewGuid().ToString());
            var endLabel = new Label(Guid.NewGuid().ToString());

            NewStackFrame();
            if (node.Declaration != null) {
                instructions.AddRange(Visit(node.Declaration as dynamic));
            }
            
            instructions.Add(new LabelInstruction(startLabel));

            if (node.Condition != null) {
                instructions.AddRange(ExpressionGenerator.Generate(node.Condition));

                // Condition is on stack
                instructions.Add(new LiteralInstruction(0));
                instructions.Add(new JmpEqInstruction(endLabel));
            }
            
            NewStackFrame();
            instructions.AddRange(VisitStatementBlock(node.Block.Statements));
            PopStackFrame(instructions);

            if (node.Update != null) {
                instructions.AddRange(ExpressionGenerator.Generate(node.Update));
                // Discard result of update expression
                var type = TypeIdentifier.Identify(node.Update);
                for(int i=0; i<type.Length; i++) {
                    // Pop to dummy register
                    instructions.Add(new StackToRegisterInstruction(2)); 
                }
            }
            
            instructions.Add(new JmpInstruction(startLabel));
            instructions.Add(new LabelInstruction(endLabel));
            PopStackFrame(instructions);
            return instructions;
        }

        private void PopStackFrame(List<Instruction> instructions) {
            var (newFrame, len) = _stackFrame.Purge();
            _stackFrame         = newFrame!;
            instructions.AddRange(SubStackPointer(len));
        }

        private StackFrame NewStackFrame() {
            return _stackFrame = new StackFrame(_stackFrame);
        }

        public List<Instruction> Visit(ReturnStatementNode node) {
            var instructions = ExpressionGenerator.Generate(node.Expression);

            // Value is on Operand Stack.
            // We need to:
            // 1. Read RetAddr (from Memory) to a Temp Register.
            // 2. SubStackPointer (Locals + RetAddr).
            // 3. Jump to RetAddr (by writing to IP).

            var (type, offset, _) = _stackFrame.Lookup("__RET_ADDR__");
            if (offset == null) {
                throw new CompileException("Return address not found in stack frame", 0, 0);
            }

            // 1. Read RetAddr to Temp Reg
            var retAddrReg = _regManager.NewRegister();
            using (retAddrReg) {
                // Push Address of RetAddr (SP + offset)
                instructions.Add(new RegisterToStackInstruction(StackPointer.Number));
                instructions.Add(new LiteralInstruction((uint)Math.Abs(offset.Value)));
                if (offset.Value > 0) {
                    instructions.Add(new AddInstruction());
                } else {
                    instructions.Add(new SubInstruction());
                }
                
                // Read RetAddr
                instructions.Add(new MemReadInstruction());
                
                // Pop RetAddr to Reg
                instructions.Add(new StackToRegisterInstruction(retAddrReg.Number));
                
                // 2. SubStackPointer
                // We need to pop Locals and RetAddr.
                // StackFrame.Purge() gives us Locals size.
                // RetAddr size is 1.
                var (_, localsSize) = _stackFrame.Purge();
                instructions.AddRange(SubStackPointer(localsSize + 1));
                
                // 3. Jump to RetAddr
                // Push RetAddr from Reg
                instructions.Add(new RegisterToStackInstruction(retAddrReg.Number));
                // Pop to IP
                instructions.Add(new StackToRegisterInstruction(_regManager.InstructionPointer.Number));
            }

            return instructions;
        }

        public List<Instruction> Visit(ExpressionNode node) {
            var instructions = ExpressionGenerator.Generate(node);
            // Discard result
            var type = TypeIdentifier.Identify(node);
            for(int i=0; i<type.Length; i++) {
                instructions.Add(new StackToRegisterInstruction(2)); 
            }
            return instructions;
        }

        private List<Instruction> PushStack(SType type) {
            _stackFrame.Pushed(type);
            return AddStackPointer(type.Length);
        }

        private List<Instruction> PopStack(SType type) {
            _stackFrame.Popped(type);
            return SubStackPointer(type.Length);
        }
        
        private List<Instruction> AddStackPointer(int amount) {
            var instructions = new List<Instruction>();
            instructions.Add(new RegisterToStackInstruction(StackPointer.Number));
            instructions.Add(new LiteralInstruction((uint)amount));
            instructions.Add(new AddInstruction());
            instructions.Add(new StackToRegisterInstruction(StackPointer.Number));
            return instructions;
        }
        
        private List<Instruction> SubStackPointer(int amount) {
            var instructions = new List<Instruction>();
            instructions.Add(new RegisterToStackInstruction(StackPointer.Number));
            instructions.Add(new LiteralInstruction((uint)amount));
            instructions.Add(new SubInstruction());
            instructions.Add(new StackToRegisterInstruction(StackPointer.Number));
            return instructions;
        }
    }
}
