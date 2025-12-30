using System;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Types;

namespace ScriptCompiler.CodeGeneration {
    public sealed class StackLValueEmitter {
        private readonly StackAssemblyEmitter _emitter;
        private readonly FunctionTypeRepository _functionTypeRepository;
        private readonly UserTypeRepository _userTypeRepository;
        private readonly StackFrame _stackFrame;
        private readonly Action<ExpressionNode> _emitExpression;

        public StackLValueEmitter(StackAssemblyEmitter emitter,
                                  FunctionTypeRepository functionTypeRepository,
                                  UserTypeRepository userTypeRepository,
                                  StackFrame stackFrame,
                                  Action<ExpressionNode> emitExpression) {
            _emitter = emitter;
            _functionTypeRepository = functionTypeRepository;
            _userTypeRepository = userTypeRepository;
            _stackFrame = stackFrame;
            _emitExpression = emitExpression;
        }

        private TypeIdentifier TypeIdentifier => new TypeIdentifier(
            _functionTypeRepository, _userTypeRepository, _stackFrame);

        public void EmitAddress(ASTNode node, int wordOffset = 0) {
            switch (node) {
                case VariableAccessNode variableAccess:
                    EmitVariableAddress(variableAccess, wordOffset);
                    return;
                case StructAccessNode structAccess:
                    EmitStructAddress(structAccess, wordOffset);
                    return;
                case DereferenceNode dereference:
                    EmitDereferenceAddress(dereference, wordOffset);
                    return;
                default:
                    throw new CompileException($"Unable to take address of {node.GetType().Name}", 0, 0);
            }
        }

        private void EmitVariableAddress(VariableAccessNode node, int wordOffset) {
            var (_, offset, guid) = _stackFrame.Lookup(node.Identifier);
            if (offset != null) {
                EmitPushStackPointerOffset(offset.Value + wordOffset);
                return;
            }

            var label = new StaticLabel(guid.GetValueOrDefault());
            _emitter.EmitText("push", label.ToString());
            if (wordOffset != 0) {
                EmitLiteral(wordOffset);
                _emitter.EmitText("add");
            }
        }

        private void EmitStructAddress(StructAccessNode node, int wordOffset) {
            EmitAddress(node.Left);
            var leftType = TypeIdentifier.Identify(node.Left);
            if (leftType is not UserType structType) {
                throw new CompileException($"Attempt to access field of non-user-type {leftType}", 0, 0);
            }

            EmitLiteral(structType.OffsetOfField(node.Field) + wordOffset);
            _emitter.EmitText("add");
        }

        private void EmitDereferenceAddress(DereferenceNode node, int wordOffset) {
            var pointerType = TypeIdentifier.Identify(node.Expression);
            if (pointerType is not ReferenceType) {
                throw new CompileException($"Attempt to dereference non-pointer type {pointerType}", 0, 0);
            }

            _emitExpression(node.Expression);

            if (wordOffset != 0) {
                EmitLiteral(wordOffset);
                _emitter.EmitText("add");
            }
        }

        private void EmitPushStackPointerOffset(int offset) {
            _emitter.EmitText("push", "r1");
            if (offset == 0) {
                return;
            }

            EmitLiteral(Math.Abs(offset));
            _emitter.EmitText(offset > 0 ? "add" : "sub");
        }

        private void EmitLiteral(int value) {
            _emitter.EmitText("literal", value.ToString());
        }

    }
}
