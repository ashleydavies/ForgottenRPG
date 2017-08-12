using System.Collections.Generic;
using ShaRPG.VM;

namespace ShaRPG.EntityDialog {
    public abstract class DialogAction {
        public abstract void Execute();
    }

    public class DialogActionChangeNode : DialogAction {
        private readonly int _id;

        public DialogActionChangeNode(int id) {
            _id = id;
        }

        public override void Execute() {
            throw new System.NotImplementedException();
        }
    }

    public class DialogActionEndDiscussion : DialogAction {
        public override void Execute() {
            throw new System.NotImplementedException();
        }
    }

    public class DialogActionCode : DialogAction {
        private readonly List<int> _code;

        public DialogActionCode(List<int> code) {
            _code = code;
            Execute();
        }

        public override void Execute() {
            new ScriptVM(_code).Execute();
        }
    }
}
