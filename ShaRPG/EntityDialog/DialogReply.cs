using System.Collections.Generic;

namespace ShaRPG.EntityDialog {
    public class DialogReply {
        private readonly List<DialogAction> _actions;
        
        public DialogReply(List<DialogAction> actions) {
            _actions = actions;
        }
    }
}
