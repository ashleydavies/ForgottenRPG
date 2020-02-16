using System;
using System.Collections.Generic;
using System.IO;

namespace ScriptCompiler.AST {
    public class ImportNode : ASTNode {
        public readonly string FileName;

        public ImportNode(string fileName) {
            FileName = Path.GetFullPath(fileName);
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode>();
        }

        private bool Equals(ImportNode other) {
            return FileName == other.FileName;
        }

        public override bool Equals(object? obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ImportNode) obj);
        }

        public override int GetHashCode() {
            return FileName.GetHashCode();
        }
    }
}
