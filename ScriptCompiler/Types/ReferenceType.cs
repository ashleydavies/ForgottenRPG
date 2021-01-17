namespace ScriptCompiler.Types {
    public class ReferenceType : SType {
        public readonly string ContainedType;

        public ReferenceType(string containedType) : this("@" + containedType, containedType) { }

        public ReferenceType(string name, string containedType) : base(name) {
            ContainedType = containedType;
        }

        protected bool Equals(ReferenceType other) {
            return Equals(ContainedType, other.ContainedType);
        }

        public override bool Equals(object? obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ReferenceType) obj);
        }

        public override int GetHashCode() {
            return (ContainedType != null ? ContainedType.GetHashCode() : 0);
        }
    }
}
