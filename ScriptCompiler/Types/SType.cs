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
    }
}
