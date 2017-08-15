using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ShaRPG.EntityDialog {
    public class Dialog {
        public string Name => "Farmer Joe";
        public string Prompt => _dialogNodes[_currentNode].Prompt;
        public List<string> Replies => _dialogNodes[_currentNode].Replies;
        public string Character => "ui_avatar_farmer_joe";
        private readonly Dictionary<int, DialogNode> _dialogNodes;
        private readonly IOpenDialog _dialogOpener;
        private int _currentNode = 0;

        public Dialog(IOpenDialog dialogOpener, Dictionary<int, DialogNode> dialogNodes) {
            _dialogOpener = dialogOpener;
            _dialogNodes = dialogNodes;
        }

        public void StartDialog() {
            _dialogOpener.StartDialog(this);
        }

        public static Dialog FromXElement(XElement dialog, IOpenDialog dialogOpener) {
            Dictionary<int, DialogReply> replies = new Dictionary<int, DialogReply>();
            Dictionary<int, DialogNode> nodes = new Dictionary<int, DialogNode>();

            foreach (XElement node in dialog.XPathSelectElements("./Nodes/Node")) {
                int nodeId = int.Parse(node.Attribute("id")?.Value);
                List<DialogReply> nodeReplies = new List<DialogReply>();

                foreach (XElement nodeReply in node.Elements("Reply")) {
                    int replyId;
                    if (!int.TryParse(nodeReply.Attribute("id")?.Value, out replyId))
                        throw new DialogException("Unspecified ID in node's reply reference");

                    if (!replies.ContainsKey(replyId)) {
                        XElement reply = dialog.XPathSelectElement($"./Replies/Reply[@id='{replyId}']");
                        replies.Add(replyId, LoadReply(reply));
                    }
                    
                    nodeReplies.Add(replies[replyId]);
                }
                
                nodes.Add(nodeId, new DialogNode(nodeReplies, node.Value.Trim()));
            }
            
            return new Dialog(dialogOpener, nodes);
        }

        private static DialogReply LoadReply(XElement reply) {
            string prompt = reply.Value.Trim();
            List<DialogAction> replyActions = new List<DialogAction>();
            
            foreach (XElement replyAction in reply.Elements("Action")) {
                switch (replyAction.Attribute("type")?.Value) {
                    case "changeNode":
                        replyActions.Add(new DialogActionChangeNode(int.Parse(replyAction.Attribute("id").Value)));
                        break;
                    case "endDiscussion":
                        replyAction.Add(new DialogActionEndDiscussion());
                        if (string.IsNullOrEmpty(prompt)) prompt = "End discussion";
                        break;
                    case "code":
                        replyAction.Add(new DialogActionCode(replyAction.Value.Split(',').Select(int.Parse).ToList()));
                        break;
                }
            }
            
            return new DialogReply(prompt, replyActions);
        }
    }

    public class DialogException : Exception {
        public DialogException(string message) : base(message) { }
    }
}
