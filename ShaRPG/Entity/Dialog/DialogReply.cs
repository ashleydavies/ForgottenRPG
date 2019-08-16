using System.Collections.Generic;

namespace ShaRPG.Entity.Dialog {
    public class DialogReply {
        public string Prompt { get; }
        private readonly List<DialogAction> _actions;
        
        public DialogReply(string prompt, List<DialogAction> actions) {
            _actions = actions;
            Prompt = prompt;
        }

        public void Actioned(Dialog dialog) {
            _actions.ForEach(a => a.Execute(dialog));
        }
    }
}
