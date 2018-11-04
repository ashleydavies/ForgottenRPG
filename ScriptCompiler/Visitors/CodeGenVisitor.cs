using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
            StackFrame = new StackFrame(new Dictionary<string, (SType, int)>());
            List<string> programStrings = new StringLiteralCollectorVisitor().Visit(node);
            
            StringBuilder programBuilder = new StringBuilder();

            programBuilder.AppendLine(".data");
            for (int i = 0; i < programStrings.Count; i++) {
                StringLiteralAliases[programStrings[i]] = $"str_{i}";
                programBuilder.AppendLine($"STRING str_{i} {programStrings[i]}");
            }
            programBuilder.AppendLine(".text");
            programBuilder.AppendLine(VisitStatementBlock(node.StatementNodes));

            // Exit, don't fall into a random function
            programBuilder.AppendLine("JMP end");
            
            foreach (dynamic functionNode in node.FunctionNodes) {
                programBuilder.AppendLine(this.Visit(functionNode));
            }

            programBuilder.AppendLine("LABEL end");

            return programBuilder.ToString();
        }

        public string Visit(FunctionNode node) {
            // Set up the stack frame for the function parameters
            node.ParameterDefinitions.ForEach(p => StackFrame.Pushed(SType.FromTypeString(p.type)));
            
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
            functionCallBuilder.AppendLine($"JMP func_{node.FunctionName}");
            functionCallBuilder.AppendLine($"LABEL return_{returnLabelNo}");

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

                if (type == SType.SString) {
                    builder.AppendLine($"PRINT {register}");
                } else if (type == SType.SInteger) {
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
    }
}
