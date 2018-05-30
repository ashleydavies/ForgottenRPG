using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
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

            while (_cachedToken != null || _lexer.HasMore() && PeekToken() != null) {
                LexToken token = PeekToken();

                if (token is IdentifierToken itok) {
                    //Console.WriteLine(itok.Content);
                    
                    if (itok.Content == "function") {
                        ParseFunctionNode();
                    } else {
                        ParseStatementNode();
                    }
                } else {
                    token.Throw($"Unexpected token {token} - unable to process");
                }
            }

            return new ProgramNode(functions, statements);
        }

        private StatementNode ParseStatementNode() {
            // TODO: Handle StatementNodes correctly instead of just discarding them
            NextToken();
            while (!(PeekToken() is SymbolToken s && s.Symbol == ";")) NextToken();
            Expecting<SymbolToken>(t => t.Symbol == ";");
            return new StatementNode();
        }

        private FunctionNode ParseFunctionNode() {
            Expecting<IdentifierToken>(t => t.Content == "function");
            var typeToken = Expecting<IdentifierToken>();
            var nameToken = Expecting<IdentifierToken>();
            // TODO: Parse argument lists
            Expecting<SymbolToken>(t => t.Symbol == "(");
            Expecting<SymbolToken>(t => t.Symbol == ")");
            Expecting<SymbolToken>(t => t.Symbol == "{");
            ParseStatementNode();
            Expecting<SymbolToken>(t => t.Symbol == "}");
            Expecting<SymbolToken>(t => t.Symbol == ";");
            return new FunctionNode(new ExplicitTypeNode(typeToken.Content));
        }

        private T Expecting<T>(Func<T, bool> predicate = null) where T : class {
            LexToken token = NextToken();

            if (token is T tToken) {
                if (predicate != null && !predicate(tToken)) {
                    token.Throw($"Token '{token}' did not satisfy expected condition");
                }

                return tToken;
            } else {
                token.Throw($"Unexpected token '{token}' (expecting {typeof(T)})");
            }

            return null;
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
            return _cachedToken;
        }
    }
}
