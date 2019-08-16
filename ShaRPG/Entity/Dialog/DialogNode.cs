using System.Collections.Generic;
using System.Linq;

namespace ShaRPG.Entity.Dialog {
    public class DialogNode {
        public string Prompt { get; }
        public List<string> Replies => _replies.Select(x => x.Prompt).ToList();
        private readonly List<DialogReply> _replies;
        
        public DialogNode(List<DialogReply> replies, string prompt) {
            _replies = replies;
            Prompt = prompt;
        }

        public void ReplyActioned(Dialog dialog, int index) {
            _replies[index].Actioned(dialog);
        }
    }
}
