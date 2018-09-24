using ScriptCompiler.Parsing;
using Xunit;

namespace ScriptCompilerTest
{
    public class IntegrationTests {
        [Fact]
        public void CanPrintHelloWorld() {
            var result = new Parser("print 'Hello, world!';").Parse();
        }
    }
}