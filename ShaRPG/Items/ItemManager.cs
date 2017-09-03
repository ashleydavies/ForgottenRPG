using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ShaRPG.Service;

namespace ShaRPG.Items {
    public class ItemManager {
        public const int SpriteSizeX = 32;
        public const int SpriteSizeY = 32;

        private readonly Dictionary<int, IItem> _itemDictionary = new Dictionary<int, IItem>();

        public IItem GetItem(int id) => _itemDictionary[id];
        public IItem GetItem(string codename) => _itemDictionary.Values.FirstOrDefault(i => i.Codename == codename);

        public ItemManager(string path, ITextureStore textureStore) {
            XDocument document = XDocument.Load(Path.Combine(path, "ItemData.xml"));
            foreach (XElement itemElement in document.Descendants("Item")) {
                int.TryParse(itemElement.Attribute("Id").Value, out int id);
                string name = itemElement.Attribute("Name").Value;
                string type = itemElement.Attribute("Type").Value;
                string codename = itemElement.Attribute("Codename").Value;
                string description = itemElement.Element("Description").Value;

                if (type == "MeleeWeapon") {
                    _itemDictionary.Add(id, MeleeWeapon.Load(id, name, codename, description,
                                                                 itemElement.Element("Modifiers"), textureStore));
                }
            }
        }
    }
}
