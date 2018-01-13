using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using ScriptCompiler.AST;

namespace ScriptCompiler {
    public class Parser {
        private readonly string _contents;
        private Lexer _lexer;
        private LexToken _cachedToken;
        
        public Parser(string contents) {
            _contents = contents;
        }

        public void Parse() {
            _lexer = new Lexer(_contents);
            ParseProgram();
        }

        private ASTNode ParseProgram() {
            List<FunctionNode> functions = new List<FunctionNode>();
            List<StatementNode> statements = new List<StatementNode>();

            while (_lexer.HasMore() || _cachedToken != null) {
                LexToken token = PeekToken();

                if (token is IdentifierToken itok) {
                    Console.WriteLine(itok.Content);
                    if (itok.Content == "function") {
                        ParseFunctionNode();
                    } else {
                        ParseStatementNode();
                    }
                }
            }

            return new ProgramNode(functions, statements);
        }

        private StatementNode ParseStatementNode() {
            return new StatementNode();
        }

        private FunctionNode ParseFunctionNode() {
            return new FunctionNode();
        }

        private LexToken NextToken() {
            if (_cachedToken == null) return _lexer.NextToken();

            LexToken cached = _cachedToken;
            _cachedToken    = null;
            return cached;

        }

        private LexToken PeekToken() {
            if (_cachedToken != null) return _cachedToken;

            _cachedToken = _lexer.NextToken();
            return NextToken();
        }
    }
}
