using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCompiler;
using ScriptCompiler.Parsing;
using ShaRPG.VM;
using Xunit;

namespace ScriptCompilerTests {
    public class IntegrationTests {
        private readonly List<string> _output = new List<string>();
        [Fact]
        public void CanPrintHelloWorld() {
            ExecuteCode("print 'Hello, world!';");
            Assert.Equal(_output, new List<string> {"Hello, world!"});
        }

        [Fact]
        public void CanPrintIntegers() {
            ExecuteCode("print 5;");
            Assert.Equal(_output, new List<string> {"5"});
        }

        [Fact]
        public void CanDoSimpleExpressions() {
            ExecuteCode("print 5 + 5; print 5 + 10; print 5 - 5; print 0 - 5; print 3 * 2; print 6 / 2;");
            Assert.Equal(_output, new List<string> {"10", "15", "0", "-5", "6", "3"});
        }

        [Fact]
        public void RespectsPrecedenceOfTermsAndFactors() {
            ExecuteCode("print 5 + 2 * 6 / 3 - 4 + 8 * (2 + 1) / (3 + 1 - 1);");
            Assert.Equal(_output, new List<string> {"13"});
        }

        [Fact]
        public void CanCallFunctionsCorrectly() {
            ExecuteCode("function void test() { print '2'; } print '1'; test(); print '3';");
            Assert.Equal(_output, new List<string> {"1", "2", "3"});
        }

        [Fact]
        public void CanDoNestedFunctionCalls() {
            ExecuteCode("function void test() { print '2'; test2(); } print '1'; test(); print '4'; function void test2() { print '3'; }");
            Assert.Equal(_output, new List<string> {"1", "2", "3", "4"});
        }

        [Fact]
        public void CanPrintVariables() {
            ExecuteCode("int x = 5; print x;");
            Assert.Equal(_output, new List<string> {"5"});
        }

        [Fact]
        public void CanPrintStringVariables() {
            ExecuteCode("string x = 'hello'; print x;");
            Assert.Equal(_output, new List<string> {"hello"});
        }

        [Fact]
        public void AccessesCorrectVariable() {
            ExecuteCode("int x = 5; int y = 6; int z = 8; string bob = 'hello'; print y;");
            Assert.Equal(_output, new List<string> {"6"});
        }

        [Fact]
        public void CanAccessMultipleVariables() {
            ExecuteCode("int x = 5; int y = 6; int z = 8; string bob = 'hello'; print y + z * x;");
            Assert.Equal(_output, new List<string> {"46"});
        }

        [Fact]
        public void CanCallFunctionsWithParameters() {
            ExecuteCode("function void a(int b) { print b; } a(5);");
            Assert.Equal(_output, new List<string> {"5"});
        }

        [Fact]
        public void CanCallFunctionsWithManyParameters() {
            ExecuteCode("function void a(int b, string c, int d) { print b * d; print c; } a(10, 'hello', 5);");
            Assert.Equal(_output, new List<string> {"50", "hello"});
        }

        [Fact]
        public void CanCallFunctionsWithManyStackVariables() {
            ExecuteCode("int z; function void a(int b, string c, int d) { print b * d; print c; } int m = 5; int b; string c; a(10, 'hello', 5);");
            Assert.Equal(_output, new List<string> {"50", "hello"});
        }
        
        private void ExecuteCode(string code) {
            var compiled = new Parser(code).Parse();
            var assembled =
                new Assembler(compiled.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList()).Compile();
            var bytecodeString = string.Join(",", assembled);
            List<int> bytecode = bytecodeString.Split(',').Select(int.Parse).ToList();
            var vm = new ScriptVM(bytecode);
            vm.PrintMethod = str => _output.Add(str);
            vm.Execute();
        }
    }
}
