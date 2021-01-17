using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using ScriptCompiler.CodeGeneration;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.Types;

namespace ScriptCompiler.AST {
    public class FunctionNode : ASTNode {
        public readonly string? ObjectName;
        public readonly string Name;
        public readonly ExplicitTypeNode TypeNode;
        public readonly CodeBlockNode CodeBlock;
        public readonly List<(string type, int pointerDepth, string name)> ParameterDefinitions;

        public Label Label => new Label($"func_{ObjectName}!{Name}");

        public FunctionReference Reference(UserTypeRepository utr) =>
            new FunctionReference(ObjectName != null ? SType.FromTypeString(ObjectName, utr) : null, Name);

        public FunctionNode(string functionName, ExplicitTypeNode typeNode, CodeBlockNode codeBlock,
                            List<(string type, int pointerDepth, string name)> parameterDefinitions,
                            string? objectName) {
            Name                 = functionName;
            TypeNode             = typeNode;
            CodeBlock            = codeBlock;
            ParameterDefinitions = parameterDefinitions;
            ObjectName           = objectName;
            if (objectName != null) {
                ParameterDefinitions.Insert(0, (new ReferenceType(objectName).Name, 1, "this"));
            }
        }

        public List<SType> ParameterTypes(UserTypeRepository userTypeRepository) {
            return ParameterDefinitions.Select(p => SType.FromTypeString(p.type, userTypeRepository, p.pointerDepth))
                                       .ToList();
        }

        public override List<ASTNode> Children() {
            return new List<ASTNode> {TypeNode, CodeBlock};
        }
    }
}
