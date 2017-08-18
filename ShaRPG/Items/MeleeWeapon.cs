using System.Xml.Linq;
using ShaRPG.Entity;

namespace ShaRPG.Items {
    public class MeleeWeapon : AbstractItem, IWeapon {
        public int Damage { get; }

        public static IItem LoadItem(int id, string name, string description, string graphic, XElement modifiers) {
            int.TryParse(modifiers.Element("Damage")?.Value ?? "0", out int damage);
            
            return new MeleeWeapon(id, name, description, graphic, damage);
        }

        private MeleeWeapon(int id, string name, string description, string graphic, int damage)
            : base(id, name, description, graphic) {
            Damage = damage;
        }
    }
}
