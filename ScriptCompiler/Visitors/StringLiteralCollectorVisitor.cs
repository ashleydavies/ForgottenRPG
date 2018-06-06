using System;
using System.Collections.Generic;
using ScriptCompiler.AST;

namespace ScriptCompiler.Visitors {
    public class StringLiteralCollectorVisitor : EverythingVisitor<List<string>> {
        protected override List<string> Base() {
            return new List<string>();
        }

        protected override List<string> Aggregate(List<string> a, List<string> b) {
            var result = new List<string>(a);
            result.AddRange(b);
            return result;
        }

        public List<string> Visit(StringLiteralNode stringLiteralNode) {
            return new List<string> { stringLiteralNode.String };
        }
    }
}