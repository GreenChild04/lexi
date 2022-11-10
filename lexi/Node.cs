using System.Collections;

namespace lexi
{
    public class NumberNode {
        public Token tok;
        public Position posStart;
        public Position posEnd;

        public NumberNode(Token tok, Position posStart, Position posEnd) {
            this.tok = tok;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public string repr() {
            return $"{this.tok.repr()}";
        }
    }

    public class StringNode {
        public Token tok;
        public Position posStart;
        public Position posEnd;

        public StringNode(Token tok, Position posStart, Position posEnd) {
            this.tok = tok;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public string repr() {
            return $"{this.tok.repr()}";
        }
    }

    public class UnaryOpNode {
        public Token opTok;
        public dynamic node;
        public Position posStart;
        public Position posEnd;

        public UnaryOpNode(Token opTok, dynamic node, Position posStart, Position posEnd) {
            this.opTok = opTok;
            this.node = node;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public string repr() {
            return $"{this.opTok.repr()}({node.repr()})";
        }
    }

    public class BinOpNode {
        public dynamic leftNode;
        public Token opTok;
        public dynamic rightNode;
        public Position posStart;
        public Position posEnd;

        public BinOpNode(dynamic leftNode, Token opTok, dynamic rightNode, Position posStart, Position posEnd) {
            this.leftNode = leftNode;
            this.opTok = opTok;
            this.rightNode = rightNode;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public string repr() {
            return $"({this.leftNode.repr()}) {this.opTok.repr()} ({this.rightNode.repr()})";
        }
    }

    public class IterNode {
        public ArrayList elementNodes;
        public Position posStart;
        public Position posEnd;

        public IterNode(ArrayList elementNodes, Position posStart, Position posEnd) {
            this.elementNodes = elementNodes;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public void inherit(IterNode other) {
            this.elementNodes = other.elementNodes;
            this.posStart = other.posStart;
            this.posEnd = other.posEnd;
        }

        public string repr() {
            string str = "";
            for (int i = 0; i < this.elementNodes.Count; i++) {
                if (i == 0) str += ((dynamic) this.elementNodes[i]).repr();
                if (i != 0) str += ", " + ((dynamic) this.elementNodes[i]).repr();
            }
            return $"{str}";
        }
    }
}