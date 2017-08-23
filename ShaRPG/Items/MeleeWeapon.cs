using System.Xml.Linq;
using ShaRPG.Entity;

namespace ShaRPG.Items {
    public class MeleeWeapon : AbstractItem, IWeapon {
        public int Damage { get; }

        public static IItem Load(int id, string name, string codename, string description, XElement modifiers) {
            int.TryParse(modifiers.Element("Damage")?.Value ?? "0", out int damage);
            return new MeleeWeapon(id, name, codename, description, damage);
        }

        private MeleeWeapon(int id, string name, string codename, string description, int damage)
            : base(id, name, description, codename) {
            Damage = damage;
        }
    }
}
