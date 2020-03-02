using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Types;
using ScriptCompiler.Visitors;

namespace ScriptCompiler.CodeGeneration {
    public class AddressabilityChecker : Visitor<bool> {
        private readonly FunctionTypeRepository _functionTypeRepository;
        private readonly UserTypeRepository _userTypeRepository;
        private readonly StackFrame _stackFrame;

        public AddressabilityChecker(FunctionTypeRepository functionTypeRepository,
                                     UserTypeRepository userTypeRepository, StackFrame stackFrame) {
            _functionTypeRepository = functionTypeRepository;
            _userTypeRepository     = userTypeRepository;
            _stackFrame             = stackFrame;
        }

        public bool Check(ASTNode node) {
            return this.Visit(node as dynamic);
        }

        public override bool Visit(ASTNode node) {
            return false;
        }

        public bool Visit(VariableAccessNode node) {
            return true;
        }

        // x.y    is addressable
        // f(a).y is not addressable
        public bool Visit(StructAccessNode node) {
            return Check(node.Left);
        }

        public bool Visit(DereferenceNode node) {
            return true;
        }

        public bool Visit(StringLiteralNode node) {
            return true;
        }
    }
}
