using System.Collections.Generic;
using System.Xml.Linq;

namespace ShaRPG.EntityDialog {
    public class DialogNode {
        public List<DialogReply> Replies { get; set; } = new List<DialogReply>();
    }
}
