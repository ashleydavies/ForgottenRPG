using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.AST.Statements.Expressions.Arithmetic;
using ScriptCompiler.CodeGeneration;

namespace ScriptCompiler.Parsing {
    public class Parser {
        private class InfixParseRule {
            public readonly Predicate<LexToken> Predicate;
            public readonly Func<LexToken, ExpressionNode, ExpressionNode> Rule;
            public readonly Precedence Precedence;

            public InfixParseRule(Predicate<LexToken> predicate, Func<LexToken, ExpressionNode, ExpressionNode> rule,
                                  Precedence precedence) {
                Predicate  = predicate;
                Rule       = rule;
                Precedence = precedence;
            }
        }

        private class PrefixParseRule {
            public readonly Predicate<LexToken> Predicate;
            public readonly Func<LexToken, ExpressionNode> Rule;

            public PrefixParseRule(Predicate<LexToken> predicate, Func<LexToken, ExpressionNode> rule) {
                Predicate = predicate;
                Rule      = rule;
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
            _lexer    = new Lexer(_contents);

            _prefixExpressionParseTable = new List<PrefixParseRule> {
                new PrefixParseRule(t => t is IntegerToken, t => new IntegerLiteralNode(((IntegerToken) t).Content)),
                new PrefixParseRule(t => t is StringToken, t => new StringLiteralNode(((StringToken) t).Content)),
                new PrefixParseRule(t => t is IdentifierToken,
                                    t => new VariableAccessNode(((IdentifierToken) t).Content)),
                new PrefixParseRule(t => t is SymbolToken s && s.Symbol == "(", _ => ParseGrouping()),
                new PrefixParseRule(t => t is SymbolToken s && s.Symbol == Consts.ADDR_OPERATOR,
                                    _ => new AddressOfNode(ParseExpressionPrecedence(Precedence.Factor))),
                new PrefixParseRule(t => t is SymbolToken s && s.Symbol == Consts.DEREF_OPERATOR,
                                    _ => new DereferenceNode(ParseExpressionPrecedence(Precedence.Factor))),
            };

            _infixExpressionParseTable = new List<InfixParseRule> {
                new InfixParseRule(t => t is SymbolToken s && new List<string> {"-", "+"}.Contains(s.Symbol),
                                   (t, left) => ParseBinaryExpressionNode(left, (SymbolToken) t), Precedence.Term),
                new InfixParseRule(t => t is SymbolToken s && new List<string> {"*", "/"}.Contains(s.Symbol),
                                   (t, left) => ParseBinaryExpressionNode(left, (SymbolToken) t), Precedence.Factor),
                new InfixParseRule(t => t is SymbolToken s && new List<string> {"==", "!="}.Contains(s.Symbol),
                                   (t, left) => ParseBinaryExpressionNode(left, (SymbolToken) t), Precedence.Equality),
                new InfixParseRule(t => t is SymbolToken s && new List<string> {">", "<"}.Contains(s.Symbol),
                                   (t, left) => ParseBinaryExpressionNode(left, (SymbolToken) t), Precedence.Comparison),
                new InfixParseRule(t => t is SymbolToken s && s.Symbol == ".",
                                   (t, left) => new StructAccessNode(left, Expecting<IdentifierToken>().Content),
                                   Precedence.Call),
                new InfixParseRule(t => t is SymbolToken s && s.Symbol == "(",
                                   (t, left) => ParseFunctionCallNode(left), Precedence.Accessor),
                new InfixParseRule(t => t is SymbolToken s && s.Symbol == "=",
                                   (t, left) => new AssignmentNode(left, ParseExpression()), Precedence.Assignment),
                // Translating x->y == (*x).y at this time saves increasing AST complexity
                new InfixParseRule(t => t is SymbolToken s && s.Symbol == "->",
                                   (t, left) =>
                                       new StructAccessNode(new DereferenceNode(left),
                                                            Expecting<IdentifierToken>().Content), Precedence.Accessor)
            };
        }

        public string Compile() {
            return string.Join('\n', Optimiser.Optimise(
                                   new CodeGenerator().Generate(Parse())
                               ).Select(p => p.ToString()));
        }

        public ProgramNode Parse() {
            return ParseProgram();
        }

        private ProgramNode ParseProgram() {
            var structs          = new List<StructNode>();
            var functions        = new List<FunctionNode>();
            var statements       = new List<StatementNode>();
            var imports          = new List<ImportNode>();
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

            returnNode = PeekToken() switch {
                IdentifierToken it when it.Content == "if"     => ParseConditionalStatementNode(),
                IdentifierToken it when it.Content == "print"  => ParsePrintStatementNode(),
                IdentifierToken it when it.Content == "return" => ParseReturnStatementNode(),
                IdentifierToken it when PeekToken(1) is IdentifierToken ||
                                        PeekToken(1) is SymbolToken s && s.Symbol == "@"
                => ParseDeclarationStatementNode(),
                _ => ((Func<StatementNode>) (() => {
                    Console.WriteLine($"Dropping to naked expression parsing for {PeekToken()} {PeekToken(1)}");
                    var expressionNode = ParseExpression();
                    Expecting<SymbolToken>(t => t.Symbol == ";");
                    return expressionNode;
                }))(),
            };

            return returnNode;
        }

        private StatementNode ParseConditionalStatementNode() {
            Expecting<IdentifierToken>(t => t.Content == "if");
            return new IfStatementNode(ParseExpression(), ParseCodeBlock());
        }

        private StatementNode ParseReturnStatementNode() {
            Expecting<IdentifierToken>(t => t.Content == "return");
            var returnStatementNode = new ReturnStatementNode(ParseExpression());
            Expecting<SymbolToken>(t => t.Symbol == ";");
            return returnStatementNode;
        }

        private DeclarationStatementNode ParseDeclarationStatementNode() {
            var identifier   = Expecting<IdentifierToken>().Content;
            var pointerDepth = 0;
            while (PeekIgnoreMatch<SymbolToken>(s => s.Symbol == "@")) pointerDepth++;
            var typeNode = new ExplicitTypeNode(identifier, pointerDepth);

            var variableIdentifier = Expecting<IdentifierToken>().Content;

            // If we have a default value, set it up
            if (PeekIgnoreMatch<SymbolToken>(s => s.Symbol == "=")) {
                ExpressionNode initialVal = ParseExpression();
                Expecting<SymbolToken>(t => t.Symbol == ";");
                return new DeclarationStatementNode(typeNode, variableIdentifier, initialVal);
            }

            Expecting<SymbolToken>(t => t.Symbol == ";");
            return new DeclarationStatementNode(typeNode, variableIdentifier);
        }

        private ExpressionNode ParseExpression() {
            return ParseExpressionPrecedence(Precedence.None);
        }

        private ExpressionNode ParseExpressionPrecedence(Precedence precedence) {
            var token      = NextToken()!;
            var expression = GetMatchingPrefixRule(token).Rule(token);

            token = PeekToken();
            while (GetMatchingInfixRule(token, precedence) != null) {
                expression = GetMatchingInfixRule(token, precedence).Rule(NextToken()!, expression);
                token      = PeekToken();
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
            return binOp.Symbol switch {
                "+" => new AdditionNode(leftSide, ParseExpressionPrecedence(Precedence.Term)),
                "-" => new SubtractionNode(leftSide, ParseExpressionPrecedence(Precedence.Term)),
                "*" => new MultiplicationNode(leftSide, ParseExpressionPrecedence(Precedence.Factor)),
                "/" => new DivisionNode(leftSide, ParseExpressionPrecedence(Precedence.Factor)),
                "==" => new EqualityOperatorNode(leftSide, ParseExpressionPrecedence(Precedence.Equality)),
                "!=" => new InequalityOperatorNode(leftSide, ParseExpressionPrecedence(Precedence.Equality)),
                ">" => new GreaterThanOperatorNode(leftSide, ParseExpressionPrecedence(Precedence.Comparison)),
                "<" => new LessThanOperatorNode(leftSide, ParseExpressionPrecedence(Precedence.Comparison)),
                _   => throw new NotImplementedException(),
            };
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
            var printStatementNode = new PrintStatementNode(ParseExpression());
            Expecting<SymbolToken>(t => t.Symbol == ";");
            return printStatementNode;
        }

        private StructNode ParseStructNode() {
            Expecting<IdentifierToken>(t => t.Content == "struct");
            var nameToken = Expecting<IdentifierToken>();
            Expecting<SymbolToken>(t => t.Symbol == "{");

            List<DeclarationStatementNode> declarations = new List<DeclarationStatementNode>();

            while (!PeekMatch<SymbolToken>(t => t.Symbol == "}")) {
                declarations.Add(ParseDeclarationStatementNode());
            }

            Expecting<SymbolToken>(t => t.Symbol == "}");
            return new StructNode(nameToken.Content, declarations);
        }

        private ImportNode ParseImportStatementNode() {
            Expecting<IdentifierToken>(t => t.Content == "import");
            var importStatementNode = new ImportNode(Expecting<StringToken>().Content);
            Expecting<SymbolToken>(t => t.Symbol == ";");
            return importStatementNode;
        }

        private FunctionNode ParseFunctionNode() {
            Expecting<IdentifierToken>(t => t.Content == "func");
            var typeToken    = Expecting<IdentifierToken>();
            int pointerDepth = 0;
            while (PeekIgnoreMatch<SymbolToken>(s => s.Symbol == "@")) pointerDepth++;

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
            return new FunctionNode(nameToken.Content, new ExplicitTypeNode(typeToken.Content, pointerDepth), block,
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

        private T Expecting<T>(Func<T, bool>? predicate = null) where T : class {
            LexToken token = NextToken()!;

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

        private LexToken? NextToken() {
            if (_cachedTokens.Count == 0) return _lexer.NextToken();

            LexToken cached = _cachedTokens[0];
            _cachedTokens.RemoveAt(0);
            return cached;
        }

        private LexToken PeekToken(int depth = 0) {
            if (_cachedTokens.Count > depth) return _cachedTokens[depth];

            for (int i = 0; i <= depth - _cachedTokens.Count; i++) {
                _cachedTokens.Add(_lexer.NextToken()!);
            }

            return _cachedTokens[depth];
        }
    }
}
