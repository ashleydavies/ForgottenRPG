namespace ScriptCompiler.Types {
    public class SType {
        public static readonly SType SVoid = new SType("void", 0);
        public static readonly SType SNoType = new SType("NO TYPE");
        public static readonly SType SInteger = new SType("int");
        public static readonly SType SBool = new SType("bool");
        public static readonly SType SChar = new SType("char");
        public static readonly SType SString = new ReferenceType(SChar);
        public static readonly SType SGenericPtr = new ReferenceType(SNoType);

        public readonly string Name;
        public readonly int Length;

        // Most types are one (32 bit) word long
        public SType(string name, int length = 1) {
            Name = name;
            Length = length;
        }

        public bool IsUnknownType() {
            return ReferenceEquals(this, SNoType);
        }
        
        // Do reference equality on basic types; more complicated types will do more complicated forms of equality
        public override bool Equals(object? obj) {
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
        public static SType FromTypeString(string nodeTypeString, UserTypeRepository utr, int pointerDepth = 0) {
            SType type = FromTypeString(nodeTypeString, utr);
            if (ReferenceEquals(type, SNoType)) return type;
            while (pointerDepth > 0) {
                pointerDepth--;
                type = new ReferenceType(type);
            }

            return type;
        }

        private static SType FromTypeString(string nodeTypeString, UserTypeRepository utr) {
            switch (nodeTypeString) {
                case "int":
                    return SInteger;
                case "bool":
                    return SBool;
                case "char":
                    return SChar;
                case "string":
                    return SString;
                case "void":
                    return SVoid;
                case { } when utr.ExistsType(nodeTypeString):
                    return utr[nodeTypeString];
            }

            return SNoType;
        }
    }
}
