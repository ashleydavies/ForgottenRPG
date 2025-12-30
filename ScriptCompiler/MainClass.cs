using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
                fileName = Console.ReadLine() ?? throw new ArgumentException("You must provide a file name");
            }

            if (args[0] == "compile") {
                Console.WriteLine(Parser.FromFile(fileName).Compile());
            } else if (args[0] == "compile-stack") {
                var program = Parser.FromFile(fileName).Parse();
                var compiled = new ScriptCompiler.CodeGeneration.StackCodeGenerator().Generate(program);
                Console.WriteLine(string.Join(Environment.NewLine, compiled));
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
                    new Assembler(compiled.Split([Environment.NewLine], StringSplitOptions.None).ToList())
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
                    new Assembler(compiled.Split([Environment.NewLine], StringSplitOptions.None).ToList())
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
            } else if (args[0] == "build-scripts") {
                if (args.Length < 3) {
                    Console.WriteLine("Usage: build-scripts <sourceDir> <outputDir>");
                    return;
                }

                BuildScripts(args[1], args[2]);
            } else {
                Console.WriteLine($"Unexpected argument {args[0]}");
            }
        }

        private static void BuildScripts(string sourceDir, string outputDir) {
            var registry = new ForgottenRPG.NativeFunctions.NativeFunctionRegistry();
            registry.ScanType(typeof(ForgottenRPG.Scripting.GameScriptLibrary));
            registry.ScanType(typeof(ForgottenRPG.Entity.GameEntity));
            registry.ScanType(typeof(ForgottenRPG.Items.IItem));

            if (!Directory.Exists(sourceDir)) {
                Console.WriteLine($"Source directory does not exist: {sourceDir}");
                return;
            }

            Parser.ScriptsRoot = Path.GetFullPath(sourceDir);
            Directory.SetCurrentDirectory(Parser.ScriptsRoot);
            Directory.CreateDirectory(outputDir);
            var scripts = Directory.GetFiles(sourceDir, "*" + ScriptExtension, SearchOption.AllDirectories)
                                   .Where(x => !x.EndsWith(".extern" + ScriptExtension, StringComparison.OrdinalIgnoreCase))
                                   .OrderBy(x => x)
                                   .ToList();

            foreach (var scriptPath in scripts) {
                string relativePath = Path.GetRelativePath(sourceDir, scriptPath);
                string relativeWithoutExt = Path.ChangeExtension(relativePath, null);
                string outputPath = Path.Combine(outputDir, relativeWithoutExt + BytecodeExtension);
                string hashPath = outputPath + ".sha";

                string scriptHash = ComputeScriptHash(scriptPath, Parser.ScriptsRoot);
                if (File.Exists(outputPath) && File.Exists(hashPath)) {
                    string existingHash = File.ReadAllText(hashPath).Trim();
                    if (string.Equals(existingHash, scriptHash, StringComparison.OrdinalIgnoreCase)) {
                        Console.WriteLine($"Up-to-date: {relativePath}");
                        continue;
                    }
                }

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

                string scriptWithoutExt = Path.ChangeExtension(scriptPath, null)!;
                var compiled = Parser.FromFile(scriptWithoutExt).Compile(registry);
                var assembled =
                    new Assembler(compiled.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList())
                        .Compile();
                string bytecodeString = string.Join(",", assembled);

                File.WriteAllText(outputPath, bytecodeString);
                File.WriteAllText(hashPath, scriptHash);
                Console.WriteLine($"Built: {relativePath} -> {outputPath}");
            }
        }

        private static string ComputeScriptHash(string scriptPath, string scriptsRoot) {
            var builder = new StringBuilder();
            builder.AppendLine("build-scripts-v1");

            foreach (var path in GetScriptDependencies(scriptPath, scriptsRoot)) {
                builder.AppendLine(path);
                builder.AppendLine(File.ReadAllText(path));
            }

            using var sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());
            byte[] hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        private static IEnumerable<string> GetScriptDependencies(string scriptPath, string scriptsRoot) {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var stack = new Stack<string>();
            stack.Push(Path.GetFullPath(scriptPath));

            while (stack.Count > 0) {
                string current = stack.Pop();
                if (!visited.Add(current)) continue;

                yield return current;

                foreach (var importPath in ReadImports(current, scriptsRoot)) {
                    stack.Push(importPath);
                }
            }
        }

        private static IEnumerable<string> ReadImports(string scriptPath, string scriptsRoot) {
            string rootDir = Directory.GetCurrentDirectory();
            foreach (var line in File.ReadLines(scriptPath)) {
                string trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("//", StringComparison.Ordinal)) {
                    continue;
                }

                if (!trimmed.StartsWith("import ", StringComparison.Ordinal)) {
                    yield break;
                }

                int firstQuote = trimmed.IndexOf('"');
                int lastQuote = trimmed.LastIndexOf('"');
                if (firstQuote < 0 || lastQuote <= firstQuote) {
                    continue;
                }

                string importName = trimmed.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
                string importPath = importName.EndsWith(ScriptExtension, StringComparison.OrdinalIgnoreCase)
                    ? importName
                    : importName + ScriptExtension;
                string resolved = Path.IsPathRooted(importPath)
                    ? importPath
                    : Path.Combine(scriptsRoot, importPath);

                if (!File.Exists(resolved)) {
                    resolved = Path.IsPathRooted(importPath)
                        ? importPath
                        : Path.Combine(rootDir, importPath);
                }

                if (!File.Exists(resolved)) {
                    throw new FileNotFoundException($"Import not found: {resolved} (from {scriptPath})");
                }

                yield return resolved;
            }
        }
    }

    class SimpleLogger : ILogService {
        public void Log(LogType logType, string content) {
            Console.WriteLine($"{logType}: {content}");
        }
    }
}
