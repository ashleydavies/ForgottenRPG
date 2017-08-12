using System.Collections.Generic;
using System.Xml.Linq;

namespace ShaRPG.EntityDialog {
    public class DialogNode {
        private readonly List<DialogReply> _replies;
        
        public DialogNode(List<DialogReply> replies) {
            _replies = replies;
        }
    }
}
