using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using ScriptCompiler.AST;
using ScriptCompiler.Visitors;

namespace ScriptCompiler {
    public class Parser {
        private readonly string _contents;
        private Lexer _lexer;
        private LexToken _cachedToken;

        public Parser(string contents) {
            _contents = contents;
        }

        public string Parse() {
            _lexer = new Lexer(_contents);
            dynamic ast = ParseProgram();
            return new CodeGenVisitor().Visit(ast);
        }

        private ProgramNode ParseProgram() {
            var functions = new List<FunctionNode>();
            var statements = new List<StatementNode>();

            while (_cachedToken != null || _lexer.HasMore() && PeekToken() != null) {
                var token = PeekToken();

                switch (token) {
                    case IdentifierToken itok when itok.Content == "function":
                        functions.Add(ParseFunctionNode());
                        break;
                    default:
                        statements.Add(ParseStatementNode());
                        break;
                }
            }

            return new ProgramNode(functions, statements);
        }

        private StatementNode ParseStatementNode() {
            if (PeekMatch<IdentifierToken>(t => t.Content == "print")) {
                var node = ParsePrintStatementNode();
                Expecting<SymbolToken>(t => t.Symbol == ";");
                return node;
            }
            
            // TODO: Add other types of statement e.g. identifier = value
            if (PeekToken() is IdentifierToken) {
                var identifierToken = Expecting<IdentifierToken>();
                
                // Parse function call statements
                if (PeekToken() is SymbolToken s && s.Symbol == "(") {
                    Expecting<SymbolToken>(t => t.Symbol == "(");
                    Expecting<SymbolToken>(t => t.Symbol == ")");
                    
                    return new FunctionCallNode(identifierToken.Content);
                }
            }

            // TODO: Handle StatementNodes correctly instead of just discarding them
            while (!(PeekToken() is SymbolToken s && s.Symbol == ";")) {
                Console.WriteLine(PeekToken());
                NextToken();
            }

            Expecting<SymbolToken>(t => t.Symbol == ";");
            throw new NotImplementedException();
        }

        private PrintStatementNode ParsePrintStatementNode() {
            Expecting<IdentifierToken>(t => t.Content == "print");
            string result = Expecting<StringToken>().Content;
            return new PrintStatementNode(new StringLiteralNode(result));
        }

        private FunctionNode ParseFunctionNode() {
            Expecting<IdentifierToken>(t => t.Content == "function");
            var typeToken = Expecting<IdentifierToken>();
            var nameToken = Expecting<IdentifierToken>();
            // TODO: Parse argument lists
            Expecting<SymbolToken>(t => t.Symbol == "(");
            Expecting<SymbolToken>(t => t.Symbol == ")");
            var block = ParseCodeBlock();
            return new FunctionNode(nameToken.Content, new ExplicitTypeNode(typeToken.Content), block);
        }

        private CodeBlockNode ParseCodeBlock() {
            var statements = new List<StatementNode>();
            Expecting<SymbolToken>(t => t.Symbol == "{");

            while (!(PeekToken() is SymbolToken st && st.Symbol == "}")) {
                statements.Add(ParseStatementNode());
            }

            Expecting<SymbolToken>(t => t.Symbol == "}");

            return new CodeBlockNode(statements);
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

        private bool PeekMatch<T>(Func<T, bool> predicate) where T : class {
            LexToken token = PeekToken();

            if (token is T tToken) {
                if (predicate(tToken)) {
                    return true;
                }
            }

            return false;
        }

        private LexToken NextToken() {
            if (_cachedToken == null) return _lexer.NextToken();

            LexToken cached = _cachedToken;
            _cachedToken = null;
            return cached;
        }

        private LexToken PeekToken() {
            if (_cachedToken != null) return _cachedToken;

            _cachedToken = _lexer.NextToken();
            return _cachedToken;
        }
    }
}
