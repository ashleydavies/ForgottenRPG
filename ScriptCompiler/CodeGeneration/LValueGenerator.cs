using System;
using System.Collections.Generic;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.CodeGeneration.Assembly.Instructions;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Types;
using ScriptCompiler.Visitors;

namespace ScriptCompiler.CodeGeneration {
    public class LValueGenerator : Visitor<(List<Instruction>, Register)> {
        private readonly FunctionTypeRepository _functionTypeRepository;
        private readonly UserTypeRepository _userTypeRepository;
        private readonly StackFrame _stackFrame;
        private readonly RegisterManager _regManager;
        
        private TypeIdentifier TypeIdentifier => new TypeIdentifier(
            _functionTypeRepository, _userTypeRepository, _stackFrame);

        public LValueGenerator(FunctionTypeRepository functionTypeRepository, UserTypeRepository userTypeRepository,
                                StackFrame stackFrame, RegisterManager regManager) {
            _functionTypeRepository = functionTypeRepository;
            _userTypeRepository     = userTypeRepository;
            _stackFrame             = stackFrame;
            _regManager             = regManager;
        }

        public (List<Instruction>, Register) Generate(ASTNode node) {
            return this.Visit(node as dynamic);
        }

        public override (List<Instruction>, Register) Visit(ASTNode node) {
            throw new NotImplementedException(node.GetType().FullName);
        }

        public (List<Instruction>, Register) Visit(VariableAccessNode node) {
            var (_, offset, guid) = _stackFrame.Lookup(node.Identifier);
            var reg = _regManager.NewRegister();
            
            if (offset != null) {
                return (new List<Instruction> {
                    new MovInstruction(reg, _regManager.StackPointer),
                    new AddInstruction(reg, offset)
                }, reg);
            }

            return (new List<Instruction> {
                new MovInstruction(reg, new StaticLabel(guid.GetValueOrDefault()))
            }, reg);
        }

        public (List<Instruction>, Register) Visit(StructAccessNode node) {
            var (instructions, reg) = Generate(node.Left);
            var leftType = TypeIdentifier.Identify(node.Left);
            if (!(leftType is UserType structType)) {
                throw new CompileException($"Attempt to access field of non-user-type {leftType}", 0, 0);
            }
            instructions.Add(new AddInstruction(reg, structType.OffsetOfField(node.Field)));
            return (instructions, reg);
        }

        public (List<Instruction>, Register) Visit(DereferenceNode node) {
            var (instructions, reg) = Generate(node.Expression);
            instructions.Add(new MemReadInstruction(reg, reg));
            return (instructions, reg);
        }
    }
}
