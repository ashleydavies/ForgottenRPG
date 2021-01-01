using System;
using System.Collections.Generic;
using ScriptCompiler;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.CodeGeneration;
using ScriptCompiler.Visitors;
using Xunit;

namespace ScriptCompilerTests {
    public class StringLiteralCollectorVisitorTest {
        [Fact]
        public void ExtractsCorrectString() {
            var result = new StringCollectorVisitor().Visit(
                new ProgramNode(
                    new List<ImportNode>(),
                    new List<StructNode>(),
                    new List<FunctionNode> {
                        new FunctionNode("test", new ExplicitTypeNode("void"), new CodeBlockNode(
                            new List<StatementNode> {
                                new PrintStatementNode(new StringLiteralNode("Hello, world!"))
                            }), new List<(string, int, string)> { ("int", 0, "x") }, null)
                    }, new List<StatementNode> {
                        new PrintStatementNode(new StringLiteralNode("Hello, world! Two"))
                    }
                )
            );

            Assert.Collection(result, s => Assert.Equal("Hello, world!", s), s => Assert.Equal("Hello, world! Two", s));
        }
    }
}
