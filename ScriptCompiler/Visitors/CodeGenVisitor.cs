using System;
using System.Collections.Generic;
using ScriptCompiler.AST;

namespace ScriptCompiler.Visitors {
    public class CodeGenVisitor : Visitor<string> {
        public override string Visit(ASTNode node) {
            throw new NotImplementedException();
        }

        public string Visit(ProgramNode node) {
            List<String> programStrings = new StringLiteralCollectorVisitor().Visit(node);
            programStrings.ForEach(x => Console.WriteLine(x));
            return "Hello world";
        }
    }
}