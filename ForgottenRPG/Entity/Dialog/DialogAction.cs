using System.Collections.Generic;
using ForgottenRPG.VM;

namespace ForgottenRPG.Entity.Dialog {
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
        private readonly List<uint> _code;

        public DialogActionCode(List<uint> code) {
            _code = code;
        }

        public override void Execute(Dialog dialog) {
            new ScriptVm(new List<uint>(_code)).Execute();
        }
    }
}
