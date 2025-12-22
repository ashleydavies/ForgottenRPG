using System;
using System.Collections.Generic;
using System.Linq;
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

        private LValueGenerator LValueGenerator => new LValueGenerator(
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
            throw new NotImplementedException(node.GetType().FullName);
        }

        public List<Instruction> Visit(VariableAccessNode node) {
            var instructions = new List<Instruction>();
            var (type, offset, guid) = _stackFrame.Lookup(node.Identifier);
            if (Equals(type, SType.SNoType)) {
                throw new CompileException($"Unknown variable {node.Identifier}", 0, 0);
            }

            for (int i = 0; i < type.Length; i++) {
                if (offset != null) {
                     instructions.Add(new RegisterToStackInstruction(StackPointer.Number));
                     instructions.Add(new LiteralInstruction((uint)(offset + i)));
                     instructions.Add(new AddInstruction());
                } else {
                     instructions.Add(new LabelLiteralInstruction(new StaticLabel(guid.GetValueOrDefault())));
                     if (i > 0) {
                         instructions.Add(new LiteralInstruction((uint)i));
                         instructions.Add(new AddInstruction());
                     }
                }
                
                instructions.Add(new MemReadInstruction());
            }

            return instructions;
        }

        public List<Instruction> Visit(FunctionCallNode node) {
            var instructions = new List<Instruction>();
            
            // 1. Push space for return value (if not void)
            // REMOVED: Return value is now returned on Operand Stack.
            var returnType = _functionTypeRepository.ReturnType(node.FunctionRef(TypeIdentifier));
            // We don't allocate space on Memory Stack anymore.

            // 2. Push arguments to Memory Stack
            foreach (var arg in node.Args) {
                // Generate Arg (Result on Operand Stack)
                // Generate Arg (Result on Operand Stack)
                instructions.AddRange(Generate(arg));
                
                var argType = TypeIdentifier.Identify(arg);
                for (int i = 0; i < argType.Length; i++) {
                    // So we pop W(Last) and write to SP.
                    // But we want SP to grow.
                    // If we push W0, W1.
                    // Memory: [W0, W1].
                    // So we should write W0 first.
                    // But W0 is at bottom of Operand Stack.
                    // We need to pop in reverse order?
                    // Or just pop and push to Memory Stack?
                    // If we pop Top (W1), write to SP. Inc SP.
                    // Then pop Next (W0), write to SP. Inc SP.
                    // Memory: [W1, W0]. Reversed!
                    
                    // So we need to reverse the words on Operand Stack?
                    // Or write in reverse order?
                    // But we can't access below Top.
                    
                    // Maybe we should use a temp register to reverse?
                    // Or maybe Generate(arg) should push in reverse order?
                    // No, Generate(arg) pushes in standard order.
                    
                    // Actually, if we pop W1, we can write it to SP + 1.
                    // Pop W0, write it to SP + 0.
                    // Then Inc SP by 2.
                    // But we can't access W0 until we pop W1.
                    
                    // So:
                    // Pop W1 to Reg1.
                    // Pop W0 to Reg2.
                    // Write Reg2 to SP.
                    // Write Reg1 to SP+1.
                    // Inc SP by 2.
                    
                    // This requires N registers for N words.
                    // Max struct size?
                    // Maybe we can just use the stack?
                    // If we have [W0, W1].
                    // We want [W0, W1] in memory.
                    // If we write W1 to SP+1. Then W0 to SP.
                    // We need to pop W1, save it?
                    // Or can we just write it?
                    // Pop W1. Write to SP+1.
                    // Pop W0. Write to SP.
                    // Yes!
                }
                
                // Wait, the loop above was hypothetical.
                // We need to implement it.
                
                // For each word in argType (reversed):
                for (int i = 0; i < argType.Length; i++) {
                    int offset = argType.Length - 1 - i;
                    
                    // Pop Value to Temp Reg
                    // We can reuse a register if we write immediately.
                    // But we need to calculate address SP + offset.
                    // SP is in r1.
                    
                    // Push Address (SP + offset)
                    instructions.Add(new RegisterToStackInstruction(StackPointer.Number));
                    if (offset > 0) {
                        instructions.Add(new LiteralInstruction((uint)offset));
                        instructions.Add(new AddInstruction());
                    }
                    
                    // Value is on Operand Stack.
                    // But MemWrite needs [Address, Value].
                    // Stack: [..., Value].
                    // We pushed Address.
                    // Stack: [..., Value, Address].
                    // MemWrite expects [Address, Value]?
                    // If MemWrite is Pop Val, Pop Addr -> Write Val to Addr.
                    // Then Stack: [Addr, Val] is needed.
                    // We have [Val, Addr].
                    // Swap? No Swap.
                    
                    // So we MUST use a temp register for Value.
                    // Pop Value to Reg.
                    // Push Addr.
                    // Push Value (from Reg).
                    // MemWrite.
                    
                    var tempReg = _regManager.NewRegister();
                    using (tempReg) {
                        instructions.Add(new StackToRegisterInstruction(tempReg.Number)); // Pop Value
                        
                        instructions.Add(new StackToRegisterInstruction(tempReg.Number)); // Pop Value
                        
                        // Push Address (SP + offset)
                        instructions.Add(new RegisterToStackInstruction(StackPointer.Number));
                        if (offset > 0) {
                            instructions.Add(new LiteralInstruction((uint)offset));
                            instructions.Add(new AddInstruction());
                        }
                        
                        instructions.Add(new RegisterToStackInstruction(tempReg.Number));
                        instructions.Add(new MemWriteInstruction());
                    }
                }
                
                // Inc SP by argType.Length
                instructions.AddRange(AddStackPointer(argType.Length));
            }
            
            // 3. Push Return Address (Label)
            var returnLabel = new Label(Guid.NewGuid().ToString());
            instructions.Add(new RegisterToStackInstruction(StackPointer.Number));
            instructions.Add(new LabelLiteralInstruction(returnLabel));
            instructions.Add(new MemWriteInstruction());
            
            // Inc StackPointer
            instructions.AddRange(AddStackPointer(1));
            
            // 4. Jump to function
            instructions.Add(new JmpInstruction(new Label(node.AsmLabel(TypeIdentifier))));
            
            // 5. Label Return Address
            instructions.Add(new LabelInstruction(returnLabel));
            
            // 6. Pop arguments
            int argsSize = 0;
            foreach (var arg in node.Args) {
                argsSize += TypeIdentifier.Identify(arg).Length;
            }
            instructions.AddRange(SubStackPointer(argsSize));
            
            // 7. Return value is now at top of stack (if not void).
            // Wait, if we changed convention to return on Operand Stack, we don't need to read from memory.
            // But we allocated space in step 1?
            // If we change convention, we should remove step 1.
            // And step 7 is just "leave it there".
            
            // Let's assume we change convention: Return on Operand Stack.
            // So remove step 1.
            // And step 7 is implicit.
            
            // But wait, I need to remove step 1 code.
            // And ensure GenerateFunction returns on Operand Stack.
            
            // So I will remove step 1 and update step 7.
            
            return instructions;
        }

        public List<Instruction> Visit(IntegerLiteralNode node) {
            var instructions = new List<Instruction>();
            instructions.Add(new LiteralInstruction((uint)node.Value));
            return instructions;
        }

        public List<Instruction> Visit(FloatLiteralNode node) {
            var instructions = new List<Instruction>();
            instructions.Add(new LiteralInstruction((uint)BitConverter.SingleToInt32Bits(node.Value)));
            return instructions;
        }

        public List<Instruction> Visit(SizeOfNode node) {
            var instructions = new List<Instruction>();
            // Special case for SizeOfNode: Allow type names
            int result;
            if (node.Arg is VariableAccessNode n && _userTypeRepository.ExistsType(n.Identifier)) {
                result = _userTypeRepository[n.Identifier].Length;
            } else {
                result = TypeIdentifier.Identify(node.Arg).Length;
            }

            instructions.Add(new LiteralInstruction((uint)result));
            return instructions;
        }

        public List<Instruction> Visit(StringLiteralNode node) {
            var instructions = new List<Instruction>();
            if (!_stringPoolAliases.ContainsKey(node.String)) {
                throw new ArgumentException();
            }

            instructions.Add(new LabelLiteralInstruction(_stringPoolAliases[node.String]));
            return instructions;
        }

        public List<Instruction> Visit(AssignmentNode node) {
            var instructions = new List<Instruction>();
            var nodeType = TypeIdentifier.Identify(node);
            var (lvalueInstructions, pointerReg) = LValueGenerator.Generate(node.Destination);
            instructions.AddRange(lvalueInstructions);
            instructions[0].WithComment("Begin assignment");
            using (pointerReg) {
                // Address is in pointerReg.
                // Generate Value (pushes to stack).
                instructions.AddRange(Generate(node.Value));
                
                // Stack: [Value]. Address in pointerReg.
                // We need to write Value to Address.
                // But we also need to leave Value on the stack (Assignment is an expression).
                // Since we don't have Dup, we'll use a temp register for the value.
                
                var valReg = _regManager.NewRegister();
                using (valReg) {
                    for (int i = 0; i < nodeType.Length; i++) {
                        // Pop Value part to valReg
                        instructions.Add(new StackToRegisterInstruction(valReg.Number));
                        
                        // Write to Memory
                        // Dest = pointerReg + (Length - 1 - i)  (Since we pop in reverse order)
                        // Wait, if we push [W0, W1], we pop W1 then W0.
                        // So first pop is W(Last).
                        
                        int offset = nodeType.Length - 1 - i;
                        
                        // Push Dest (pointerReg + offset)
                        instructions.Add(new RegisterToStackInstruction(pointerReg.Number));
                        if (offset > 0) {
                            instructions.Add(new LiteralInstruction((uint)offset));
                            instructions.Add(new AddInstruction());
                        }
                        
                        // Push Value (from valReg)
                        instructions.Add(new RegisterToStackInstruction(valReg.Number));
                        
                        instructions.Add(new MemWriteInstruction());
                        
                        // Push Value back to stack (to return it)
                        // But we are iterating. If we push back now, it will be on top.
                        // We need to reconstruct the stack [W0, W1].
                        // If we pop W1, write it, push W1.
                        // Then pop W0, write it, push W0.
                        // Stack becomes [W1, W0]. Reversed!
                        // So we need to pop all to temp storage, then write, then push back?
                        // Or just use MemRead from the destination after writing?
                        // Yes, MemRead is easier.
                    }
                    
                    // Re-read value from memory to stack
                    for (int i = 0; i < nodeType.Length; i++) {
                        instructions.Add(new RegisterToStackInstruction(pointerReg.Number));
                        if (i > 0) {
                            instructions.Add(new LiteralInstruction((uint)i));
                            instructions.Add(new AddInstruction());
                        }
                        instructions.Add(new MemReadInstruction());
                    }
                }
            }

            return instructions;
        }

        public List<Instruction> Visit(StructAccessNode node) {
            // If Left is addressable, use LValueGenerator to get address, then MemRead.
            if (AddressabilityChecker.Check(node.Left)) {
                var (lvalueInstructions, reg) = LValueGenerator.Generate(node); // Generate(node) calls Visit(StructAccessNode) in LValueGenerator
                // Wait, LValueGenerator.Visit(StructAccessNode) returns address of the field.
                // So we just need to MemRead.
                var instructions = new List<Instruction>();
                instructions.AddRange(lvalueInstructions);
                
                var fieldType = (TypeIdentifier.Identify(node.Left) as UserType).TypeOfField(node.Field);
                
                using (reg) {
                    for (int i = 0; i < fieldType.Length; i++) {
                        instructions.Add(new RegisterToStackInstruction(reg.Number));
                        if (i > 0) {
                            instructions.Add(new LiteralInstruction((uint)i));
                            instructions.Add(new AddInstruction());
                        }
                        instructions.Add(new MemReadInstruction());
                    }
                }
                return instructions;
            } else {
                // RValue Struct Access.
                // Generate(Left) pushes struct to stack.
                // We need to extract field.
                // Stack: [W0, W1, ...].
                // We need to pop unwanted words and keep field words.
                
                var instructions = Generate(node.Left);
                var structType = TypeIdentifier.Identify(node.Left) as UserType;
                var fieldType = structType.TypeOfField(node.Field);
                var fieldOffset = structType.OffsetOfField(node.Field);
                
                // Struct is on stack. Top is Last Word.
                // [Field0, Field1, ...].
                // We want to keep Field words.
                // Field is at offset `fieldOffset` (from bottom of struct).
                // Length `fieldType.Length`.
                
                // We need to pop (StructLength - (fieldOffset + fieldLength)) words (Top part).
                // Then keep `fieldLength` words.
                // Then pop `fieldOffset` words (Bottom part).
                
                int structLength = structType.Length;
                int wordsAbove = structLength - (fieldOffset + fieldType.Length);
                int wordsBelow = fieldOffset;
                
                // Pop wordsAbove
                for (int i = 0; i < wordsAbove; i++) {
                    instructions.Add(new StackToRegisterInstruction(_regManager.NewRegister().Number)); // Discard
                    // Note: We are leaking registers here if we don't reuse them.
                    // But _regManager.NewRegister() just returns a Register object.
                    // It doesn't allocate resource that needs freeing?
                    // RegisterManager manages a pool?
                    // RegisterManager.NewRegister() returns a Register.
                    // We should probably use a single dummy register for discarding.
                }
                
                // Pop field words to temp storage (to preserve them)
                // We need to preserve them while we pop wordsBelow.
                // We can use registers.
                var tempRegs = new List<Register>();
                for (int i = 0; i < fieldType.Length; i++) {
                    var reg = _regManager.NewRegister();
                    tempRegs.Add(reg);
                    instructions.Add(new StackToRegisterInstruction(reg.Number));
                }
                
                // Pop wordsBelow
                for (int i = 0; i < wordsBelow; i++) {
                    instructions.Add(new StackToRegisterInstruction(_regManager.NewRegister().Number)); // Discard
                }
                
                // Push field words back (in reverse order of popping, so they are in correct order)
                // We popped Top first.
                // Stack: [W0, W1]. Pop W1, Pop W0.
                // Push W0, Push W1.
                // tempRegs has [W(Last), W(Last-1), ...].
                // We want to push W0 first?
                // No, we want to restore the stack segment.
                // If we had [F0, F1].
                // We popped F1, then F0.
                // tempRegs = [F1, F0].
                // We want [F0, F1].
                // So Push F0, then F1.
                // So iterate tempRegs in reverse.
                
                for (int i = tempRegs.Count - 1; i >= 0; i--) {
                    instructions.Add(new RegisterToStackInstruction(tempRegs[i].Number));
                }
                
                return instructions;
            }
        }

        public List<Instruction> Visit(AddressOfNode node) {
            if (!AddressabilityChecker.Check(node.Expression)) {
                throw new CompileException("Attempt to take the address of an unaddressable expression", 0, 0);
            }

            var (lvalueInstructions, reg) = LValueGenerator.Generate(node.Expression);
            var instructions = new List<Instruction>(lvalueInstructions);
            using (reg) {
                // Address is in reg.
                // Push reg to stack.
                instructions.Add(new RegisterToStackInstruction(reg.Number));
            }
            
            return instructions;
        }

        public List<Instruction> Visit(DereferenceNode node) {
            var pointerType   = TypeIdentifier.Identify(node.Expression);
            var referenceType = (pointerType as ReferenceType);
            if (referenceType == null) {
                throw new CompileException($"Unable to dereference expression of type {pointerType}", 0, 0);
            }

            var instructions = Generate(node.Expression);
            
            // Pointer is on Stack.
            // We need to MemRead 'type.Length' words.
            // Stack: [Pointer].
            // We need to keep Pointer to increment it?
            // Or pop to register.
            
            var reg = _regManager.NewRegister();
            using (reg) {
                instructions.Add(new StackToRegisterInstruction(reg.Number));
                
                var type = referenceType.ContainedType;
                for (int i = 0; i < type.Length; i++) {
                    instructions.Add(new RegisterToStackInstruction(reg.Number));
                    if (i > 0) {
                        instructions.Add(new LiteralInstruction((uint)i));
                        instructions.Add(new AddInstruction());
                    }
                    instructions.Add(new MemReadInstruction());
                }
            }

            return instructions;
        }

        public List<Instruction> Visit(EqualityOperatorNode node) {
            return VisitComparisonOperator(node, label => new JmpEqInstruction(label));
        }

        public List<Instruction> Visit(InequalityOperatorNode node) {
            return VisitComparisonOperator(node, label => new JmpNeqInstruction(label));
        }

        public List<Instruction> Visit(GreaterThanOperatorNode node) {
            return VisitComparisonOperator(node, label => new JmpGtInstruction(label));
        }

        public List<Instruction> Visit(LessThanOperatorNode node) {
            return VisitComparisonOperator(node, label => new JmpLtInstruction(label));
        }

        public List<Instruction> Visit(GreaterThanEqualOperatorNode node) {
            return VisitComparisonOperator(node, label => new JmpGteInstruction(label));
        }

        public List<Instruction> Visit(LessThanEqualOperatorNode node) {
            return VisitComparisonOperator(node, label => new JmpLteInstruction(label));
        }

        private List<Instruction> VisitComparisonOperator(BinaryOperatorNode node,
                                                          Func<Label, Instruction> operationGenerator) {
             // Generate Left and Right
             var instructions = new List<Instruction>();
             instructions.AddRange(Generate(node.Left));
             instructions.AddRange(Generate(node.Right));
             
             // Stack: [Left, Right]
             // Cmp pops 2 values.
             // But wait, Cmp instruction in VM usually sets flags or pushes result?
             // If Cmp pushes result (-1, 0, 1), then we need to process it.
             // But existing code did: Cmp, then JmpEq/Neq etc.
             // This implies Cmp sets internal flags?
             // Assembler.cs: case CmpInstruction _: AddInstruction(ScriptVm.Instruction.Cmp);
             // If VM has flags, then Cmp sets them.
             // And JmpEq jumps based on flags.
             
             // So:
             instructions.Add(new CmpInstruction());
             
             var endLabel = new Label(Guid.NewGuid().ToString());
             var trueLabel  = new Label(Guid.NewGuid().ToString());
             
             instructions.Add(operationGenerator(trueLabel));
             
             // False case (0)
             instructions.Add(new LiteralInstruction(0));
             instructions.Add(new JmpInstruction(endLabel));
             
             // True case (1)
             instructions.Add(new LabelInstruction(trueLabel));
             instructions.Add(new LiteralInstruction(1));
             
             instructions.Add(new LabelInstruction(endLabel));
             
             return instructions;
        }

        public List<Instruction> Visit(AdditionNode node) {
            return VisitArithmeticOperator(node, new AddInstruction(), new AddFloatInstruction());
        }

        public List<Instruction> Visit(SubtractionNode node) {
            return VisitArithmeticOperator(node, new SubInstruction(), new SubFloatInstruction());
        }

        public List<Instruction> Visit(MultiplicationNode node) {
            return VisitArithmeticOperator(node, new MulInstruction(), new MulFloatInstruction());
        }

        public List<Instruction> Visit(DivisionNode node) {
            return VisitArithmeticOperator(node, new DivInstruction(), new DivFloatInstruction());
        }

        public List<Instruction> VisitArithmeticOperator(BinaryOperatorNode node, Instruction op, Instruction floatOp) {
            var instructions = new List<Instruction>();
            instructions.AddRange(Generate(node.Left));
            instructions.AddRange(Generate(node.Right));
            
            var type = TypeIdentifier.Identify(node.Left);
            
            if (ReferenceEquals(type, SType.SFloat)) {
                instructions.Add(floatOp);
            } else {
                instructions.Add(op);
            }
            
            return instructions;
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
