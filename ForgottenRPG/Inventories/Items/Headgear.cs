using System.Xml.Linq;
using ForgottenRPG.Service;

namespace ForgottenRPG.Inventories.Items {
    public class Headgear : AbstractItem, IWeapon {
        public float Defence { get; }

        public static IItem Load(int id, string name, string codename, string description, XElement modifiers,
                                 ITextureStore textureStore) {
            float.TryParse(modifiers.Element("Defence")?.Value ?? "0", out float defence);
            return new Headgear(id, name, codename, description, defence, textureStore);
        }

        private Headgear(int id, string name, string codename, string description, float defence,
                            ITextureStore textureStore)
            : base(id, name, description, codename, textureStore.GetNewSprite($"ui_item_{codename}"), 1) {
            Defence = defence;
        }
    }
}
