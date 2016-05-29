#region

using System;
using System.Collections.Generic;

#endregion

namespace DataTypes {
    public class DEDictionary<TLeft, TRight> {
        private readonly Dictionary<TLeft, TRight> _forwardDictionary = new Dictionary<TLeft, TRight>();
        private readonly Dictionary<TRight, TLeft> _backwardDictionary = new Dictionary<TRight,TLeft>();
        public Map<TLeft, TRight> Forward;
        public Map<TRight, TLeft> Backward;

        public DEDictionary() {
            Forward = new Map<TLeft, TRight>(_forwardDictionary);
            Backward = new Map<TRight, TLeft>(_backwardDictionary);
        }

        public void Add(TLeft key, TRight val) {
            if (_forwardDictionary.ContainsKey(key) || _backwardDictionary.ContainsKey(val)) {
                throw new ArgumentException("Neither key must not be present in dictionary");
            }
        }

        public class Map<TMapleft, TMapright> {
            private readonly Dictionary<TMapleft, TMapright> _dictionary;

            public Map(Dictionary<TMapleft, TMapright> dictionary) {
                _dictionary = dictionary;
            }

            public TMapright this[TMapleft key] => _dictionary[key];
        }
    }
}
