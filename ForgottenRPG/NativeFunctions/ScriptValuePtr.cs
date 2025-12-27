namespace ForgottenRPG.NativeFunctions {
    public readonly struct ScriptValuePtr<T> {
        public int Address { get; }

        public ScriptValuePtr(int address) {
            Address = address;
        }
    }
}
