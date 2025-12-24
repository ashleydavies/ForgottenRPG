using System;
using System.Linq;
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
        private readonly ForgottenRPG.NativeFunctions.NativeFunctionRegistry _nativeRegistry;

        public TypeIdentifier(FunctionTypeRepository functionTypeRepository, UserTypeRepository userTypeRepository,
                              StackFrame stackFrame, ForgottenRPG.NativeFunctions.NativeFunctionRegistry nativeRegistry = null) {
            _functionTypeRepository = functionTypeRepository;
            _userTypeRepository     = userTypeRepository;
            _stackFrame             = stackFrame;
            _nativeRegistry         = nativeRegistry;
        }

        public SType Identify(ASTNode node) {
            return this.Visit(node as dynamic);
        }

        public override SType Visit(ASTNode node) {
            throw new NotImplementedException(node.GetType().FullName);
        }

        public SType Visit(FunctionCallNode node) {
            if (_nativeRegistry != null && node.Function is VariableAccessNode vn && _nativeRegistry.HasFunction(vn.Identifier)) {
                var func = _nativeRegistry.GetByName(vn.Identifier);
                var returnType = func.Method.ReturnType;

                // Check if it's a registered reference type
                var registeredType = _nativeRegistry.GetRegisteredTypes().FirstOrDefault(x => x.Value == returnType);
                if (registeredType.Key != null) {
                    // It is! Return a pointer to the script type (e.g. Entity*)
                    if (_userTypeRepository.ExistsType(registeredType.Key)) {
                        return new ReferenceType(_userTypeRepository[registeredType.Key]);
                    }
                }

                return SType.FromType(returnType);
            }
            return _functionTypeRepository.ReturnType(node.FunctionRef(this));
        }

        public SType Visit(VariableAccessNode node) {
            var (type, _, _) = _stackFrame.Lookup(node.Identifier);
            return type;
        }

        public SType Visit(IntegerLiteralNode _) {
            return SType.SInteger;
        }

        public SType Visit(FloatLiteralNode _) {
            return SType.SFloat;
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

        public SType Visit(SizeOfNode node) {
            return SType.SInteger;
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

        public SType Visit(ComparatorNode node) {
            return SType.SBool;
        }

        public SType Visit(BinaryOperatorNode node) {
            var typeL = Identify(node.Left);
            var typeR = Identify(node.Right);

            if (ReferenceEquals(typeL, typeR)) {
                return typeL;
            }
            
            // Allow pointer arithmetic: Pointer + Int = Pointer
            if (typeL is ReferenceType && ReferenceEquals(typeR, SType.SInteger)) {
                return typeL;
            }
            if (typeR is ReferenceType && ReferenceEquals(typeL, SType.SInteger)) {
                return typeR;
            }

            throw new Exception($"Type mismatch in binary operator: {typeL.Name} and {typeR.Name}");
        }
    }
}
