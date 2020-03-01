namespace ScriptCompiler.Parsing {
    public enum Precedence {
        None,
        Assignment,
        Or,
        And,
        Equality,
        Comparison,
        Term,
        Factor,
        Accessor,
        Unary,
        Call,
        Max
    }
}
