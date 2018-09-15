﻿using System;
using System.Collections.Generic;
using ScriptCompiler;
using ScriptCompiler.AST;
using ScriptCompiler.Visitors;
using Xunit;

namespace ScriptCompilerTest {
    public class StringLiteralCollectorVisitorTest {
        [Fact]
        public void ExtractsCorrectString() {
            var result = new StringLiteralCollectorVisitor().Visit(
                new ProgramNode(
                    new List<FunctionNode> {
                        new FunctionNode(new ExplicitTypeNode("void"), new CodeBlockNode(
                            new List<StatementNode> {
                                new PrintStatementNode(new StringLiteralNode("Hello, world!"))
                            }))
                    }, new List<StatementNode> {
                        new PrintStatementNode(new StringLiteralNode("Hello, world! Two"))
                    }
                )
            );

            Assert.Collection(result, s => Assert.Equal(s, "Hello, world!"), s => Assert.Equal(s, "Hello, world! Two"));
        }
    }
}