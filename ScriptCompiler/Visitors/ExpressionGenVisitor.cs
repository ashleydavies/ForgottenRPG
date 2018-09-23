using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using ScriptCompiler.AST;

namespace ScriptCompiler.Visitors {
    // TODO: Abstraction over string
    // Returns <commands>, <result register/location>
    public class ExpressionGenVisitor : Visitor<(List<string>, string)> {
        private readonly CodeGenVisitor _codeGenVisitor;

        public ExpressionGenVisitor(CodeGenVisitor codeGenVisitor) {
            _codeGenVisitor = codeGenVisitor;
        }

        public (List<string>, string) VisitDynamic(ASTNode node) {
            (List<string>, string) res = Visit(node as dynamic);
            return (res.Item1, res.Item2);
        }

        public override (List<string>, string) Visit(ASTNode node) {
            throw new System.NotImplementedException(node.GetType().FullName);
        }

        public (List<string>, string) Visit(StringLiteralNode node) {
            // TODO: node.Throw()
            if (!_codeGenVisitor.StringAliases.ContainsKey(node.String)) {
                throw new ArgumentException();
            }

            return (new List<string>(), "!" + _codeGenVisitor.StringAliases[node.String]);
        }

        public (List<string>, string) Visit(IntegerLiteralNode node) {
            return (new List<string>(), node.value.ToString());
        }

        public (List<string>, string) Visit(AdditionNode node) {
            return VisitBinOpNode(node, "ADD");
        }

        public (List<string>, string) Visit(SubtractionNode node) {
            return VisitBinOpNode(node, "SUB");
        }

        public (List<string>, string) Visit(MultiplicationNode node) {
            return VisitBinOpNode(node, "MUL");
        }

        public (List<string>, string) Visit(DivisionNode node) {
            return VisitBinOpNode(node, "DIV");
        }
        
        public (List<string>, string) VisitBinOpNode(BinaryOperatorNode node, string opCommand) {
            List<string> commands = new List<string>();
            var (commandsL, resultL) = VisitDynamic(node.Left);
            var (commandsR, resultR) = VisitDynamic(node.Right);
            commands.AddRange(commandsL);
            commands.AddRange(commandsR);
            var register = _codeGenVisitor.GetRegister();
            commands.Add($"MOV {register} {resultL}");
            commands.Add($"{opCommand} {register} {resultR}");
            return (commands, register);
        }
    }
}
