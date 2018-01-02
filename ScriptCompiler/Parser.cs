using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptCompiler {
    public class Parser {
        private readonly string _contents;
        
        public Parser(string contents) {
            _contents = contents;
        }

        public void Parse() {
            Lexer lexer = new Lexer(_contents);
            LexToken token;
            
            while ((token = lexer.NextToken()) != null) {
                Console.WriteLine(token);
            }
        }
    }
}
