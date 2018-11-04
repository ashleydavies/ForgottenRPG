using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.AST.Statements.Expressions.Arithmetic;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Types;

namespace ScriptCompiler.Visitors {
    // TODO: Abstraction over string
    // Returns <commands>, <result register/location>
    public class ExpressionGenVisitor : Visitor<(List<string>, Register)> {
        private readonly CodeGenVisitor _codeGenVisitor;

        public ExpressionGenVisitor(CodeGenVisitor codeGenVisitor) {
            _codeGenVisitor = codeGenVisitor;
        }

        public (List<string>, Register) VisitDynamic(ASTNode node) {
            (List<string>, Register) res = Visit(node as dynamic);
            return res;
        }

        public override (List<string>, Register) Visit(ASTNode node) {
            throw new NotImplementedException(node.GetType().FullName);
        }

        public (List<string>, Register) Visit(StringLiteralNode node) {
            // TODO: node.Throw()
            if (!_codeGenVisitor.StringLiteralAliases.ContainsKey(node.String)) {
                throw new ArgumentException();
            }

            List<string> commands = new List<string>();
            // Put a pointer to the string into a register
            var register = _codeGenVisitor.GetRegister();
            commands.Add($"MOV {register} !{_codeGenVisitor.StringLiteralAliases[node.String]}");
            
            return (commands, register);
        }

        public (List<string>, Register) Visit(VariableAccessNode node) {
            List<string> instructions = new List<string>();
            var register = _codeGenVisitor.GetRegister();
            
            // reg = Stack
            instructions.Add($"MOV {register} r1");
            
            // reg = Stack - offset to variable
            var (type, offset) = _codeGenVisitor.StackFrame.Lookup(node.Identifier);
            if (type == SType.SNoType) {
                // TODO: Line, col
                throw new CompileException($"Unexpected variable {node.Identifier}", 0, 0);
            }
            instructions.Add($"ADD {register} {offset}");
            
            // Read from memory into the same register as we are using
            instructions.Add($"MEMREAD {register} {register}");
            
            return (instructions, register);
        }

        public (List<string>, Register) Visit(IntegerLiteralNode node) {
            var register = _codeGenVisitor.GetRegister();
            return (new List<string>() { $"MOV {register} {node.value}" }, register);
        }

        public (List<string>, Register) Visit(AdditionNode node) {
            return VisitBinOpNode(node, "ADD");
        }

        public (List<string>, Register) Visit(SubtractionNode node) {
            return VisitBinOpNode(node, "SUB");
        }

        public (List<string>, Register) Visit(MultiplicationNode node) {
            return VisitBinOpNode(node, "MUL");
        }

        public (List<string>, Register) Visit(DivisionNode node) {
            return VisitBinOpNode(node, "DIV");
        }
        
        public (List<string>, Register) VisitBinOpNode(BinaryOperatorNode node, string opCommand) {
            List<string> commands = new List<string>();
            var (commandsL, resultL) = VisitDynamic(node.Left);
            var (commandsR, resultR) = VisitDynamic(node.Right);
            using (resultL)
            using (resultR) {
                commands.AddRange(commandsL);
                commands.AddRange(commandsR);
                var register = _codeGenVisitor.GetRegister();
                commands.Add($"MOV {register} {resultL}");
                commands.Add($"{opCommand} {register} {resultR}");
                return (commands, register);
            }
        }
    }
}
