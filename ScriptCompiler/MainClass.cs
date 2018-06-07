using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShaRPG.VM;

namespace ScriptCompiler {
    public static class MainClass {
        public static void Main(string[] args) {
            string fileName;

            if (args.Length > 1) {
                fileName = args[1];
            } else {
                Console.WriteLine("Enter the path to the file to compile/assemble");
                fileName = Console.ReadLine();
            }
            
            if (args[0] == "compile") {
                Console.WriteLine(new Parser(File.ReadAllText(fileName + ".sscript")).Parse());
            } else if (args[0] == "assemble") {
                List<string> compiled = new Assembler(File.ReadLines(fileName + ".shascr").ToList()).Compile();

                File.WriteAllLines($"{fileName}.shascrbyte", compiled);
                Console.WriteLine("Completed output:");

                string bytecodeString = string.Join(",", compiled);
                
                Console.WriteLine(bytecodeString);
                
                List<int> bytecode = bytecodeString.Split(',').Select(int.Parse).ToList();

                ScriptVM scriptVm = new ScriptVM(bytecode);
                scriptVm.Execute();
            } else {
                Console.WriteLine($"Unexpected argument {args[0]}");
            }
        }
    }
}
