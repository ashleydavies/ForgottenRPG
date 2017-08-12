using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ShaRPG.EntityDialog {
    public class Dialog {
        private readonly Dictionary<int, DialogNode> _dialogNodes;
        
        public Dialog(Dictionary<int, DialogNode> dialogNodes) {
            _dialogNodes = dialogNodes;
        }

        public static Dialog FromXElement(XElement dialog) {
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
            
            return new Dialog(nodes);
        }

        private static DialogReply LoadReply(XElement reply) {
            List<DialogAction> replyActions = new List<DialogAction>();
            
            foreach (XElement replyAction in reply.Elements("Action")) {
                switch (replyAction.Attribute("type")?.Value) {
                    case "changeNode":
                        replyActions.Add(new DialogActionChangeNode(int.Parse(replyAction.Attribute("id").Value)));
                        break;
                    case "endDiscussion":
                        replyAction.Add(new DialogActionEndDiscussion());
                        break;
                    case "code":
                        replyAction.Add(new DialogActionCode(replyAction.Value.Split(',').Select(int.Parse).ToList()));
                        break;
                }
            }
            
            return new DialogReply(replyActions);
        }
    }

    public class DialogException : Exception {
        public DialogException(string message) : base(message) { }
    }
}
