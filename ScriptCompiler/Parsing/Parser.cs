using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.AST.Statements.Expressions.Arithmetic;
using ScriptCompiler.Visitors;

namespace ScriptCompiler.Parsing {
    public class Parser {
        private class InfixParseRule {
            public readonly Predicate<LexToken> Predicate;
            public readonly Func<LexToken, ExpressionNode, ExpressionNode> Rule;
            public readonly Precedence Precedence;

            public InfixParseRule(Predicate<LexToken> predicate, Func<LexToken, ExpressionNode, ExpressionNode> rule,
                Precedence precedence) {
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
        private List<LexToken> _cachedTokens = new List<LexToken>();

        private readonly List<PrefixParseRule> _prefixExpressionParseTable;

        private readonly List<InfixParseRule>
            _infixExpressionParseTable;

        public static Parser FromFile(string filename) {
            return new Parser(File.ReadAllText(filename + MainClass.ScriptExtension));
        }

        public Parser(string contents) {
            _contents = contents;

            _prefixExpressionParseTable = new List<PrefixParseRule> {
                new PrefixParseRule(t => t is IntegerToken, t => new IntegerLiteralNode(((IntegerToken) t).Content)),
                new PrefixParseRule(t => t is StringToken, t => new StringLiteralNode(((StringToken) t).Content)),
                new PrefixParseRule(t => t is IdentifierToken,
                    t => new VariableAccessNode(((IdentifierToken) t).Content)),
                new PrefixParseRule(t => t is SymbolToken s && s.Symbol == "(", _ => ParseGrouping()),
            };

            _infixExpressionParseTable = new List<InfixParseRule> {
                new InfixParseRule(t => t is SymbolToken s && new List<string> {"-", "+"}.Contains(s.Symbol),
                    (t, left) => ParseBinaryExpressionNode(left, t as SymbolToken), Precedence.TERM),
                new InfixParseRule(t => t is SymbolToken s && new List<string> {"*", "/"}.Contains(s.Symbol),
                    (t, left) => ParseBinaryExpressionNode(left, t as SymbolToken), Precedence.FACTOR),
                new InfixParseRule(t => t is SymbolToken s && s.Symbol == ".",
                    (t, left) => new StructAccessNode(left, Expecting<IdentifierToken>().Content), Precedence.CALL),
                new InfixParseRule(t => t is SymbolToken s && s.Symbol == "(",
                    (t, left) => ParseFunctionCallNode(left), Precedence.CALL),
                new InfixParseRule(t => t is SymbolToken s && s.Symbol == "=",
                    (t, left) => new AssignmentNode(left, ParseExpression()), Precedence.ASSIGNMENT)
            };
        }

        public string Compile() {
            return new CodeGenVisitor().Visit(Parse());
        }

        public ProgramNode Parse() {
            _lexer = new Lexer(_contents);
            return ParseProgram();
        }

        private ProgramNode ParseProgram() {
            var structs = new List<StructNode>();
            var functions = new List<FunctionNode>();
            var statements = new List<StatementNode>();
            var imports = new List<ImportNode>();
            var importProcessing = true;

            while (_cachedTokens.Count > 0 || _lexer.HasMore() && PeekToken() != null) {
                var token = PeekToken();

                importProcessing =
                    importProcessing && (token is IdentifierToken exToken && exToken.Content == "import");

                switch (token) {
                    case IdentifierToken itok when itok.Content == "func":
                        functions.Add(ParseFunctionNode());
                        break;
                    case IdentifierToken itok when itok.Content == "import" && importProcessing:
                        imports.Add(ParseImportStatementNode());
                        break;
                    case IdentifierToken itok when itok.Content == "struct":
                        structs.Add(ParseStructNode());
                        break;
                    default:
                        statements.Add(ParseStatementNode());
                        break;
                }
            }

            return new ProgramNode(imports, structs, functions, statements);
        }

        private StatementNode ParseStatementNode() {
            StatementNode returnNode;

            switch (PeekToken()) {
                case IdentifierToken it when it.Content == "print":
                    returnNode = ParsePrintStatementNode();
                    break;
                case IdentifierToken it when it.Content == "return":
                    returnNode = ParseReturnStatementNode();
                    break;
                case IdentifierToken it when PeekToken(1) is IdentifierToken:
                    returnNode = ParseDeclarationStatementNode();
                    break;
                default:
                    Console.WriteLine($"Dropping to naked expression parsing for {PeekToken()} {PeekToken(1)}");
                    returnNode = ParseExpression();
                    break;
            }

            Expecting<SymbolToken>(t => t.Symbol == ";");
            return returnNode;
        }

        private StatementNode ParseReturnStatementNode() {
            Expecting<IdentifierToken>(t => t.Content == "return");
            return new ReturnStatementNode(ParseExpression());
        }

        private DeclarationStatementNode ParseDeclarationStatementNode() {
            var typeNode = new ExplicitTypeNode(Expecting<IdentifierToken>().Content);
            var variableIdentifier = Expecting<IdentifierToken>().Content;

            // If we have a default value, set it up
            if (PeekIgnoreMatch<SymbolToken>(s => s.Symbol == "=")) {
                ExpressionNode initialVal = ParseExpression();
                return new DeclarationStatementNode(typeNode, variableIdentifier, initialVal);
            }

            return new DeclarationStatementNode(typeNode, variableIdentifier);
        }

        private ExpressionNode ParseExpression() {
            return ParseExpressionPrecedence(Precedence.NONE);
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

        private ExpressionNode ParseFunctionCallNode(ExpressionNode left) {
            List<ExpressionNode> @params = new List<ExpressionNode>();

            // If we have parameters, parse them
            while (!PeekMatch<SymbolToken>(t => t.Symbol == ")")) {
                // Parse parameter expression and include it in the list
                @params.Add(ParseExpression());
                if (PeekIgnoreMatch<SymbolToken>(t => t.Symbol == ",")) continue;
                break;
            }

            Expecting<SymbolToken>(t => t.Symbol == ")");

            return new FunctionCallNode(((VariableAccessNode) left).Identifier, @params);
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

        private StructNode ParseStructNode() {
            Expecting<IdentifierToken>(t => t.Content == "struct");
            var nameToken = Expecting<IdentifierToken>();
            Expecting<SymbolToken>(t => t.Symbol == "{");

            List<DeclarationStatementNode> declarations = new List<DeclarationStatementNode>();

            while (!PeekMatch<SymbolToken>(t => t.Symbol == "}")) {
                declarations.Add(ParseDeclarationStatementNode());
                Expecting<SymbolToken>(t => t.Symbol == ";");
            }

            Expecting<SymbolToken>(t => t.Symbol == "}");

            return new StructNode(nameToken.Content, declarations);
        }

        private ImportNode ParseImportStatementNode() {
            Expecting<IdentifierToken>(t => t.Content == "import");
            return new ImportNode(Expecting<StringToken>().Content);
        }

        private FunctionNode ParseFunctionNode() {
            Expecting<IdentifierToken>(t => t.Content == "func");
            var typeToken = Expecting<IdentifierToken>();
            var nameToken = Expecting<IdentifierToken>();
            Expecting<SymbolToken>(t => t.Symbol == "(");

            List<(string type, string name)> paramDefinitions = new List<(string type, string name)>();

            while (!PeekMatch<SymbolToken>(t => t.Symbol == ")")) {
                // Parse parameter expression and include it in the list
                var paramType = Expecting<IdentifierToken>();
                var paramName = Expecting<IdentifierToken>();
                paramDefinitions.Add((paramType.Content, paramName.Content));
                if (PeekIgnoreMatch<SymbolToken>(t => t.Symbol == ",")) continue;
                break;
            }

            Expecting<SymbolToken>(t => t.Symbol == ")");
            var block = ParseCodeBlock();
            return new FunctionNode(nameToken.Content, new ExplicitTypeNode(typeToken.Content), block,
                paramDefinitions);
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

            if (!(token is T tToken))
                throw token.CreateException($"Unexpected token '{token}' (expecting {typeof(T)})");

            if (predicate != null && !predicate(tToken)) {
                throw token.CreateException($"Token '{token}' did not satisfy expected condition");
            }

            return tToken;
        }

        /// <summary>
        /// Similar to PeekMatch, but skips the token if the check was successful. This is useful for syntax which
        /// exists only for redirecting parsing, but has no importance of its own - for example, all that matters about
        /// an equals sign in an assignment is that it is matched; the matched value of "=" is no longer useful after
        /// the conditional has succeeded.
        /// </summary>
        private bool PeekIgnoreMatch<T>(Func<T, bool> predicate) where T : class {
            if (PeekMatch(predicate)) {
                NextToken();
                return true;
            }

            return false;
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
            if (_cachedTokens.Count == 0) return _lexer.NextToken();

            LexToken cached = _cachedTokens[0];
            _cachedTokens.RemoveAt(0);
            return cached;
        }

        private LexToken PeekToken(int depth = 0) {
            if (_cachedTokens.Count > depth) return _cachedTokens[depth];

            for (int i = 0; i <= depth - _cachedTokens.Count; i++) {
                _cachedTokens.Add(_lexer.NextToken());
            }
            return _cachedTokens[depth];
        }
    }
}
