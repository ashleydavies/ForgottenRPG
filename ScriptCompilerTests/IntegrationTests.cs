using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCompiler;
using ScriptCompiler.Parsing;
using ForgottenRPG.VM;
using Xunit;

namespace ScriptCompilerTests {
    public class IntegrationTests {
        private readonly List<string> _output = new List<string>();
        [Fact]
        public void CanPrintHelloWorld() {
            ExecuteCode("print 'Hello, world!';");
            Assert.Equal(new List<string> {"Hello, world!"}, _output);
        }

        [Fact]
        public void CanPrintIntegers() {
            ExecuteCode("print 5;");
            Assert.Equal(new List<string> {"5"}, _output);
        }

        [Fact]
        public void CanDoSimpleExpressions() {
            ExecuteCode("print 5 + 5; print 5 + 10; print 5 - 5; print 0 - 5; print 3 * 2; print 6 / 2;");
            Assert.Equal(new List<string> {"10", "15", "0", "-5", "6", "3"}, _output);
        }

        [Fact]
        public void RespectsPrecedenceOfTermsAndFactors() {
            ExecuteCode("print 5 + 2 * 6 / 3 - 4 + 8 * (2 + 1) / (3 + 1 - 1);");
            Assert.Equal(new List<string> {"13"}, _output);
        }

        [Fact]
        public void CanCallFunctionsCorrectly() {
            ExecuteCode("func void test() { print '2'; } print '1'; test(); print '3';");
            Assert.Equal(new List<string> {"1", "2", "3"}, _output);
        }

        [Fact]
        public void CanDoNestedFunctionCalls() {
            ExecuteCode("func void test() { print '2'; test2(); } print '1'; test(); print '4'; func void test2() { print '3'; }");
            Assert.Equal(new List<string> {"1", "2", "3", "4"}, _output);
        }

        [Fact]
        public void CanHandleBasicFunctionReturns() {
            ExecuteCode("func int test() { return 2; } print test();");
            Assert.Equal(new List<string> {"2"}, _output);
        }

        [Fact]
        public void CanHandleStringFunctionReturns() {
            ExecuteCode("func string test() { return 'five'; } print test();");
            Assert.Equal(new List<string> {"five"}, _output);
        }

        [Fact]
        public void CanPrintVariables() {
            ExecuteCode("int x = 5; print x;");
            Assert.Equal(new List<string> {"5"}, _output);
        }

        [Fact]
        public void CanPrintStringVariables() {
            ExecuteCode("string x = 'hello'; print x;");
            Assert.Equal(new List<string> {"hello"}, _output);
        }

        [Fact]
        public void AccessesCorrectVariable() {
            ExecuteCode("int x = 5; int y = 6; int z = 8; string bob = 'hello'; print y;");
            Assert.Equal(new List<string> {"6"}, _output);
        }

        [Fact]
        public void CanAccessMultipleVariables() {
            ExecuteCode("int x = 5; int y = 6; int z = 8; string bob = 'hello'; print y + z * x;");
            Assert.Equal(new List<string> {"46"}, _output);
        }

        [Fact]
        public void CanCallFunctionsWithParameters() {
            ExecuteCode("func void a(int b) { print b; } a(5);");
            Assert.Equal(new List<string> {"5"}, _output);
        }

        [Fact]
        public void CanCallFunctionsWithManyParameters() {
            ExecuteCode("func void a(int b, string c, int d) { print b * d; print c; } a(10, 'hello', 5);");
            Assert.Equal(new List<string> {"50", "hello"}, _output);
        }

        [Fact]
        public void CanCallFunctionsWithManyStackVariables() {
            ExecuteCode("int z; func void a(int b, string c, int d) { print b * d; print c; } int m = 5; int b; string c; a(10, 'hello', 5);");
            Assert.Equal(new List<string> {"50", "hello"}, _output);
        }

        [Fact]
        public void CanCallFunctionsWithManyStackVariablesAndReturns() {
            ExecuteCode("int z; func int a(int b, string c, int d) { print b * d; print c; return 10; } int m = 5; int b; string c; print a(10, 'hello', 5);");
            Assert.Equal(new List<string> {"50", "hello", "10"}, _output);
        }

        /*
         TODO: Enable once struct access expression generation works
        [Fact]
        public void CanHandleAssignmentsToSimpleStructs() {
            ExecuteCode(@"
struct Player {
    int health;
    int mana;
}

Player a;
Player b;

print a.health = 10;
print a.mana = 20;
print b.health = 30;
b = a;
print a.health;
print a.mana;
print b.health;
print b.mana;");
            Assert.Equal(new List<string> {"10", "20", "30", "10", "20", "10", "20"}, _output);
        }
        */
        
        private void ExecuteCode(string code) {
            var compiled = new Parser(code).Compile();
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
