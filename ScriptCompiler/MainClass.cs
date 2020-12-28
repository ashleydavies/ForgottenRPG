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
            } else if (args[0] == "execute") {
                ServiceLocator.LogService = new SimpleLogger();
                var compiled = Parser.FromFile(fileName).Compile();
                ServiceLocator.LogService.Log(LogType.Info, "Compiled code:");
                ServiceLocator.LogService.Log(LogType.Info, compiled);

                var assembled =
                    new Assembler(compiled.Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList())
                        .Compile();
                string bytecodeString = string.Join(",", assembled);

                ServiceLocator.LogService.Log(LogType.Info, "Assembled code:");
                ServiceLocator.LogService.Log(LogType.Info, bytecodeString);
                ServiceLocator.LogService.Log(LogType.Info, $"{assembled.Count} words in total");

                List<uint> bytecode = bytecodeString.Split(',').Select(uint.Parse).ToList();
                new ScriptVm(bytecode).Execute();
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
