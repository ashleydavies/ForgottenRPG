using System;
using System.Collections.Generic;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.AST.Statements.Expressions.Arithmetic;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Types;

namespace ScriptCompiler.CodeGeneration {
    public sealed class StackExpressionEmitter {
        private readonly StackAssemblyEmitter _emitter;
        private readonly FunctionTypeRepository _functionTypeRepository;
        private readonly UserTypeRepository _userTypeRepository;
        private readonly StaticVariableRepository _staticVariableRepository;
        private readonly Dictionary<string, StringLabel> _stringPoolAliases;
        private readonly ForgottenRPG.NativeFunctions.NativeFunctionRegistry _nativeRegistry;

        private StackFrame _stackFrame;

        public StackExpressionEmitter(StackAssemblyEmitter emitter,
                                      FunctionTypeRepository functionTypeRepository,
                                      UserTypeRepository userTypeRepository,
                                      StaticVariableRepository staticVariableRepository,
                                      Dictionary<string, StringLabel> stringPoolAliases,
                                      StackFrame stackFrame,
                                      ForgottenRPG.NativeFunctions.NativeFunctionRegistry nativeRegistry = null) {
            _emitter = emitter;
            _functionTypeRepository = functionTypeRepository;
            _userTypeRepository = userTypeRepository;
            _staticVariableRepository = staticVariableRepository;
            _stringPoolAliases = stringPoolAliases;
            _stackFrame = stackFrame;
            _nativeRegistry = nativeRegistry;
        }

        private TypeIdentifier TypeIdentifier => new TypeIdentifier(
            _functionTypeRepository, _userTypeRepository, _stackFrame, _nativeRegistry);

        private AddressabilityChecker AddressabilityChecker => new AddressabilityChecker(
            _functionTypeRepository, _userTypeRepository, _stackFrame);

        private StackLValueEmitter LValueEmitter => new StackLValueEmitter(
            _emitter, _functionTypeRepository, _userTypeRepository, _stackFrame, EmitToVmStack);

        public void Emit(ExpressionNode node) {
            switch (node) {
                case VariableAccessNode variableAccess:
                    EmitVariableAccess(variableAccess);
                    return;
                case FunctionCallNode functionCall:
                    EmitFunctionCall(functionCall);
                    return;
                case IntegerLiteralNode intNode:
                    EmitLiteral(intNode.Value);
                    return;
                case FloatLiteralNode floatNode:
                    EmitLiteral(unchecked((uint)BitConverter.SingleToInt32Bits(floatNode.Value)));
                    return;
                case StringLiteralNode stringNode:
                    EmitStringLiteral(stringNode);
                    return;
                case SizeOfNode sizeOf:
                    EmitSizeOf(sizeOf);
                    return;
                case AssignmentNode assignment:
                    EmitAssignment(assignment);
                    return;
                case StructAccessNode structAccess:
                    EmitStructAccess(structAccess);
                    return;
                case AddressOfNode addressOf:
                    EmitAddressOf(addressOf);
                    return;
                case DereferenceNode dereference:
                    EmitDereference(dereference);
                    return;
                case EqualityOperatorNode eq:
                    EmitComparison(eq, "jeq");
                    return;
                case InequalityOperatorNode neq:
                    EmitComparison(neq, "jneq");
                    return;
                case GreaterThanOperatorNode gt:
                    EmitComparison(gt, "jgt");
                    return;
                case LessThanOperatorNode lt:
                    EmitComparison(lt, "jlt");
                    return;
                case GreaterThanEqualOperatorNode gte:
                    EmitComparison(gte, "jgte");
                    return;
                case LessThanEqualOperatorNode lte:
                    EmitComparison(lte, "jlte");
                    return;
                case AdditionNode add:
                    EmitArithmetic(add, "add", "addf");
                    return;
                case SubtractionNode sub:
                    EmitArithmetic(sub, "sub", "subf");
                    return;
                case MultiplicationNode mul:
                    EmitArithmetic(mul, "mul", "mulf");
                    return;
                case DivisionNode div:
                    EmitArithmetic(div, "div", "divf");
                    return;
            }

            throw new NotSupportedException($"Stack emitter does not support expression {node.GetType().Name}");
        }

        public void EmitToVmStack(ExpressionNode node) {
            var type = TypeIdentifier.Identify(node);
            if (type.Length != 1) {
                throw new CompileException($"Expression {node.GetType().Name} is not single-word", 0, 0);
            }

            switch (node) {
                case IntegerLiteralNode intNode:
                    _emitter.EmitText("literal", intNode.Value.ToString());
                    return;
                case FloatLiteralNode floatNode:
                    _emitter.EmitText("literal",
                                      unchecked((uint)BitConverter.SingleToInt32Bits(floatNode.Value)).ToString());
                    return;
                case StringLiteralNode stringNode:
                    if (!_stringPoolAliases.TryGetValue(stringNode.String, out var stringLabel)) {
                        throw new CompileException("Unknown string literal", 0, 0);
                    }

                    _emitter.EmitText("push", stringLabel.ToString());
                    return;
                case VariableAccessNode variableAccess:
                    LValueEmitter.EmitAddress(variableAccess);
                    _emitter.EmitText("memread_stack");
                    return;
                case StructAccessNode structAccess:
                    EmitStructAccessToVmStack(structAccess);
                    return;
                case FunctionCallNode functionCall:
                    EmitFunctionCall(functionCall, true);
                    return;
                case AssignmentNode assignment:
                    EmitAssignmentToVmStack(assignment);
                    return;
                case AddressOfNode addressOf:
                    LValueEmitter.EmitAddress(addressOf.Expression);
                    return;
                case DereferenceNode dereference:
                    EmitDereferenceToVmStack(dereference);
                    return;
                case EqualityOperatorNode eq:
                    EmitComparisonToVmStack(eq, "jeq");
                    return;
                case InequalityOperatorNode neq:
                    EmitComparisonToVmStack(neq, "jneq");
                    return;
                case GreaterThanOperatorNode gt:
                    EmitComparisonToVmStack(gt, "jgt");
                    return;
                case LessThanOperatorNode lt:
                    EmitComparisonToVmStack(lt, "jlt");
                    return;
                case GreaterThanEqualOperatorNode gte:
                    EmitComparisonToVmStack(gte, "jgte");
                    return;
                case LessThanEqualOperatorNode lte:
                    EmitComparisonToVmStack(lte, "jlte");
                    return;
                case AdditionNode add:
                    EmitArithmeticToVmStack(add, "add", "addf");
                    return;
                case SubtractionNode sub:
                    EmitArithmeticToVmStack(sub, "sub", "subf");
                    return;
                case MultiplicationNode mul:
                    EmitArithmeticToVmStack(mul, "mul", "mulf");
                    return;
                case DivisionNode div:
                    EmitArithmeticToVmStack(div, "div", "divf");
                    return;
                case SizeOfNode sizeOf:
                    EmitSizeOfToVmStack(sizeOf);
                    return;
            }

            throw new NotSupportedException($"Stack emitter does not support expression {node.GetType().Name}");
        }

        private void EmitVariableAccess(VariableAccessNode node) {
            var (type, offset, guid) = _stackFrame.Lookup(node.Identifier);
            if (Equals(type, SType.SNoType)) {
                throw new CompileException($"Unknown variable {node.Identifier}", 0, 0);
            }

            if (type.Length > 1) {
                if (offset != null) {
                    EmitPushStackPointerOffset(offset.Value);
                } else {
                    EmitSourceAddress(offset, guid, 0);
                }
                _emitter.EmitText("pop", "r2");

                for (int i = 0; i < type.Length; i++) {
                    EmitPushStackPointer();
                    _emitter.EmitText("push", "r2");
                    if (i != 0) {
                        EmitLiteralWithoutWrite(i);
                        _emitter.EmitText("add");
                    }
                    _emitter.EmitText("memread_stack");
                    _emitter.EmitText("memwrite_stack");
                    PushMemoryStack(SType.SInteger);
                }
                return;
            }

            for (int i = 0; i < type.Length; i++) {
                EmitPushStackPointer();
                if (offset != null) {
                    // Adjust for stack growth as we copy multi-word values.
                    EmitPushStackPointerOffset(offset.Value);
                } else {
                    EmitSourceAddress(offset, guid, i);
                }
                _emitter.EmitText("memread_stack");
                _emitter.EmitText("memwrite_stack");
                PushMemoryStack(SType.SInteger);
            }
        }

        private void EmitFunctionCall(FunctionCallNode node) {
            EmitFunctionCall(node, false);
        }

        private void EmitFunctionCall(FunctionCallNode node, bool returnToVmStack) {
            if (_nativeRegistry != null && node.Function is VariableAccessNode vn &&
                _nativeRegistry.HasFunction(vn.Identifier)) {
                EmitNativeFunctionCall(node, vn.Identifier, returnToVmStack);
                return;
            }

            var returnType = _functionTypeRepository.ReturnType(node.FunctionRef(TypeIdentifier));

            if (!ReferenceEquals(returnType, SType.SVoid)) {
                EmitAdjustStackPointer(returnType.Length);
                _stackFrame.Pushed(returnType);
            }

            if (node.Function is StructAccessNode structCall) {
                EmitPushStackPointer();
                LValueEmitter.EmitAddress(structCall.Left);
                _emitter.EmitText("memwrite_stack");
                PushMemoryStack(SType.SGenericPtr);
            }

            foreach (var arg in node.Args) {
                Emit(arg);
            }

            var returnLabel = $"return_{Guid.NewGuid()}";
            EmitPushStackPointer();
            _emitter.EmitText("push", $"${returnLabel}");
            _emitter.EmitText("memwrite_stack");
            EmitAdjustStackPointer(1);

            var functionRef = node.FunctionRef(TypeIdentifier);
            if (_functionTypeRepository.IsExtern(functionRef)) {
                _emitter.EmitText("jmp_extern", _functionTypeRepository.SymbolHash(functionRef).ToString());
            } else {
                _emitter.EmitText("jmp", $"${node.AsmLabel(TypeIdentifier)}");
            }

            _emitter.EmitLabel(returnLabel);

            for (int i = node.Args.Count - 1; i >= 0; i--) {
                PopMemoryStack(TypeIdentifier.Identify(node.Args[i]));
            }

            if (node.Function is StructAccessNode) {
                PopMemoryStack(SType.SGenericPtr);
            }

            if (returnToVmStack && !ReferenceEquals(returnType, SType.SVoid) && returnType.Length == 1) {
                EmitPushStackPointerOffset(-returnType.Length);
                _emitter.EmitText("memread_stack");
                PopMemoryStack(returnType);
            }
        }

        private void EmitNativeFunctionCall(FunctionCallNode node, string functionName, bool returnToVmStack) {
            if (!_nativeRegistry.HasFunction(functionName)) {
                throw new CompileException($"Native function {functionName} not found", 0, 0);
            }

            var funcInfo = _nativeRegistry.GetByName(functionName);
            var returnType = SType.FromType(funcInfo.Method.ReturnType);

            foreach (var arg in node.Args) {
                var argType = TypeIdentifier.Identify(arg);
                if (argType.Length != 1) {
                    throw new CompileException($"Native function {functionName} does not support multi-word args", 0, 0);
                }

                EmitToVmStack(arg);
            }

            var symbolHash = _nativeRegistry.GetSymbolHash(functionName);
            if (symbolHash == null) {
                throw new CompileException($"Native function symbol hash not found for {functionName}", 0, 0);
            }

            _emitter.EmitText("call_native_extern", symbolHash.Value.ToString());

            bool requiresDynamicMarshalling =
                funcInfo.Method.ReturnType == typeof(string) ||
                funcInfo.Method.ReturnType.IsArray ||
                (funcInfo.Method.ReturnType.IsGenericType &&
                 funcInfo.Method.ReturnType.GetGenericTypeDefinition() == typeof(List<>));

            if (requiresDynamicMarshalling) {
                EmitDynamicMarshalling();
            }

            if (!ReferenceEquals(returnType, SType.SVoid)) {
                if (returnToVmStack) {
                    return;
                }

                EmitStoreVmTopToMemory();
                PushMemoryStack(returnType);
            }
        }

        private void EmitDynamicMarshalling() {
            EmitWriteLiteralToMemory(0);

            EmitPopToRegister("r2");
            EmitPushStackPointer();
            _emitter.EmitText("push", "r2");
            _emitter.EmitText("memwrite_stack");
            PushMemoryStack(SType.SInteger);

            var mallocReturn = $"malloc_return_{Guid.NewGuid()}";
            EmitPushStackPointer();
            _emitter.EmitText("push", $"${mallocReturn}");
            _emitter.EmitText("memwrite_stack");
            PushMemoryStack(SType.SInteger);

            var mallocRef = new FunctionReference(null, "malloc");
            if (_functionTypeRepository.Has(mallocRef) && _functionTypeRepository.IsExtern(mallocRef)) {
                _emitter.EmitText("jmp_extern", _functionTypeRepository.SymbolHash(mallocRef).ToString());
            } else if (_functionTypeRepository.Has(mallocRef)) {
                _emitter.EmitText("jmp", "$func_!malloc");
            } else {
                throw new CompileException("malloc is required for native reference returns", 0, 0);
            }

            _emitter.EmitLabel(mallocReturn);

            EmitPushStackPointerOffset(-2);
            _emitter.EmitText("memread_stack");
            EmitAdjustStackPointer(-2);
            _stackFrame.Popped(SType.SInteger);
            _stackFrame.Popped(SType.SInteger);
            _stackFrame.Popped(SType.SInteger);

            var commitFunc = _nativeRegistry.GetByName("_internal_commit_data");
            if (commitFunc == null) {
                throw new CompileException("Internal commit function not found", 0, 0);
            }

            _emitter.EmitText("callnative", commitFunc.Id.ToString());
        }

        private void EmitLiteral(uint value) {
            EmitPushStackPointer();
            _emitter.EmitText("literal", value.ToString());
            _emitter.EmitText("memwrite_stack");
            PushMemoryStack(SType.SInteger);
        }

        private void EmitStringLiteral(StringLiteralNode node) {
            if (!_stringPoolAliases.TryGetValue(node.String, out var stringLabel)) {
                throw new CompileException("Unknown string literal", 0, 0);
            }

            EmitPushStackPointer();
            _emitter.EmitText("push", stringLabel.ToString());
            _emitter.EmitText("memwrite_stack");
            PushMemoryStack(SType.SInteger);
        }

        private void EmitSizeOf(SizeOfNode node) {
            int result;
            if (node.Arg is VariableAccessNode n && _userTypeRepository.ExistsType(n.Identifier)) {
                result = _userTypeRepository[n.Identifier].Length;
            } else {
                result = TypeIdentifier.Identify(node.Arg).Length;
            }

            EmitLiteral((uint)result);
        }

        private void EmitSizeOfToVmStack(SizeOfNode node) {
            int result;
            if (node.Arg is VariableAccessNode n && _userTypeRepository.ExistsType(n.Identifier)) {
                result = _userTypeRepository[n.Identifier].Length;
            } else {
                result = TypeIdentifier.Identify(node.Arg).Length;
            }

            _emitter.EmitText("literal", result.ToString());
        }

        private void EmitAssignment(AssignmentNode node) {
            var valueType = TypeIdentifier.Identify(node);
            Emit(node.Value);

            for (int i = 0; i < valueType.Length; i++) {
                LValueEmitter.EmitAddress(node.Destination, i);
                EmitPushStackPointerOffset(-valueType.Length + i);
                _emitter.EmitText("memread_stack");
                _emitter.EmitText("memwrite_stack");
            }
        }

        private void EmitAssignmentToVmStack(AssignmentNode node) {
            var valueType = TypeIdentifier.Identify(node);
            if (valueType.Length != 1) {
                throw new CompileException("Assignment requires single-word type for VM stack mode", 0, 0);
            }

            LValueEmitter.EmitAddress(node.Destination);
            EmitToVmStack(node.Value);
            _emitter.EmitText("memwrite_stack");
            LValueEmitter.EmitAddress(node.Destination);
            _emitter.EmitText("memread_stack");
        }

        private void EmitStructAccess(StructAccessNode node) {
            Emit(node.Left);
            var structType = TypeIdentifier.Identify(node.Left);
            if (structType is not UserType userStructType) {
                throw new CompileException($"Unable to access field {node.Field} of non-struct type {structType}", 0, 0);
            }

            PopMemoryStack(userStructType);

            var fieldOffset = userStructType.OffsetOfField(node.Field);
            var fieldType = userStructType.TypeOfField(node.Field);
            EmitPushStackPointer();
            _emitter.EmitText("pop", "r2");
            for (int i = 0; i < fieldType.Length; i++) {
                EmitPushStackPointer();
                _emitter.EmitText("push", "r2");
                int offset = fieldOffset + i;
                if (offset != 0) {
                    EmitLiteralWithoutWrite(offset);
                    _emitter.EmitText("add");
                }
                _emitter.EmitText("memread_stack");
                _emitter.EmitText("memwrite_stack");
                PushMemoryStack(SType.SInteger);
            }
        }

        private void EmitStructAccessToVmStack(StructAccessNode node) {
            var structType = TypeIdentifier.Identify(node.Left);
            if (structType is not UserType userStructType) {
                throw new CompileException($"Unable to access field {node.Field} of non-struct type {structType}", 0, 0);
            }

            var fieldType = userStructType.TypeOfField(node.Field);
            if (fieldType.Length != 1) {
                throw new CompileException("Struct field is not single-word", 0, 0);
            }

            LValueEmitter.EmitAddress(node);
            _emitter.EmitText("memread_stack");
        }

        private void EmitAddressOf(AddressOfNode node) {
            if (!AddressabilityChecker.Check(node.Expression)) {
                throw new CompileException("Attempt to take the address of an unaddressable expression", 0, 0);
            }

            EmitPushStackPointer();
            LValueEmitter.EmitAddress(node.Expression);
            _emitter.EmitText("memwrite_stack");
            PushMemoryStack(SType.SGenericPtr);
        }

        private void EmitDereference(DereferenceNode node) {
            var pointerType = TypeIdentifier.Identify(node.Expression);
            if (pointerType is not ReferenceType refType) {
                throw new CompileException($"Unable to dereference expression of type {pointerType}", 0, 0);
            }

            Emit(node.Expression);
            EmitPushStackPointerOffset(-1);
            _emitter.EmitText("memread_stack");
            _emitter.EmitText("pop", "r2");
            PopMemoryStack(SType.SGenericPtr);

            for (int i = 0; i < refType.ContainedType.Length; i++) {
                EmitPushStackPointer();
                _emitter.EmitText("push", "r2");
                EmitLiteralWithoutWrite(i);
                _emitter.EmitText("add");
                _emitter.EmitText("memread_stack");
                _emitter.EmitText("memwrite_stack");
                PushMemoryStack(SType.SInteger);
            }
        }

        private void EmitDereferenceToVmStack(DereferenceNode node) {
            var pointerType = TypeIdentifier.Identify(node.Expression);
            if (pointerType is not ReferenceType refType) {
                throw new CompileException($"Unable to dereference expression of type {pointerType}", 0, 0);
            }

            if (refType.ContainedType.Length != 1) {
                throw new CompileException("Dereference requires single-word type for VM stack mode", 0, 0);
            }

            EmitToVmStack(node.Expression);
            _emitter.EmitText("memread_stack");
        }

        private void EmitComparison(BinaryOperatorNode node, string opcode) {
            if (TypeIdentifier.Identify(node.Left).Length != 1 || TypeIdentifier.Identify(node.Right).Length != 1) {
                throw new CompileException("Comparison operators require single-word operands", 0, 0);
            }

            Emit(node.Left);
            Emit(node.Right);

            EmitPushStackPointerOffset(-2);
            EmitPushStackPointerOffset(-1);
            _emitter.EmitText("memread_stack");
            EmitPushStackPointerOffset(-2);
            _emitter.EmitText("memread_stack");
            _emitter.EmitText("cmp");

            var trueLabel = $"cmp_true_{Guid.NewGuid()}";
            var endLabel = $"cmp_end_{Guid.NewGuid()}";

            _emitter.EmitText(opcode, $"${trueLabel}");
            EmitLiteralWithoutWrite(0);
            _emitter.EmitText("jmp", $"${endLabel}");
            _emitter.EmitLabel(trueLabel);
            EmitLiteralWithoutWrite(1);
            _emitter.EmitLabel(endLabel);
            _emitter.EmitText("memwrite_stack");
            PopMemoryStack(SType.SInteger);
        }

        private void EmitComparisonToVmStack(BinaryOperatorNode node, string opcode) {
            EmitToVmStack(node.Right);
            EmitToVmStack(node.Left);
            _emitter.EmitText("cmp");

            var trueLabel = $"cmp_true_{Guid.NewGuid()}";
            var endLabel = $"cmp_end_{Guid.NewGuid()}";

            _emitter.EmitText(opcode, $"${trueLabel}");
            _emitter.EmitText("literal", "0");
            _emitter.EmitText("jmp", $"${endLabel}");
            _emitter.EmitLabel(trueLabel);
            _emitter.EmitText("literal", "1");
            _emitter.EmitLabel(endLabel);
        }

        private void EmitArithmetic(BinaryOperatorNode node, string intOp, string floatOp) {
            if (TypeIdentifier.Identify(node.Left).Length != 1 || TypeIdentifier.Identify(node.Right).Length != 1) {
                throw new CompileException("Arithmetic operators require single-word operands", 0, 0);
            }

            Emit(node.Left);
            Emit(node.Right);

            EmitPushStackPointerOffset(-2);
            EmitPushStackPointerOffset(-2);
            _emitter.EmitText("memread_stack");
            EmitPushStackPointerOffset(-1);
            _emitter.EmitText("memread_stack");

            var type = TypeIdentifier.Identify(node.Left);
            _emitter.EmitText(ReferenceEquals(type, SType.SFloat) ? floatOp : intOp);
            _emitter.EmitText("memwrite_stack");
            PopMemoryStack(SType.SInteger);
        }

        private void EmitArithmeticToVmStack(BinaryOperatorNode node, string intOp, string floatOp) {
            EmitToVmStack(node.Left);
            EmitToVmStack(node.Right);
            var type = TypeIdentifier.Identify(node.Left);
            _emitter.EmitText(ReferenceEquals(type, SType.SFloat) ? floatOp : intOp);
        }

        private void EmitSourceAddress(int? offset, Guid? guid, int wordOffset) {
            if (offset != null) {
                EmitPushStackPointerOffset(offset.Value + wordOffset);
                return;
            }

            var label = new StaticLabel(guid.GetValueOrDefault());
            _emitter.EmitText("push", label.ToString());
            if (wordOffset != 0) {
                EmitLiteralWithoutWrite(wordOffset);
                _emitter.EmitText("add");
            }
        }

        private void EmitWriteLiteralToMemory(uint value) {
            EmitPushStackPointer();
            _emitter.EmitText("literal", value.ToString());
            _emitter.EmitText("memwrite_stack");
            PushMemoryStack(SType.SInteger);
        }

        private void EmitStoreVmTopToMemory() {
            _emitter.EmitText("pop", "r2");
            EmitPushStackPointer();
            _emitter.EmitText("push", "r2");
            _emitter.EmitText("memwrite_stack");
        }

        private void EmitPushStackPointer() {
            _emitter.EmitText("push", "r1");
        }

        private void EmitPushStackPointerOffset(int offset) {
            EmitPushStackPointer();
            if (offset == 0) {
                return;
            }

            if (offset > 0) {
                EmitLiteralWithoutWrite(offset);
                _emitter.EmitText("add");
            } else {
                EmitLiteralWithoutWrite(-offset);
                _emitter.EmitText("sub");
            }
        }

        private void EmitLiteralWithoutWrite(int value) {
            _emitter.EmitText("literal", value.ToString());
        }

        private void EmitAdjustStackPointer(int delta) {
            if (delta == 0) return;
            EmitPushStackPointer();
            EmitLiteralWithoutWrite(Math.Abs(delta));
            _emitter.EmitText(delta > 0 ? "add" : "sub");
            _emitter.EmitText("pop", "r1");
        }

        private void EmitPopToRegister(string register) {
            _emitter.EmitText("pop", register);
        }

        private void PushMemoryStack(SType type) {
            _stackFrame.Pushed(type);
            EmitAdjustStackPointer(type.Length);
        }

        private void PopMemoryStack(SType type) {
            _stackFrame.Popped(type);
            EmitAdjustStackPointer(-type.Length);
        }
    }
}
