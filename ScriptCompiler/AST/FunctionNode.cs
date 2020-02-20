using System.Collections.Generic;
using System.Linq;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.Types;

namespace ScriptCompiler.AST {
    public class FunctionNode : ASTNode {
        public readonly string Name;
        public readonly ExplicitTypeNode TypeNode;
        public readonly CodeBlockNode CodeBlock;
        public readonly List<(string type, string name)> ParameterDefinitions;

        public Label Label => new Label($"func_{Name}");

        public FunctionNode(string functionName, ExplicitTypeNode typeNode, CodeBlockNode codeBlock,
                            List<(string type, string name)> parameterDefinitions) {
            Name                 = functionName;
            TypeNode             = typeNode;
            CodeBlock            = codeBlock;
            ParameterDefinitions = parameterDefinitions;
        }

        public List<SType> ParameterTypes(UserTypeRepository userTypeRepository) {
            return ParameterDefinitions.Select(p => SType.FromTypeString(p.type, userTypeRepository))
                                       .ToList();
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> {TypeNode, CodeBlock};
        }
    }
}
