using System.Collections.Generic;

namespace ForgottenRPG.VM {
    public static class GlobalSymbolRegistry {
        private static readonly Dictionary<string, uint> _symbols = new();

        public static void Register(string name, uint address) {
            _symbols[name] = address;
        }

        public static bool TryGet(string name, out uint address) {
            return _symbols.TryGetValue(name, out address);
        }

        public static void Clear() {
            _symbols.Clear();
        }
        
        public static IEnumerable<KeyValuePair<string, uint>> AllSymbols => _symbols;
    }
}
