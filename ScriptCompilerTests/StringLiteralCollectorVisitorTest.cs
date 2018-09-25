using System;
using System.Collections.Generic;
using ScriptCompiler;
using ScriptCompiler.AST;
using ScriptCompiler.Visitors;
using Xunit;

namespace ScriptCompilerTests {
    public class StringLiteralCollectorVisitorTest {
        [Fact]
        public void ExtractsCorrectString() {
            var result = new StringLiteralCollectorVisitor().Visit(
                new ProgramNode(
                    new List<FunctionNode> {
                        new FunctionNode("test", new ExplicitTypeNode("void"), new CodeBlockNode(
                            new List<StatementNode> {
                                new PrintStatementNode(new StringLiteralNode("Hello, world!"))
                            }))
                    }, new List<StatementNode> {
                        new PrintStatementNode(new StringLiteralNode("Hello, world! Two"))
                    }
                )
            );

            Assert.Collection(result, s => Assert.Equal("Hello, world!", s), s => Assert.Equal("Hello, world! Two", s));
        }
    }
}
