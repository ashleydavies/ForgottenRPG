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
            var compiled = new Parser("print 'Hello, world!';").Parse();
            var assembled = new Assembler(compiled.Split(new []{ Environment.NewLine }, StringSplitOptions.None).ToList()).Compile();
            var bytecodeString = string.Join(",", assembled);
            List<int> bytecode = bytecodeString.Split(',').Select(int.Parse).ToList();
            var vm = new ScriptVM(bytecode);
            vm.PrintMethod = str => _output.Add(str);
            vm.Execute();
            
            Assert.Equal(_output, new List<string> {"Hello, world!"});
        }
    }
}
