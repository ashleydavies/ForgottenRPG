using System.Collections.Generic;
using ShaRPG.VM;

namespace ShaRPG.EntityDialog {
    public abstract class DialogAction {
        public abstract void Execute(Dialog dialog);
    }

    public class DialogActionChangeNode : DialogAction {
        private readonly int _id;

        public DialogActionChangeNode(int id) {
            _id = id;
        }

        public override void Execute(Dialog dialog) {
            dialog.ChangeNode(_id);
        }
    }

    public class DialogActionEndDiscussion : DialogAction {
        public override void Execute(Dialog dialog) {
            dialog.EndDialog();
        }
    }

    public class DialogActionCode : DialogAction {
        private readonly List<int> _code;

        public DialogActionCode(List<int> code) {
            _code = code;
        }

        public override void Execute(Dialog dialog) {
            new ScriptVM(new List<int>(_code)).Execute();
        }
    }
}
