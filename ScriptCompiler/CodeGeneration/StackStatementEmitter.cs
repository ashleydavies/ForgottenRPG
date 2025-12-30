using System;
using System.Collections.Generic;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Types;

namespace ScriptCompiler.CodeGeneration {
    public sealed class StackStatementEmitter {
        private readonly StackAssemblyEmitter _emitter;
        private readonly FunctionTypeRepository _functionTypeRepository;
        private readonly UserTypeRepository _userTypeRepository;
        private readonly StaticVariableRepository _staticVariableRepository;
        private readonly Dictionary<string, StringLabel> _stringPoolAliases;
        private readonly ForgottenRPG.NativeFunctions.NativeFunctionRegistry _nativeRegistry;
        private readonly string _functionEndLabel;

        private StackFrame _stackFrame;

        public StackStatementEmitter(StackAssemblyEmitter emitter,
                                     FunctionTypeRepository functionTypeRepository,
                                     UserTypeRepository userTypeRepository,
                                     StaticVariableRepository staticVariableRepository,
                                     Dictionary<string, StringLabel> stringPoolAliases,
                                     StackFrame stackFrame,
                                     string functionEndLabel,
                                     ForgottenRPG.NativeFunctions.NativeFunctionRegistry nativeRegistry = null) {
            _emitter = emitter;
            _functionTypeRepository = functionTypeRepository;
            _userTypeRepository = userTypeRepository;
            _staticVariableRepository = staticVariableRepository;
            _stringPoolAliases = stringPoolAliases;
            _stackFrame = stackFrame;
            _functionEndLabel = functionEndLabel;
            _nativeRegistry = nativeRegistry;
        }

        private StackExpressionEmitter ExpressionEmitter => new StackExpressionEmitter(
            _emitter, _functionTypeRepository, _userTypeRepository, _staticVariableRepository, _stringPoolAliases,
            _stackFrame, _nativeRegistry);

        private TypeIdentifier TypeIdentifier => new TypeIdentifier(
            _functionTypeRepository, _userTypeRepository, _stackFrame, _nativeRegistry);

        public void EmitStatements(List<StatementNode> statements) {
            foreach (var statement in statements) {
                Emit(statement);
            }
        }

        public StackFrame CurrentStackFrame => _stackFrame;

        private void Emit(StatementNode node) {
            switch (node) {
                case DeclarationStatementNode declaration:
                    EmitDeclaration(declaration);
                    return;
                case PrintStatementNode print:
                    EmitPrint(print);
                    return;
                case IfStatementNode ifStatement:
                    EmitIf(ifStatement);
                    return;
                case ForStatementNode forStatement:
                    EmitFor(forStatement);
                    return;
                case ReturnStatementNode returnStatement:
                    EmitReturn(returnStatement);
                    return;
                case ExpressionNode expression:
                    EmitExpressionStatement(expression);
                    return;
                default:
                    throw new NotSupportedException($"Stack emitter does not support statement {node.GetType().Name}");
            }
        }

        private void EmitDeclaration(DeclarationStatementNode node) {
            if (_stackFrame.ExistsLocalAny(node.Identifier)) {
                throw new CompileException($"Attempt to redefine identifier {node.Identifier}", 0, 0);
            }

            SType type = node.TypeNode.GetSType(_userTypeRepository);
            if (type.IsUnknownType()) {
                throw new CompileException($"Unable to discern type from {node.TypeNode}", 0, 0);
            }

            if (node.Static) {
                var initialValue = new uint[type.Length];
                if (node.InitialValue != null) {
                    initialValue = node.InitialValue.Calculate(
                                       new CalcContext(true, _stackFrame, _staticVariableRepository))
                                   ?? throw new CompileException("Failed to evaluate constexpr assignment", 0, 0);
                }

                _stackFrame.AddStaticIdentifier(type, node.Identifier, _staticVariableRepository.CreateNew(initialValue));
                return;
            }

            _stackFrame.AddIdentifier(type, node.Identifier);
            EmitAdjustStackPointer(type.Length);

            if (node.InitialValue != null) {
                if (type.Length == 1) {
                    EmitPushStackPointerOffset(-1);
                    ExpressionEmitter.EmitToVmStack(node.InitialValue);
                    _emitter.EmitText("memwrite_stack");
                } else {
                    ExpressionEmitter.Emit(node.InitialValue);

                    int len = type.Length;
                    for (int i = 0; i < len; i++) {
                        EmitPushStackPointerOffset(-2 * len + i);
                        EmitPushStackPointerOffset(-len + i);
                        _emitter.EmitText("memread_stack");
                        _emitter.EmitText("memwrite_stack");
                    }

                    PopMemoryStack(type);
                }
            } else {
                if (type.Length == 1) {
                    EmitPushStackPointerOffset(-1);
                    _emitter.EmitText("literal", "0");
                    _emitter.EmitText("memwrite_stack");
                } else {
                    for (int i = 0; i < type.Length; i++) {
                        EmitPushStackPointerOffset(-type.Length + i);
                        _emitter.EmitText("literal", "0");
                        _emitter.EmitText("memwrite_stack");
                    }
                }
            }
        }

        private void EmitPrint(PrintStatementNode node) {
            var expressionType = TypeIdentifier.Identify(node.Expression);
            if (expressionType.Length != 1) {
                throw new CompileException("Unable to print multi-word expression", 0, 0);
            }

            ExpressionEmitter.EmitToVmStack(node.Expression);

            if (ReferenceEquals(expressionType, SType.SString)) {
                _emitter.EmitText("print");
            } else if (ReferenceEquals(expressionType, SType.SUInteger)) {
                _emitter.EmitText("printuint");
            } else if (ReferenceEquals(expressionType, SType.SFloat)) {
                _emitter.EmitText("printfloat");
            } else {
                _emitter.EmitText("printint");
            }
        }

        private void EmitIf(IfStatementNode node) {
            string elseLabel = $"if_else_{Guid.NewGuid()}";
            string endLabel = $"if_end_{Guid.NewGuid()}";

            ExpressionEmitter.EmitToVmStack(node.Expression);
            _emitter.EmitText("literal", "0");
            _emitter.EmitText("cmp");
            _emitter.EmitText("jeq", $"${(node.ElseBlock != null ? elseLabel : endLabel)}");

            NewStackFrame();
            EmitStatements(node.IfBlock.Statements);
            PopStackFrame();

            if (node.ElseBlock != null) {
                _emitter.EmitText("jmp", $"${endLabel}");
                _emitter.EmitLabel(elseLabel);
                NewStackFrame();
                EmitStatements(node.ElseBlock.Statements);
                PopStackFrame();
                _emitter.EmitLabel(endLabel);
            } else {
                _emitter.EmitLabel(endLabel);
            }
        }

        private void EmitFor(ForStatementNode node) {
            var startLabel = $"for_start_{Guid.NewGuid()}";
            var endLabel = $"for_end_{Guid.NewGuid()}";

            NewStackFrame();

            if (node.Declaration != null) {
                Emit(node.Declaration);
            }

            _emitter.EmitLabel(startLabel);

            if (node.Condition != null) {
                ExpressionEmitter.EmitToVmStack(node.Condition);
                _emitter.EmitText("literal", "0");
                _emitter.EmitText("cmp");
                _emitter.EmitText("jeq", $"${endLabel}");
            }

            NewStackFrame();
            EmitStatements(node.Block.Statements);
            PopStackFrame();

            if (node.Update != null) {
                if (TypeIdentifier.Identify(node.Update).Length == 1) {
                    ExpressionEmitter.EmitToVmStack(node.Update);
                    _emitter.EmitText("pop", "r2");
                } else {
                    ExpressionEmitter.Emit(node.Update);
                    PopMemoryStack(TypeIdentifier.Identify(node.Update));
                }
            }

            _emitter.EmitText("jmp", $"${startLabel}");
            _emitter.EmitLabel(endLabel);

            PopStackFrame();
        }

        private void EmitReturn(ReturnStatementNode node) {
            var (type, offset, _) = _stackFrame.Lookup(StackFrame.ReturnIdentifier);
            int len = type.Length;

            if (len == 1) {
                EmitPushStackPointerOffset(offset.GetValueOrDefault());
                ExpressionEmitter.EmitToVmStack(node.Expression);
                _emitter.EmitText("memwrite_stack");
            } else {
                ExpressionEmitter.Emit(node.Expression);
                int adjustedOffset = offset.GetValueOrDefault() - len;
                for (int i = 0; i < len; i++) {
                    EmitPushStackPointerOffset(adjustedOffset + i);
                    EmitPushStackPointerOffset(-len + i);
                    _emitter.EmitText("memread_stack");
                    _emitter.EmitText("memwrite_stack");
                }
                PopMemoryStack(type);
            }
            _emitter.EmitText("jmp", $"${_functionEndLabel}");
        }

        private void EmitExpressionStatement(ExpressionNode node) {
            var type = TypeIdentifier.Identify(node);
            if (type.Length == 1) {
                ExpressionEmitter.EmitToVmStack(node);
                _emitter.EmitText("pop", "r2");
                return;
            }

            ExpressionEmitter.Emit(node);
            PopMemoryStack(type);
        }

        private void EmitAdjustStackPointer(int delta) {
            if (delta == 0) return;
            _emitter.EmitText("push", "r1");
            _emitter.EmitText("literal", Math.Abs(delta).ToString());
            _emitter.EmitText(delta > 0 ? "add" : "sub");
            _emitter.EmitText("pop", "r1");
        }

        private void EmitPushStackPointerOffset(int offset) {
            _emitter.EmitText("push", "r1");
            if (offset == 0) return;
            _emitter.EmitText("literal", Math.Abs(offset).ToString());
            _emitter.EmitText(offset > 0 ? "add" : "sub");
        }

        private void PopMemoryStack(SType type) {
            _stackFrame.Popped(type);
            EmitAdjustStackPointer(-type.Length);
        }

        private void NewStackFrame() {
            _stackFrame = new StackFrame(_stackFrame);
        }

        private void PopStackFrame() {
            var (newFrame, len) = _stackFrame.Purge();
            _stackFrame = newFrame!;
            EmitAdjustStackPointer(-len);
        }
    }
}
