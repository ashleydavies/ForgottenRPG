using System;
using ScriptCompiler.AST;
using ScriptCompiler.Types;

namespace ScriptCompiler.Visitors {
    public class TypeDeterminationVisitor : Visitor<SType> {
        public override SType Visit(ASTNode node) {
            throw new NotImplementedException(node.GetType().FullName);
        }

        public SType VisitDynamic(ASTNode node) {
            return this.Visit(node as dynamic);
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

            if (typeL == typeR) {
                return typeL;
            }
            
            throw new Exception();
        }
    }
}
