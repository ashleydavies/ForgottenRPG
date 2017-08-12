using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ShaRPG.EntityDialog {
    public class DialogNode {
        public string Prompt { get; }
        private readonly List<DialogReply> _replies;
        
        public DialogNode(List<DialogReply> replies, string prompt) {
            _replies = replies;
            Prompt = prompt;
        }
    }
}
