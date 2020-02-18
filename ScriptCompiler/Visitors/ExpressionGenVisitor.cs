using System;
using System.Collections.Generic;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.AST.Statements.Expressions.Arithmetic;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Types;
using static ScriptCompiler.Visitors.CodeGenVisitor;

namespace ScriptCompiler.Visitors {
    // TODO: Abstraction over string
    // Returns <commands>, <result register/location>
    public class ExpressionGenVisitor : Visitor<(List<string>, Register)> {
        private static int _returnLabelCount;
        private readonly CodeGenVisitor _codeGenVisitor;
        
        // Set to false when the address of a value is desired rather than its value; signals generation to
        //   skip the final read from memory step (e.g. for struct access)
        private bool _directAccess = true;
        // The length of the portion of the stack being used to hold function return values; this stack space is
        //   reserved until all of the expression has finished being processed to avoid having to manage this more
        //   granularly
        private int _functionReturnLength = 0;

        public ExpressionGenVisitor(CodeGenVisitor codeGenVisitor) {
            _codeGenVisitor = codeGenVisitor;
        }

        public (List<string>, Register) GenerateCode(ASTNode node) {
            var (commands, dest) = VisitDynamic(node);
            
            commands.Add(Comment($"SUB r1 {_functionReturnLength}", "End of expression"));
            for (int i = 0; i < _functionReturnLength; i++) _codeGenVisitor.StackFrame.Popped(SType.SInteger);
            return (commands, dest);
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

        public (List<string>, Register) Visit(FunctionCallNode node) {
            var commands = new List<string>();

            // Save the address of where we should return to after the function ends
            var returnLabelNo = _returnLabelCount++;
            commands.Add($"PUSH $return_{returnLabelNo}");
            
            // Add space for returned data
            var returnType = _codeGenVisitor.FTRepo.ReturnType(node.FunctionName);
            commands.Add($"ADD r1 {returnType.Length}");
            _codeGenVisitor.StackFrame.Pushed(returnType);
            _functionReturnLength += returnType.Length;
            
            // Push each of the parameters
            foreach (var arg in node.Args) {
                var (argCommands, argReg) = new ExpressionGenVisitor(_codeGenVisitor).GenerateCode(arg);
                // All arguments are one word big
                _codeGenVisitor.StackFrame.Pushed(SType.SInteger);
                commands.AddRange(argCommands);
                using (argReg) {
                    // Push the argument onto the stack for the function to use
                    commands.Add($"MEMWRITE r1 {argReg}");
                    // Shift the stack up
                    commands.Add($"ADD r1 1");
                }
            }
            
            commands.Add($"JMP func_{node.FunctionName}");
            commands.Add($"LABEL return_{returnLabelNo}");
            
            // Fix the stack
            foreach (var arg in node.Args) _codeGenVisitor.StackFrame.Popped(SType.SInteger);
            commands.Add($"SUB r1 {node.Args.Count}");

            Register returnReg = _codeGenVisitor.GetRegister();
            commands.Add($"MOV {returnReg} r1");
            if (_directAccess) {
                commands.Add($"MEMREAD {returnReg} {returnReg}");
            }
            return (commands, returnReg);
        }

        public (List<string>, Register) Visit(VariableAccessNode node) {
            List<string> instructions = new List<string>();
            var register = _codeGenVisitor.GetRegister();
            
            // reg = Stack
            instructions.Add($"MOV {register} r1");
            
            // reg = Stack - offset to variable
            var (type, offset) = _codeGenVisitor.StackFrame.Lookup(node.Identifier);
            if (ReferenceEquals(type, SType.SNoType)) {
                // TODO: Line, col
                throw new CompileException($"Unexpected variable {node.Identifier}", 0, 0);
            }
            instructions.Add($"ADD {register} {offset}");
            
            // Read from memory into the same register as we are using
            if (_directAccess)
                instructions.Add($"MEMREAD {register} {register}");
            
            return (instructions, register);
        }

        public (List<string>, Register) Visit(AssignmentNode node) {
            List<string> instructions = new List<string>();
            
            var leftType = new TypeDeterminationVisitor(_codeGenVisitor).VisitDynamic(node.Destination);
            Console.WriteLine($"An assignment to a destination of size {leftType.Length}");

            var (valCommands, valDest) = VisitDynamic(node.Value);
            instructions.AddRange(valCommands);

            var copyReg = _codeGenVisitor.GetRegister();
            
            using (valDest) {
                // Disable direct access to get the address of the destination rather than the first word
                bool reenable = _directAccess;
                _directAccess = false;
                var (destCommands, destDest) = VisitDynamic(node.Destination);
                _directAccess = reenable;
                instructions.AddRange(destCommands);
                using (destDest) {
                    // TODO: Multi-word assignments
                    instructions.Add($"MEMWRITE {destDest} {valDest}");
                    instructions.Add($"MOV {copyReg} {valDest}");
                }
            }

            return (instructions, copyReg);
        }

        public (List<string>, Register) Visit(StructAccessNode node) {
            List<string> instructions = new List<string>();
            
            var structType = new TypeDeterminationVisitor(_codeGenVisitor).VisitDynamic(node.Left) as UserType;
            if (structType == null || ReferenceEquals(structType, SType.SNoType)) {
                throw new CompileException($"Unexpected type {structType}; expected struct.", 0, 0);
            }

            // Disable direct access to get the address of the struct rather than the first word
            bool reenable = _directAccess;
            _directAccess = false;
            var (commands, result) = VisitDynamic(node.Left);
            _directAccess = reenable;
            
            instructions.AddRange(commands);

            if (structType.Length == 1) {
                Console.WriteLine($"Single field struct; accessing {node.Field} directly.");
            } else {
                Console.WriteLine($"Accessing {node.Field}");
                instructions.Add($"ADD {result} {structType.OffsetOfField(node.Field)}");
                if (_directAccess)
                    instructions.Add($"MEMREAD {result} {result}");
            }
            
            return (instructions, result);
        }

        public (List<string>, Register) Visit(IntegerLiteralNode node) {
            var register = _codeGenVisitor.GetRegister();
            return (new List<string> { $"MOV {register} {node.Value}" }, register);
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
