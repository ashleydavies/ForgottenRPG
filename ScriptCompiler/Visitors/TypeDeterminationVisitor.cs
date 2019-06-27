using System;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.Types;

namespace ScriptCompiler.Visitors {
    public class TypeDeterminationVisitor : Visitor<SType> {
        private readonly CodeGenVisitor _codeGenVisitor;
        
        public TypeDeterminationVisitor(CodeGenVisitor codeGenVisitor) {
            _codeGenVisitor = codeGenVisitor;
        }

        public override SType Visit(ASTNode node) {
            throw new NotImplementedException(node.GetType().FullName);
        }
        
        public SType VisitDynamic(ASTNode node) {
            return this.Visit(node as dynamic);
        }

        public SType Visit(VariableAccessNode node) {
            var (type, _) = _codeGenVisitor.StackFrame.Lookup(node.Identifier);
            return type;
        }

        public SType Visit(IntegerLiteralNode _) {
            return SType.SInteger;
        }

        public SType Visit(StringLiteralNode _) {
            return SType.SString;
        }

        public SType Visit(BinaryOperatorNode _) {
            var typeL = VisitDynamic(_.Left);
            var typeR = VisitDynamic(_.Right);

            if (ReferenceEquals(typeL, typeR)) {
                return typeL;
            }
            
            throw new Exception();
        }
    }
}
