using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using ScriptCompiler;
using ScriptCompiler.Parsing;
using ForgottenRPG.VM;
using Microsoft.VisualBasic;
using NUnit.Framework;
using Xunit;
using Assert = Xunit.Assert;

namespace ScriptCompilerTests {
    public class IntegrationTests {
        private readonly List<string> _output = new List<string>();

        [Fact]
        public void CanPrintHelloWorld() {
            ExecuteCode("print 'Hello, world!';");
            CollectionAssert.AreEqual(new List<string> {"Hello, world!"}, _output);
        }

        [Fact]
        public void CanPrintIntegers() {
            ExecuteCode("print 5;");
            CollectionAssert.AreEqual(new List<string> {"5"}, _output);
        }

        [Fact]
        public void CanDoSimpleExpressions() {
            ExecuteCode("print 5 + 5; print 5 + 10; print 5 - 5; print 0 - 5; print 3 * 2; print 6 / 2;");
            CollectionAssert.AreEqual(new List<string> {"10", "15", "0", "-5", "6", "3"}, _output);
        }

        [Fact]
        public void RespectsPrecedenceOfTermsAndFactors() {
            ExecuteCode("print 5 + 2 * 6 / 3 - 4 + 8 * (2 + 1) / (3 + 1 - 1);");
            CollectionAssert.AreEqual(new List<string> {"13"}, _output);
        }

        [Fact]
        public void CanCallFunctionsCorrectly() {
            ExecuteCode("func void test() { print '2'; } print '1'; test(); print '3';");
            CollectionAssert.AreEqual(new List<string> {"1", "2", "3"}, _output);
        }


        [Fact]
        public void CanDoFunctionCallsWithinFunctions() {
            ExecuteCode(
                "func void test() { print '2'; test2(); } print '1'; test(); print '4'; func void test2() { print '3'; }");
            Assert.Equal(new List<string> {"1", "2", "3", "4"}, _output);
        }

        [Fact]
        public void CanHandleBasicFunctionReturns() {
            ExecuteCode("func int test() { return 2; } print test();");
            CollectionAssert.AreEqual(new List<string> {"2"}, _output);
        }

        [Fact]
        public void CanHandleStringFunctionReturns() {
            ExecuteCode("func string test() { return 'five'; } print test();");
            CollectionAssert.AreEqual(new List<string> {"five"}, _output);
        }

        [Fact]
        public void CanPrintVariables() {
            ExecuteCode("int x = 5; print x;");
            CollectionAssert.AreEqual(new List<string> {"5"}, _output);
        }

        [Fact]
        public void CanPrintStringVariables() {
            ExecuteCode("string x = 'hello'; print x;");
            CollectionAssert.AreEqual(new List<string> {"hello"}, _output);
        }

        [Fact]
        public void AccessesCorrectVariable() {
            ExecuteCode("int x = 5; int y = 6; int z = 8; string bob = 'hello'; print y;");
            CollectionAssert.AreEqual(new List<string> {"6"}, _output);
        }

        [Fact]
        public void CanAccessMultipleVariables() {
            ExecuteCode("int x = 5; int y = 6; int z = 8; string bob = 'hello'; print y + z * x;");
            CollectionAssert.AreEqual(new List<string> {"46"}, _output);
        }

        [Fact]
        public void CanCallFunctionsWithParameters() {
            ExecuteCode("func void a(int b) { print b; } a(5);");
            CollectionAssert.AreEqual(new List<string> {"5"}, _output);
        }

        [Fact]
        public void CanCallFunctionsWithManyParameters() {
            ExecuteCode("func void a(int b, string c, int d) { print b * d; print c; } a(10, 'hello', 5);");
            CollectionAssert.AreEqual(new List<string> {"50", "hello"}, _output);
        }

        [Fact]
        public void CanCallFunctionsWithManyStackVariables() {
            ExecuteCode(
                "int z; func void a(int b, string c, int d) { print b * d; print c; } int m = 5; int b; string c; a(10, 'hello', 5);");
            CollectionAssert.AreEqual(new List<string> {"50", "hello"}, _output);
        }

        [Fact]
        public void CanCallFunctionsWithManyStackVariablesAndReturns() {
            ExecuteCode(
                "int z; func int a(int b, string c, int d) { print b * d; print c; return 10; } int m = 5; int b; string c; print a(10, 'hello', 5);");
            CollectionAssert.AreEqual(new List<string> {"50", "hello", "10"}, _output);
        }

        [Fact]
        public void CanDoNestedFunctionCalls() {
            ExecuteCode("func int a(int b) { print b; return b + 1; } a(a(a(10)));");
            Assert.Equal(new List<string> {"10", "11", "12"}, _output);
        }

        [Fact]
        public void CanCallManyFunctionsInASingleExpression() {
            ExecuteCode(
                "func int a(int b, int c) { print b + c; return b + c * 2; } int b = 1; int c = 2; int d = 3; a(a(1, 2) + a(b, c), a(c, 5));");
            Assert.Equal(new List<string> {"3", "3", "7", "22"}, _output);
        }

        [Fact]
        public void CanDoDeeplyNestedFunctionCallsInASingleExpression() {
            ExecuteCode("func int add(int a, int b) { return a + b; } " +
                        "func int sub(int a, int b) { return a - b; } " +
                        "func int mul(int a, int b) { return a * b; } " +
                        "func int div(int a, int b) { return a / b; } " +
                        "int x = 5; int z = 6;" +
                        "print add(x, add(sub(div(mul(2, z), 3), 4), div(mul(8, add(2, 1)), sub(add(3, 1), 1))));");
            Assert.Equal(new List<string> {"13"}, _output);
        }

        [Fact]
        public void CanDoNestedFunctionCallsWithMixedExpressions() {
            ExecuteCode("func int add(int a, int b) { return a + b; } " +
                        "func int sub(int a, int b) { return a - b; } " +
                        "int one = 1; int five = 5;" +
                        "print add(one, five + add(5, 5));");
            Assert.Equal(new List<string> {"16"}, _output);
        }

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
            CollectionAssert.AreEqual(new List<string> {"10", "20", "30", "10", "20", "10", "20"}, _output);
        }

        [Fact]
        public void CommentsWorkAsExpected() {
            ExecuteCode("print /* 'Hello World' */ 'Goodbye World' /* Test */; // Test");
            CollectionAssert.AreEqual(new List<string> {"Goodbye World"}, _output);
        }

        [Fact]
        public void FunctionsCanReturnStructs() {
            ExecuteCode(
                "struct player { int health; int mana; } func player a(int b, string c, int d) { print b * d; print c; player p; p.mana = 30; return p; } print a(10, 'hello', 5).mana;");
            CollectionAssert.AreEqual(new List<string> {"50", "hello", "30"}, _output);
        }

        [Fact]
        public void EqualityWorksAsExpected() {
            ExecuteCode(
                "print 5 == 5; print 6 == 6; print 5 == 10; print 'hello' == 'hello'; print 'hello' == 'fred';");
            CollectionAssert.AreEqual(new List<string> {"1", "1", "0", "1", "0"}, _output);
        }

        [Fact]
        public void IfStatementsWorkAsExpected() {
            ExecuteCode(
                "if 1 == 1 { print 'Hello World'; } if 1 == 2 { print 'Goodbye World'; } if 'hello' == 'hello' { print 'String'; } if 5 + 5 == 10 - 5 + 5 { print 'Maths'; }");
            CollectionAssert.AreEqual(new List<string> {"Hello World", "String", "Maths"}, _output);
        }

        [Fact]
        public void ConditionalsWorkAsExpected() {
            ExecuteCode(
                "print 1 > 1; print 1 >= 1; print 1 < 1; print 1 <= 1; print 1 == 1; print 1 != 1; print 2 > 1; print 0 < 1; print 1 >= 0; print 1 <= 5;");
            CollectionAssert.AreEqual(new List<string> {"0", "1", "0", "1", "1", "0", "1", "1", "1", "1"}, _output);
        }

        [Fact]
        public void CanDoBasicLoops() {
            ExecuteCode("int sum = 0; for int i = 0; i < 10; i = i + 1 { sum = sum + i; } print sum;");
            CollectionAssert.AreEqual(new List<string> {"45"}, _output);
        }

        [Fact]
        public void CanDoNestedLoops() {
            ExecuteCode(
                "int sum = 0; for int i = 0; i < 3; i = i + 1 { for int j = 0; j < 3; j = j + 1 { sum = sum + i; } } print sum;");
            CollectionAssert.AreEqual(new List<string> {"9"}, _output);
        }

        [Fact]
        public void CanDoDoublyNestedLoops() {
            ExecuteCode(
                "for int j = 0; j < 3; j = j + 1 { for int k = 0; k < 3; k = k + 1 { for int l = 0; l < 3; l = l + 1 { print j * k * l; } } }");
            var expected = new List<string>();
            for (var j = 0; j < 3; j++) {
                for (var k = 0; k < 3; k++) {
                    for (var l = 0; l < 3; l++) {
                        expected.Add((j * k * l).ToString());
                    }
                }
            }
            CollectionAssert.AreEqual(expected, _output);
        }

        [Fact]
        public void CanDoLoopsWithVariablesWithin() {
            ExecuteCode("for int i = 0; i < 3; i = i + 1 { int k = 5; print k + i; }");
            CollectionAssert.AreEqual(new List<string>{"5", "6", "7"}, _output);
        }

        [Fact]
        public void CanDoStaticVariables() {
            ExecuteCode("func void iter() { static int x = 0; print (x = x + 1); } iter(); iter(); iter();");
            CollectionAssert.AreEqual(new List<string>{"1", "2", "3"}, _output);
        }

        [Fact]
        public void CanDoInitialisedStaticVariables() {
            ExecuteCode(@"
static int a = 1;
static int b = 1 + 1;
static int c = 1 + b;
static int d = a + b + c;

print a;
print b;
print c;
print d;");
            CollectionAssert.AreEqual(new List<string>{"1", "2", "3", "6"}, _output);
        }

        [Fact]
        public void CorrectlyHandlesNumericTypes() {
            ExecuteCode("int x = 5; int y = 0 - 7; uint z = 10; uint a = 12; print x; print x + y; print z; print z - a;");
            CollectionAssert.AreEqual(new List<string>{"5", "-2", "10", "4294967294"}, _output);
        }

        [Fact]
        public void CorrectlyHandlesFloats() {
            ExecuteCode("print (1f/32f) + (1f/16f);");
            CollectionAssert.AreEqual(new List<string>{(3f/32f).ToString(CultureInfo.InvariantCulture)}, _output);
        }

        private void ExecuteCode(string code) {
            var compiled = new Parser(code).Compile();
            var assembled =
                new Assembler(compiled.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList()).Compile();
            var       bytecodeString = string.Join(",", assembled);
            List<uint> bytecode       = bytecodeString.Split(',').Select(uint.Parse).ToList();
            var       vm             = new ScriptVm(bytecode);
            vm.PrintMethod = str => _output.Add(str);
            vm.Execute();
        }
    }
}
