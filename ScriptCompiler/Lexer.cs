﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScriptCompiler {
    public class Lexer {
        private static readonly char[] StringDelimeters = {'\'', '"'};

        private static readonly char[] Symbols = {
            '+', '-', '*', '/', '=', ';',
            '<', '>', '[', ']', '{', '}'
        };

        private int _scanLine = 1;
        private int _scanPosition;
        private bool _unpopped = false;
        private char _unpoppedChar;
        private readonly Stack<char> _charStack;

        public Lexer(string input) {
            _charStack = new Stack<char>(input.ToCharArray().Reverse());
        }

        public LexToken NextToken() {
            if (!HasMore()) return null;

            var next = PeekNextChar();

            if (IsStringDelimeter(next)) {
                return LexString();
            }

            if (IsIdentifierStart(next)) {
                return LexIdentifier();
            }

            if (IsWhitespace(next)) {
                SkipChar();
                return NextToken();
            }

            if (IsSymbol(next)) {
                return LexSymbol();
            }

            throw new Exception();
        }

        private LexToken LexString() {
            int scanL = _scanLine, scanP = _scanPosition;
            char delim = NextChar();

            var (content, terminated) = TakeUntil(c => c == delim);

            if (terminated) {
                throw new LexException("Non-terminated string", scanL, scanP);
            }

            // Pop char to skip the delimeter
            SkipChar();
            return new StringToken(scanL, scanP, content);
        }

        private LexToken LexIdentifier() {
            return new IdentifierToken(_scanLine,
                                       _scanPosition,
                                       TakeUntil(c => !IsIdentifierCharacter(c)).str);
        }

        private LexToken LexSymbol() {
            return new SymbolToken(_scanLine,
                                   _scanPosition,
                                   NextChar().ToString());
        }

        private (string str, bool terminated) TakeUntil(Predicate<char> condition) {
            if (!HasMore()) return ("", true);

            if (condition(PeekNextChar())) {
                return ("", false);
            }

            var next = NextChar();
            var (rest, term) = TakeUntil(condition);
            return (next + rest, term);
        }

        private bool IsWhitespace(char first) {
            return Regex.IsMatch(first.ToString(), @"\s");
        }

        private bool IsIdentifierStart(char first) {
            return Regex.IsMatch(first.ToString(), @"[a-zA-Z]");
        }

        private bool IsIdentifierCharacter(char content) {
            return IsIdentifierStart(content) || Regex.IsMatch(content.ToString(), @"_");
        }

        private bool IsStringDelimeter(char first) {
            return StringDelimeters.Contains(first);
        }

        private bool IsSymbol(char first) {
            return Symbols.Contains(first);
        }

        private void UnpopChar(char unpop) {
            _unpopped = true;
            _unpoppedChar = unpop;
        }

        private char PeekNextChar() {
            if (_unpopped) return _unpoppedChar;
            return _charStack.Peek();
        }

        private char NextChar() {
            if (_unpopped) {
                _unpopped = false;
                return _unpoppedChar;
            }

            _scanPosition++;
            if (_charStack.Peek() == '\n') _scanLine++;

            return _charStack.Pop();
        }

        private void SkipChar() {
            NextChar();
        }

        public bool HasMore() {
            return _unpopped || _charStack.Count > 0;
        }
    }

    internal class LexException : CompileException {
        public LexException(string message, int line, int position) : base(message, line, position) { }
    }

    public abstract class LexToken {
        private int _line;
        private int _position;

        public LexToken(int line, int position) {
            _line = line;
            _position = position;
        }
    }

    public class StringToken : LexToken {
        private readonly string _content;

        public StringToken(int line, int position, string content) : base(line, position) {
            _content = content;
        }
    }

    public class IdentifierToken : LexToken {
        private readonly string _content;

        public IdentifierToken(int line, int position, string content) : base(line, position) {
            _content = content;
        }
    }

    public class SymbolToken : LexToken {
        private readonly string _symbol;

        public SymbolToken(int line, int position, string symbol) : base(line, position) {
            _symbol = symbol;
        }
    }
}
