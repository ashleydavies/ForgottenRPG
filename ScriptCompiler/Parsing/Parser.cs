using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Security;
using ScriptCompiler.AST;
using ScriptCompiler.Visitors;

namespace ScriptCompiler.Parsing {
    public class Parser {
        private class InfixParseRule {
            public readonly Predicate<LexToken> Predicate;
            public readonly Func<LexToken, ExpressionNode, ExpressionNode> Rule;
            public readonly Precedence Precedence;
            
            public InfixParseRule(Predicate<LexToken> predicate, Func<LexToken, ExpressionNode, ExpressionNode> rule, Precedence precedence) {
                Predicate = predicate;
                Rule = rule;
                Precedence = precedence;
            }
        }

        private class PrefixParseRule {
            public readonly Predicate<LexToken> Predicate;
            public readonly Func<LexToken, ExpressionNode> Rule;
            
            public PrefixParseRule(Predicate<LexToken> predicate, Func<LexToken, ExpressionNode> rule) {
                Predicate = predicate;
                Rule = rule;
            }
        }
        
        private readonly string _contents;
        private Lexer _lexer;
        private LexToken _cachedToken;

        private readonly List<PrefixParseRule> _prefixExpressionParseTable;
        private readonly List<InfixParseRule>
            _infixExpressionParseTable;

        public Parser(string contents) {
            _contents = contents;

            _prefixExpressionParseTable = new List<PrefixParseRule> {
                new PrefixParseRule(t => t is IntegerToken, t => new IntegerLiteralNode(((IntegerToken) t).Content)),
                new PrefixParseRule(t => t is StringToken, t => new StringLiteralNode(((StringToken) t).Content)),
                new PrefixParseRule(t => t is SymbolToken s && s.Symbol == "(", _ => ParseGrouping()),
            };

            _infixExpressionParseTable = new List<InfixParseRule> {
                new InfixParseRule(t => t is SymbolToken s && new List<string> {"-", "+"}.Contains(s.Symbol),
                                   (t, left) => ParseBinaryExpressionNode(left, t as SymbolToken), Precedence.TERM),
                new InfixParseRule(t => t is SymbolToken s && new List<string> {"*", "/"}.Contains(s.Symbol),
                                   (t, left) => ParseBinaryExpressionNode(left, t as SymbolToken), Precedence.FACTOR),
            };
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
                    // TODO: Parameter lists
                    Expecting<SymbolToken>(t => t.Symbol == ")");
                    Expecting<SymbolToken>(t => t.Symbol == ";");
                    
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

        private ExpressionNode ParseExpression() {
            return ParseExpressionPrecedence(Precedence.ASSIGNMENT);
        }

        private ExpressionNode ParseExpressionPrecedence(Precedence precedence) {
            var token = NextToken();
            var expression = GetMatchingPrefixRule(token).Rule(token);

            token = PeekToken();
            while (GetMatchingInfixRule(token, precedence) != null) {
                expression = GetMatchingInfixRule(token, precedence).Rule(NextToken(), expression);
                token = PeekToken();
            }

            return expression;
        }

        private ExpressionNode ParseBinaryExpressionNode(ExpressionNode leftSide, SymbolToken binOp) {
            switch (binOp.Symbol) {
                    case "+":
                        return new AdditionNode(leftSide, ParseExpressionPrecedence(Precedence.TERM));
                    case "-":
                        return new SubtractionNode(leftSide, ParseExpressionPrecedence(Precedence.TERM));
                    case "*":
                        return new MultiplicationNode(leftSide, ParseExpressionPrecedence(Precedence.FACTOR));
                    case "/":
                        return new DivisionNode(leftSide, ParseExpressionPrecedence(Precedence.FACTOR));
            }
            
            throw new NotImplementedException();
        }

        private PrefixParseRule GetMatchingPrefixRule(LexToken token) {
            return _prefixExpressionParseTable.FindAll(rule => rule.Predicate(token)).FirstOrDefault();
        }

        private InfixParseRule GetMatchingInfixRule(LexToken token, Precedence precedence) {
            return _infixExpressionParseTable.FindAll(rule => rule.Precedence > precedence)
                .FindAll(rule => rule.Predicate(token)).FirstOrDefault();
        }

        private ExpressionNode ParseGrouping() {
            var expr = ParseExpression();
            Expecting<SymbolToken>(s => s.Symbol == ")");
            return expr;
        }

        private PrintStatementNode ParsePrintStatementNode() {
            Expecting<IdentifierToken>(t => t.Content == "print");
            return new PrintStatementNode(ParseExpression());
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
