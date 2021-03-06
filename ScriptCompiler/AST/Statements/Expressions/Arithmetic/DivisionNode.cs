﻿namespace ScriptCompiler.AST.Statements.Expressions.Arithmetic {
    public class DivisionNode : BinaryOperatorNode {
        public DivisionNode(ExpressionNode left, ExpressionNode right) : base(left, right) {
            
        }

        public override uint[]? Calculate(CalcContext ctx) {
            return IConstExpr.CalcTwo(ctx, Left, Right, (uints, uints1) => {
                return new[]{ uints[0] / uints1[0] };
            });
        }
    }
}
