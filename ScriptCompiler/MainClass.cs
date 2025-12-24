using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ForgottenRPG.Service;
using ForgottenRPG.Service.Null;
using ScriptCompiler.Parsing;
using ForgottenRPG.VM;

namespace ScriptCompiler {
    public static class MainClass {
        public const string ScriptExtension = ".fscr";
        public const string BytecodeExtension = ".fscrbyte";
        public const string AssemblyExtension = ".fscrasm";

        public static void Main(string[] args) {
            string fileName;

            if (args.Length > 1) {
                fileName = args[1];
            } else {
                Console.WriteLine("Enter the path to the file to compile/assemble");
                fileName = Console.ReadLine();
            }

            if (args[0] == "compile") {
                Console.WriteLine(Parser.FromFile(fileName).Compile());
            } else if (args[0] == "assemble") {
                List<string> compiled = new Assembler(File.ReadLines(fileName + AssemblyExtension).ToList()).Compile();

                File.WriteAllText($"{fileName}{BytecodeExtension}", string.Join(",", compiled));
                Console.WriteLine("Completed output:");

                string bytecodeString = string.Join(",", compiled);
                Console.WriteLine($"{compiled.Count} words in total");

                Console.WriteLine(bytecodeString);

                List<uint> bytecode = bytecodeString.Split(',').Select(uint.Parse).ToList();

                ScriptVm scriptVm = new ScriptVm(bytecode);
                scriptVm.Execute();
            } else if (args[0] == "compile-assemble") {
                var compiled = Parser.FromFile(fileName).Compile();
                Console.WriteLine("Compiled code:");
                Console.WriteLine(compiled);

                var assembled =
                    new Assembler(compiled.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList())
                        .Compile();
                string bytecodeString = string.Join(",", assembled);

                Console.WriteLine("Assembled code:");
                Console.WriteLine(bytecodeString);
                Console.WriteLine($"{assembled.Count} words in total");
            } else if (args[0] == "native-compile") {
                var registry = new ForgottenRPG.NativeFunctions.NativeFunctionRegistry();
                registry.ScanType(typeof(ForgottenRPG.Scripting.GameScriptLibrary));
                registry.ScanType(typeof(ForgottenRPG.Entity.GameEntity));
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

                var compiled = Parser.FromFile(fileName).Compile(registry);
                Console.WriteLine("Compiled code:");
                Console.WriteLine(compiled);

                var assembled =
                    new Assembler(compiled.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList())
                        .Compile();
                string bytecodeString = string.Join(",", assembled);

                Console.WriteLine("Assembled code:");
                Console.WriteLine(bytecodeString);
                Console.WriteLine($"{assembled.Count} words in total");
            } else if (args[0] == "execute") {
                ServiceLocator.LogService = new SimpleLogger();
                var registry = new ForgottenRPG.NativeFunctions.NativeFunctionRegistry();
                registry.ScanType(typeof(ForgottenRPG.Scripting.GameScriptLibrary));
                registry.ScanType(typeof(ForgottenRPG.Entity.GameEntity));
                registry.ScanType(typeof(ForgottenRPG.Items.IItem));

                var compiled = Parser.FromFile(fileName).Compile(registry);
                ServiceLocator.LogService.Log(LogType.Info, "Compiled code:");
                ServiceLocator.LogService.Log(LogType.Info, compiled);

                var assembled =
                    new Assembler(compiled.Split([Environment.NewLine], StringSplitOptions.None).ToList())
                        .Compile();
                string bytecodeString = string.Join(",", assembled);

                ServiceLocator.LogService.Log(LogType.Info, "Assembled code:");
                ServiceLocator.LogService.Log(LogType.Info, bytecodeString);
                ServiceLocator.LogService.Log(LogType.Info, $"{assembled.Count} words in total");

                List<uint> bytecode = bytecodeString.Split(',').Select(uint.Parse).ToList();
                var vm = new ScriptVm(bytecode);
                vm.RegisterNativeFunctions(registry);
                vm.Execute();
            } else {
                Console.WriteLine($"Unexpected argument {args[0]}");
            }
        }
    }

    class SimpleLogger : ILogService {
        public void Log(LogType logType, string content) {
            Console.WriteLine($"{logType}: {content}");
        }
    }
}
