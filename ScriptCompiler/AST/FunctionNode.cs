using System;
using System.Collections.Generic;

namespace ScriptCompiler.AST {
    public class FunctionNode : ASTNode {
        public readonly string FunctionName;
        public readonly ExplicitTypeNode TypeNode;
        public readonly CodeBlockNode CodeBlock;
        public readonly List<(string type, string name)> ParameterDefinitions;

        public FunctionNode(string functionName, ExplicitTypeNode typeNode, CodeBlockNode codeBlock,
                            List<(string type, string name)> parameterDefinitions) {
            FunctionName = functionName;
            TypeNode = typeNode;
            CodeBlock = codeBlock;
            ParameterDefinitions = parameterDefinitions;
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> { TypeNode, CodeBlock };
        }
    }
}
