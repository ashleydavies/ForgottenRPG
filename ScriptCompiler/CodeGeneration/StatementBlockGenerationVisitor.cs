using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ForgottenRPG.VM;
using ScriptCompiler.AST;
using ScriptCompiler.AST.Statements;
using ScriptCompiler.AST.Statements.Expressions;
using ScriptCompiler.CodeGeneration.Assembly;
using ScriptCompiler.CodeGeneration.Assembly.Instructions;
using ScriptCompiler.CompileUtil;
using ScriptCompiler.Types;
using ScriptCompiler.Visitors;
using Register = ScriptCompiler.CodeGeneration.Assembly.Register;

namespace ScriptCompiler.CodeGeneration {
    public class StatementBlockGenerationVisitor : Visitor<List<Instruction>> {
        private readonly FunctionTypeRepository _functionTypeRepository;
        private readonly UserTypeRepository _userTypeRepository;
        private readonly Dictionary<string, StringLabel> StringPoolAliases;

        private StackFrame _stackFrame = new StackFrame();
        private RegisterManager _regManager = new RegisterManager();

        private Register StackPointer => _regManager.StackPointer;

        public StatementBlockGenerationVisitor(FunctionTypeRepository functionTypeRepository,
                                               UserTypeRepository userTypeRepository,
                                               Dictionary<string, StringLabel> stringPoolAliases) {
            _functionTypeRepository = functionTypeRepository;
            _userTypeRepository     = userTypeRepository;
            StringPoolAliases       = stringPoolAliases;
        }

        public List<Instruction> VisitStatementBlock(List<StatementNode> statements) {
            return statements.SelectMany<StatementNode, Instruction>(p => Visit(p as dynamic)).ToList();
        }

        public override List<Instruction> Visit(ASTNode node) {
            throw new System.NotImplementedException();
        }

        public List<Instruction> Visit(DeclarationStatementNode node) {
            var instructions = new List<Instruction>();

            if (_stackFrame.ExistsLocalScope(node.Identifier)) {
                // TODO: Add line and col numbers (as well as other debug info) to all nodes, and report correctly here
                throw new CompileException($"Attempt to redefine identifier {node.Identifier}", 0, 0);
            }

            SType type = node.TypeNode.GetSType(_userTypeRepository);
            if (ReferenceEquals(type, SType.SNoType)) {
                throw new CompileException($"Unable to discern type from {node.TypeNode}", 0, 0);
            }

            _stackFrame.AddIdentifier(type, node.Identifier);
            // Adjust stack pointer
            instructions.Add(new AddInstruction(StackPointer, type.Length)
                                 .WithComment($"Declaring {node.Identifier}"));

            // Set up with default value, if any
            if (node.InitialValue != null) {
                /*var (commands, resultReg) = new ExpressionGenVisitor(this).GenerateCode(node.InitialValue);
                commands.ForEach(s => declarationBuilder.AppendLine(s));
                // TODO: Remove duplication with the ExpressionGenVisitor VariableAccessNode handler
                using (resultReg) {
                    // Put the memory location of our variable into a free register
                    using (var locationReg = GetRegister()) {
                        // reg = Stack
                        declarationBuilder.AppendLine($"MOV {locationReg} r1");

                        // reg = Stack - offset to variable
                        var offset = _stackFrame.Lookup(node.Identifier).position;
                        declarationBuilder.AppendLine($"ADD {locationReg} {offset}");

                        // Write to memory
                        declarationBuilder.AppendLine($"MEMWRITE {locationReg} {resultReg}");
                    }
                }*/
            } else {
                instructions.Add(new SubInstruction(StackPointer, type.Length)
                                     .WithComment("Move to start of object to write zeros"));
                for (int i = 0; i < type.Length; i++) {
                    instructions.Add(new MemWriteInstruction(StackPointer, 0));
                    instructions.Add(new AddInstruction(StackPointer, 1));
                }
            }

            return instructions;
        }

        public List<Instruction> Visit(PrintStatementNode node) {
            return new List<Instruction>();
        }

        public List<Instruction> Visit(ReturnStatementNode node) {
            return new List<Instruction>();
        }

        public List<Instruction> Visit(ExpressionNode node) {
            return new List<Instruction>();
        }
    }
}
