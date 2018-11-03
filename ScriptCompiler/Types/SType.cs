namespace ScriptCompiler.Types {
    public class SType {
        public static readonly SType SNoType = new SType();
        public static readonly SType SInteger = new SType();
        public static readonly SType SChar = new SType();
        public static readonly SType SString = new ReferenceType(SChar);

        // Most types are one (32 bit) word long
        public readonly int Length = 1;
        
        // Do reference equality on basic types; more complicated types will do more complicated forms of equality
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return false;
        }

        public override int GetHashCode() {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Returns a type from a type string (e.g. from 'int x = 5;'), returning SNoType if no type could be decided on
        /// </summary>
        public static SType FromTypeString(string nodeTypeString) {
            switch (nodeTypeString) {
                case "int":
                    return SInteger;
                case "char":
                    return SChar;
                case "string":
                    return SString;
            }

            return SNoType;
        }
    }
}
