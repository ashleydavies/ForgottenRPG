namespace ScriptCompiler.Types {
    public class ArrayType : SType {
        public readonly SType ContainedType;
        
        public ArrayType(SType containedType) {
            ContainedType = containedType;
        }

        protected bool Equals(ArrayType other) {
            return Equals(ContainedType, other.ContainedType);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArrayType) obj);
        }

        public override int GetHashCode() {
            return (ContainedType != null ? ContainedType.GetHashCode() : 0);
        }
    }
}
