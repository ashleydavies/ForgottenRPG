using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using ScriptCompiler.AST;

namespace ScriptCompiler.Visitors {
    public class CodeGenVisitor : Visitor<string> {
        public override string Visit(ASTNode node) {
            throw new NotImplementedException();
        }

        public string Visit(ProgramNode node) {
            List<String> programStrings = new StringLiteralCollectorVisitor().Visit(node);
            
            StringBuilder programBuilder = new StringBuilder();

            programBuilder.AppendLine(".data");
            for (int i = 0; i < programStrings.Count; i++) {
                programBuilder.AppendLine($"STRING str_{i} {programStrings[i]}");
            }
            programBuilder.AppendLine(".text");

            return programBuilder.ToString();
        }
    }
}