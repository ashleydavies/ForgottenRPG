using System;
using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public interface IConstExpr {
        public uint[]? Calculate(CalcContext ctx);

        public static uint[]? CalcOne(CalcContext ctx, IConstExpr child, Func<uint[], uint[]?> calc) {
            var result = child.Calculate(ctx);
            return result != null ? calc(result) : null;
        }

        public static uint[]? CalcTwo(CalcContext ctx, IConstExpr child, IConstExpr child2, Func<uint[], uint[], uint[]?> calc) {
            var result1 = child.Calculate(ctx);
            var result2 = child2.Calculate(ctx);
            return (result1 != null && result2 != null) ? calc(result1, result2) : null;
        }

        public static uint[]? CalcThree(CalcContext ctx, IConstExpr child, IConstExpr child2, IConstExpr child3, Func<uint[], uint[], uint[], uint[]?> calc) {
            var result1 = child.Calculate(ctx);
            var result2 = child2.Calculate(ctx);
            var result3 = child3.Calculate(ctx);
            return (result1 != null && result2 != null && result3 != null) ? calc(result1, result2, result3) : null;
        }
    }
}
