using System.Collections.Generic;
using System.IO;
using System.Linq;
using ForgottenRPG.VM;
using ScriptCompiler;
using ScriptCompiler.Parsing;
using Xunit;

namespace ScriptCompilerTests {
    public class StdlibLinkingTests {
        [Fact]
        public void StdlibExternMallocLinksAndExecutes() {
            Directory.SetCurrentDirectory("/Users/ashley/ShaRPG/ScriptCompiler");

            var stdlibAsm = Parser.FromFile("mem").Compile();
            var stdlibBytes = new Assembler(stdlibAsm.Split('\n').ToList())
                              .Compile()
                              .Select(uint.Parse)
                              .ToList();

            var scriptAsm = Parser.FromFile("mem_demo").Compile();
            var scriptBytes = new Assembler(scriptAsm.Split('\n').ToList())
                              .Compile()
                              .Select(uint.Parse)
                              .ToList();

            var outputs = new List<string>();
            var vm = new ScriptVm(stdlibBytes, scriptBytes) {
                PrintMethod = s => outputs.Add(s)
            };

            vm.ResolveRelocations();

            var exportOffset = stdlibBytes[3];
            var exportCount = stdlibBytes[(int)exportOffset];
            var exports = new Dictionary<uint, uint>();
            int exportIndex = (int)exportOffset + 1;
            for (uint i = 0; i < exportCount; i++) {
                uint hash = stdlibBytes[exportIndex++];
                uint address = stdlibBytes[exportIndex++];
                exports[hash] = address;
            }

            var relocationOffset = scriptBytes[2];
            var relocationCount = scriptBytes[(int)relocationOffset];
            int relocationIndex = (int)relocationOffset + 1;
            uint? patchedAddress = null;
            for (uint i = 0; i < relocationCount; i++) {
                uint addressToPatch = scriptBytes[relocationIndex++];
                uint hash = scriptBytes[relocationIndex++];
                if (exports.TryGetValue(hash, out var exportAddress)) {
                    uint loadOffset = (uint.MaxValue / MemoryPage.PageSize / 5) * MemoryPage.PageSize;
                    patchedAddress = vm.DebugReadMemory(addressToPatch + loadOffset);
                    Assert.Equal(exportAddress, patchedAddress.Value);
                    break;
                }
            }

            Assert.True(patchedAddress.HasValue);
            vm.Execute();

            Assert.Equal(new[] {"5", "10", "29"}, outputs);
        }
    }
}
