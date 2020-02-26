using System;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Types;
using ScriptCompiler.Visitors;

namespace ScriptCompiler.CodeGeneration {
    public class TypeIdentifier : Visitor<SType> {
        private readonly FunctionTypeRepository _functionTypeRepository;
        private readonly UserTypeRepository _userTypeRepository;
        private readonly StackFrame _stackFrame;

        public TypeIdentifier(FunctionTypeRepository functionTypeRepository, UserTypeRepository userTypeRepository,
                              StackFrame stackFrame) {
            _functionTypeRepository = functionTypeRepository;
            _userTypeRepository     = userTypeRepository;
            _stackFrame             = stackFrame;
        }

        public SType Identify(ASTNode node) {
            return this.Visit(node as dynamic);
        }

        public override SType Visit(ASTNode node) {
            throw new NotImplementedException(node.GetType().FullName);
        }

        public SType Visit(FunctionCallNode node) {
            return _functionTypeRepository.ReturnType(node.FunctionName);
        }

        public SType Visit(VariableAccessNode node) {
            var (type, _) = _stackFrame.Lookup(node.Identifier);
            return type;
        }

        public SType Visit(IntegerLiteralNode _) {
            return SType.SInteger;
        }

        public SType Visit(StringLiteralNode _) {
            return SType.SString;
        }

        public SType Visit(StructAccessNode node) {
            SType userType = Identify(node.Left);
            if (userType is UserType uT) {
                return uT.TypeOfField(node.Field);
            }

            throw new CompileException($"Unexpected type {userType} in struct access", 0, 0);
        }

        public SType Visit(AddressOfNode node) {
            SType exprType = Identify(node.Expression);
            return new ReferenceType(exprType);
        }

        public SType Visit(DereferenceNode node) {
            SType exprType = Identify(node.Expression);
            if (!(exprType is ReferenceType refType)) {
                throw new CompileException($"Attempt to dereference non-pointer type {exprType}", 0, 0);
            }

            return refType.ContainedType;
        }

        public SType Visit(AssignmentNode node) {
            var valueType = Identify(node.Value);
            var destType  = Identify(node.Destination);
            if (!Equals(valueType, destType)) {
                throw new CompileException($"Mismatching types in assignment (from {valueType.Name} to {destType.Name}",
                                           0, 0);
            }

            return valueType;
        }

        public SType Visit(BinaryOperatorNode node) {
            var typeL = Identify(node.Left);
            var typeR = Identify(node.Right);

            if (ReferenceEquals(typeL, typeR)) {
                return typeL;
            }

            throw new Exception();
        }
    }
}
