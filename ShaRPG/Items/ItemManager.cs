using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Xml.Linq;

namespace ShaRPG.Items {
    public class ItemManager {
        private readonly Dictionary<int, IItem> _itemDictionary = new Dictionary<int, IItem>();

        public IItem GetItem(int id) => _itemDictionary[id];

        public ItemManager(string path) {
            XDocument document = XDocument.Load(Path.Combine(path, "ItemData.xml"));
            foreach (XElement itemElement in document.Descendants("Item")) {
                int.TryParse(itemElement.Attribute("Id").Value, out int id);
                string name = itemElement.Attribute("Name").Value;
                string type = itemElement.Attribute("Type").Value;
                string graphic = itemElement.Attribute("Graphic").Value;
                string description = itemElement.Element("Description").Value;

                if (type == "MeleeWeapon") {
                    _itemDictionary.Add(id, MeleeWeapon.LoadItem(id, name, description,
                                                                 graphic, itemElement.Element("Modifiers")));
                }
            }
        }
    }
}
