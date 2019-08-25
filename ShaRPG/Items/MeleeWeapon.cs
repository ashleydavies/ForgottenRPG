using System.Xml.Linq;
using ShaRPG.Service;

namespace ShaRPG.Items {
    public class MeleeWeapon : AbstractItem, IWeapon {
        public int Damage { get; }

        public static IItem Load(int id, string name, string codename, string description, XElement modifiers,
                                 ITextureStore textureStore) {
            int.TryParse(modifiers.Element("Damage")?.Value ?? "0", out int damage);
            return new MeleeWeapon(id, name, codename, description, damage, textureStore);
        }

        private MeleeWeapon(int id, string name, string codename, string description, int damage,
                            ITextureStore textureStore)
            : base(id, name, description, codename, textureStore.GetNewSprite($"ui_item_{codename}"), 1) {
            Damage = damage;
        }
    }
}
