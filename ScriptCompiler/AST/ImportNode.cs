using System;
using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class ImportNode : ASTNode {
        public readonly string FileName;

        public ImportNode(string fileName) {
            FileName = fileName;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode>();
        }
    }
}
