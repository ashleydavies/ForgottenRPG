using System;
using System.Collections.Generic;

namespace ScriptCompiler.AST.Statements.Expressions {
    public interface IConstExpr {
        public uint[]? Calculate();

        protected uint[]? CalcOne(IConstExpr child, Func<uint[], uint[]?> calc) {
            var result = child.Calculate();
            return result != null ? calc(result) : null;
        }

        protected uint[]? CalcTwo(IConstExpr child, Func<uint[], uint[], uint[]?> calc) {
            var result1 = child.Calculate();
            var result2 = child.Calculate();
            return (result1 != null && result2 != null) ? calc(result1, result2) : null;
        }

        protected uint[]? CalcThree(IConstExpr child, Func<uint[], uint[], uint[], uint[]?> calc) {
            var result1 = child.Calculate();
            var result2 = child.Calculate();
            var result3 = child.Calculate();
            return (result1 != null && result2 != null && result3 != null) ? calc(result1, result2, result3) : null;
        }
    }
}
