using System;
using System.Collections.Generic;
using ScriptCompiler.AST;

namespace ScriptCompiler.Visitors {
    // TODO: Abstraction over string
    public class ExpressionGenVisitor : Visitor<(List<string>, string)> {
        private readonly CodeGenVisitor _codeGenVisitor;

        public ExpressionGenVisitor(CodeGenVisitor codeGenVisitor) {
            _codeGenVisitor = codeGenVisitor;
        }

        public (List<string>, string) VisitDynamic(ASTNode node) {
            (List<string>, string) res = Visit(node as dynamic);
            return (res.Item1, res.Item2);
        }

        public override (List<string>, string) Visit(ASTNode node) {
            throw new System.NotImplementedException();
        }

        public (List<string>, string) Visit(StringLiteralNode node) {
            // TODO: node.Throw()
            if (!_codeGenVisitor.StringAliases.ContainsKey(node.String)) {
                throw new ArgumentException();
            }

            return (new List<string>(), _codeGenVisitor.StringAliases[node.String]);
        }
    }
}