namespace ScriptCompiler.Types {
    public class ReferenceType : SType {
        public readonly SType ContainedType;
        
        public ReferenceType(SType containedType) {
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
