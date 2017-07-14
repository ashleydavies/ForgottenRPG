using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ShaRPG.EntityDialog {
    public class Dialog {
        private readonly List<DialogNode> _dialogNodes;
        
        public Dialog(List<DialogNode> dialogNodes) {
            _dialogNodes = dialogNodes;
        }

        public static Dialog FromXElement(XElement dialog) {
            Dictionary<int, DialogReply> replies = new Dictionary<int, DialogReply>();
            Dictionary<int, DialogNode> nodes = new Dictionary<int, DialogNode>();

            foreach (XElement node in dialog.XPathSelectElements("./Nodes/Node")) {
                DialogNode dialogNode = new DialogNode();

                foreach (XElement nodeReply in node.Elements("Reply")) {
                    int replyId;
                    if (!int.TryParse(nodeReply.Attribute("id")?.Value, out replyId))
                        throw new DialogException("Unspecified ID in node's reply reference");

                    if (!replies.ContainsKey(replyId)) {
                        
                    }
                    
                    dialogNode.Replies.Add(replies[replyId]);
                }
            }
        }
    }

    public class DialogException : Exception {
        public DialogException(string message) : base(message) { }
    }
}
