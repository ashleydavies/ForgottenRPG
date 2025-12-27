using SFML.Graphics;

using ForgottenRPG.NativeFunctions;

namespace ForgottenRPG.Items {
    [ScriptReference("ItemHandle")]
    public interface IItem {
        int Id { get; }
        string Name { get; }
        string Codename { get; }
        string DisplayName { get; }
        string Description { get; }
        Sprite Texture { get; }
        int MaxStackSize { get; }
    }
}
