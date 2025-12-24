using System;
using System.Linq;
using ForgottenRPG.Entity;
using ForgottenRPG.Scripting;
using ScriptCompiler.Parsing;

namespace ScriptCompiler {
    public class NativeCompileHelper {
        public static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("Usage: NativeCompileHelper <script_file>");
                return;
            }

            var registry = new ForgottenRPG.NativeFunctions.NativeFunctionRegistry();
            registry.ScanType(typeof(GameScriptLibrary));
            registry.ScanType(typeof(GameEntity));
            registry.ScanType(typeof(ForgottenRPG.Items.IItem));

            Console.WriteLine("Native functions registered:");
            for (int i = 0; i < 20; i++) {
                try {
                    var func = registry.GetById(i);
                    Console.WriteLine($"  {i}: {func.ScriptName}");
                } catch {
                    break;
                }
            }



            Console.WriteLine($"\nCompiling {args[0]}...");
            var compiled = Parser.FromFile(args[0]).Compile(registry);
            Console.WriteLine("\nCompiled assembly:");
            Console.WriteLine(compiled);

            Console.WriteLine("\nAssembling...");
            var assembled = new Assembler(compiled.Split('\n').ToList()).Compile();
            var bytecode = string.Join(",", assembled);
            
            Console.WriteLine($"\nBytecode ({assembled.Count} words):");
            Console.WriteLine(bytecode);
        }
    }
}
