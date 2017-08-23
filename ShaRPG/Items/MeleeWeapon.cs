using System.Xml.Linq;
using ShaRPG.Entity;
using ShaRPG.Service;

namespace ShaRPG.Items {
    public class MeleeWeapon : AbstractItem, IWeapon {
        public int Damage { get; }

        public static IItem Load(int id, string name, string codename, string description, XElement modifiers,
                                 ISpriteStoreService spriteStore) {
            int.TryParse(modifiers.Element("Damage")?.Value ?? "0", out int damage);
            return new MeleeWeapon(id, name, codename, description, damage, spriteStore);
        }

        private MeleeWeapon(int id, string name, string codename, string description, int damage,
                            ISpriteStoreService spriteStore)
            : base(id, name, description, codename, spriteStore.GetSprite($"ui_item_{codename}")) {
            Damage = damage;
        }
    }
}
