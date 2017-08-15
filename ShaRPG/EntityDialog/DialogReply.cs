using System.Collections.Generic;

namespace ShaRPG.EntityDialog {
    public class DialogReply {
        public string Prompt { get; }
        private readonly List<DialogAction> _actions;
        
        public DialogReply(string prompt, List<DialogAction> actions) {
            _actions = actions;
            Prompt = prompt;
        }
    }
}
