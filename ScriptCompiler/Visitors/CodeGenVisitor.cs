using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Types;

namespace ScriptCompiler.Visitors {
    public class CodeGenVisitor : Visitor<string>, IRegisterAllocator {
        public StackFrame StackFrame { get; private set; }
        public readonly Dictionary<string, string> StringLiteralAliases = new Dictionary<string, string>();

        private int _returnLabelCount = 0;
        // The instruction and stack pointer registers are always 'occupied'
        private readonly List<bool> _occupiedRegisters = new List<bool> { true, true };

        
        /// <summary>
        /// Main entry method - sets up private parameters and generates code for a full program
        /// </summary>
        public string Visit(ProgramNode node) {
            // Set up the initial stack frame
            StackFrame = new StackFrame();
            List<string> programStrings = new StringLiteralCollectorVisitor().Visit(node);
            
            StringBuilder programBuilder = new StringBuilder();

            programBuilder.AppendLine(".data");
            for (int i = 0; i < programStrings.Count; i++) {
                StringLiteralAliases[programStrings[i]] = $"str_{i}";
                programBuilder.AppendLine($"STRING str_{i} {programStrings[i]}");
            }
            programBuilder.AppendLine(Comment(".text", "Begin code"));
            programBuilder.AppendLine(VisitStatementBlock(node.StatementNodes));
            // Exit, don't fall into a random function
            programBuilder.AppendLine(Comment("JMP end", "Standard termination"));
            
            foreach (dynamic functionNode in node.FunctionNodes) {
                programBuilder.AppendLine(this.Visit(functionNode));
            }

            programBuilder.AppendLine("LABEL end");

            return programBuilder.ToString();
        }
        
        public string Visit(FunctionNode node) {
            // Set up the stack frame for the function parameters
            StackFrame = new StackFrame(null);
            node.ParameterDefinitions.ForEach(p => StackFrame.AddIdentifier(SType.FromTypeString(p.type), p.name));
            
            // TODO: Uncomment when the instruction pointer is stored on the memory stack and not the stack machine
            // 'Push' the instruction pointer, which is the same length as an integer
            //StackFrame.Pushed(SType.SInteger);
            
            var functionBuilder = new StringBuilder();
            
            functionBuilder.AppendLine($"LABEL func_{node.FunctionName}");
            functionBuilder.AppendLine(VisitStatementBlock(node.CodeBlock.Statements));
            
            // AKA RET
            functionBuilder.AppendLine($"POP r0");

            node.ParameterDefinitions.ForEach(p => StackFrame.Popped(SType.FromTypeString(p.type)));
            return functionBuilder.ToString();
        }

        public string Visit(FunctionCallNode node) {
            var functionCallBuilder = new StringBuilder();

            // Save the address of where we should return to after the function ends
            var returnLabelNo = _returnLabelCount++;
            functionCallBuilder.AppendLine($"PUSH $return_{returnLabelNo}");
            
            // Push each of the parameters
            foreach (var arg in node.Args) {
                var (argCommands, argReg) = new ExpressionGenVisitor(this).VisitDynamic(arg);
                // All arguments are one word big
                StackFrame.Pushed(SType.SInteger);
                argCommands.ForEach(c => functionCallBuilder.AppendLine(c));
                using (argReg) {
                    // Push the argument onto the stack for the function to use
                    functionCallBuilder.AppendLine($"MEMWRITE r1 {argReg}");
                    // Shift the stack up
                    functionCallBuilder.AppendLine($"ADD r1 1");
                }
            }
            
            functionCallBuilder.AppendLine($"JMP func_{node.FunctionName}");
            functionCallBuilder.AppendLine($"LABEL return_{returnLabelNo}");
            
            // Fix the stack
            foreach (var arg in node.Args) StackFrame.Popped(SType.SInteger);
            functionCallBuilder.AppendLine($"SUB r1 {node.Args.Count}");

            return functionCallBuilder.ToString();
        }

        public string Visit(DeclarationStatementNode node) {
            var declarationBuilder = new StringBuilder();

            if (StackFrame.ExistsLocalScope(node.Identifier)) {
                // TODO: Add line and col numbers (as well as other debug info) to all nodes, and report correctly here
                throw new CompileException($"Attempt to redefine identifier {node.Identifier}", 0, 0);
            }

            SType type = SType.FromTypeString(node.TypeString);
            if (type == SType.SNoType) {
                throw new CompileException($"Unable to discern type from {node.TypeString}", 0, 0);
            }
            
            StackFrame.AddIdentifier(type, node.Identifier);
            // Adjust stack pointer
            declarationBuilder.AppendLine($"ADD r1 {type.Length}");
            
            // Set up with default value, if any
            if (node.InitialValue != null) {
                var (commands, resultReg) = new ExpressionGenVisitor(this).VisitDynamic(node.InitialValue);
                commands.ForEach(s => declarationBuilder.AppendLine(s));
                // TODO: Remove duplication with the ExpressionGenVisitor VariableAccessNode handler
                using (resultReg) {
                    // Put the memory location of our variable into a free register
                    using (var locationReg = GetRegister()) {
                        // reg = Stack
                        declarationBuilder.AppendLine($"MOV {locationReg} r1");
                        
                        // reg = Stack - offset to variable
                        var offset = StackFrame.Lookup(node.Identifier).position;
                        declarationBuilder.AppendLine($"ADD {locationReg} {offset}");
                        
                        // Write to memory
                        declarationBuilder.AppendLine($"MEMWRITE {locationReg} {resultReg}");
                    }
                }
            }
            
            return declarationBuilder.ToString();
        }

        public string Visit(PrintStatementNode node) {
            var (commands, register) = new ExpressionGenVisitor(this).VisitDynamic(node.Expression);

            StringBuilder builder = new StringBuilder();
            
            using (register) {
                foreach (var command in commands) {
                    builder.AppendLine(command);
                }

                // There are different print instructions depending on the type of the thing we are printing
                var type = new TypeDeterminationVisitor(this).VisitDynamic(node.Expression);

                if (ReferenceEquals(type, SType.SString)) {
                    builder.AppendLine($"PRINT {register}");
                } else if (ReferenceEquals(type, SType.SInteger)) {
                    builder.AppendLine($"PRINTINT {register}");
                }
            }

            return builder.ToString();
        }

        public string VisitStatementBlock(List<StatementNode> statements) {
            var blockBuilder = new StringBuilder();
            
            foreach (dynamic statementNode in statements) {
                blockBuilder.AppendLine(Visit(statementNode));
            }

            return blockBuilder.ToString();
        }

        public Register GetRegister() {
            for (int i = 0; i < _occupiedRegisters.Count; i++) {
                if (_occupiedRegisters[i] == false) {
                    _occupiedRegisters[i] = true;
                    return new Register(i, () => _occupiedRegisters[i] = false);
                }
            }

            int idx = _occupiedRegisters.Count;
            _occupiedRegisters.Add(true);
            return new Register(idx, () => _occupiedRegisters[idx] = false);
        }

        public override string Visit(ASTNode node) {
            throw new NotImplementedException(node.GetType().Name);
        }

        public static string Comment(string test, string comment) {
            return test.PadRight(40) + "# " + comment;
        }
    }
}
