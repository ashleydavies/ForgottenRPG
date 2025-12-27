using System.Collections.Generic;

namespace ForgottenRPG.VM {
    public static class GlobalSymbolRegistry {
        private static readonly Dictionary<uint, uint> _symbols = new();

        public static void Register(uint symbolHash, uint address) {
            _symbols[symbolHash] = address;
        }

        public static bool TryGet(uint symbolHash, out uint address) {
            return _symbols.TryGetValue(symbolHash, out address);
        }

        public static void Clear() {
            _symbols.Clear();
        }
        
        public static IEnumerable<KeyValuePair<uint, uint>> AllSymbols => _symbols;
    }
}
