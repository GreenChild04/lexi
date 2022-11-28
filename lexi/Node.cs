using System.Collections;

namespace lexi
{
    public class Node {
        public Position posStart;
        public Position posEnd;

        public string repr() {
            return "NODE";
        }
    }

    public class NumberNode: Node {
        public Token tok;

        public NumberNode(Token tok, Position posStart, Position posEnd) {
            this.tok = tok;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return $"{this.tok.repr()}";
        }
    }

    public class StringNode: Node {
        public Token tok;

        public StringNode(Token tok, Position posStart, Position posEnd) {
            this.tok = tok;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return $"{this.tok.repr()}";
        }
    }

    public class UnaryOpNode: Node {
        public Token opTok;
        public dynamic node;

        public UnaryOpNode(Token opTok, dynamic node, Position posStart, Position posEnd) {
            this.opTok = opTok;
            this.node = node;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return $"{this.opTok.repr()}({node.repr()})";
        }
    }

    public class BinOpNode: Node {
        public dynamic leftNode;
        public Token opTok;
        public dynamic rightNode;

        public BinOpNode(dynamic leftNode, Token opTok, dynamic rightNode, Position posStart, Position posEnd) {
            this.leftNode = leftNode;
            this.opTok = opTok;
            this.rightNode = rightNode;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return $"({this.leftNode.repr()}) {this.opTok.repr()} ({this.rightNode.repr()})";
        }
    }

    public class IterNode: Node {
        public ArrayList elementNodes;

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

        public new string repr() {
            string str = "";
            for (int i = 0; i < this.elementNodes.Count; i++) {
                if (i == 0) str += ((dynamic) this.elementNodes[i]).repr();
                if (i != 0) str += ", " + ((dynamic) this.elementNodes[i]).repr();
            }
            return $"({str})";
        }
    }

    public class TupleNode: IterNode {public TupleNode(ArrayList elementNodes, Position posStart, Position posEnd): base(elementNodes, posStart, posEnd) {}}
    public class ListNode: IterNode {public ListNode(ArrayList elementNodes, Position posStart, Position posEnd): base(elementNodes, posStart, posEnd) {} public new string repr() {string str = ""; for (int i = 0; i < this.elementNodes.Count; i++) {if (i == 0) str += ((dynamic) this.elementNodes[i]).repr(); if (i != 0) str += ", " + ((dynamic) this.elementNodes[i]).repr();} return $"[{str}]";}}
    public class CurlNode: IterNode {public CurlNode(ArrayList elementNodes, Position posStart, Position posEnd): base(elementNodes, posStart, posEnd) {} public new string repr() {string str = ""; for (int i = 0; i < this.elementNodes.Count; i++) {if (i == 0) str += ((dynamic) this.elementNodes[i]).repr(); if (i != 0) str += "; " + ((dynamic) this.elementNodes[i]).repr();} return $"{{{str}}}";}}

    public class ConvNode: Node {
        public object nodeToConv;
        public ArrayList argNodes;

        public ConvNode(object nodeToConv, ArrayList argNodes, Position posStart, Position posEnd) {
            this.nodeToConv = nodeToConv;
            this.argNodes = argNodes;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

	    public new string repr() {
		    return $"ConvNode({((dynamic) this.nodeToConv).repr()})";
	    }
    }

    public class ListConvNode: ConvNode {public ListConvNode(object nodeToConv, ArrayList argNodes, Position posStart, Position posEnd): base(nodeToConv, argNodes, posStart, posEnd) {}}
    public class TupleConvNode: ConvNode {public TupleConvNode(object nodeToConv, ArrayList argNodes, Position posStart, Position posEnd): base(nodeToConv, argNodes, posStart, posEnd) {}}
    public class CurlConvNode: ConvNode {public CurlConvNode(object nodeToConv, ArrayList argNodes, Position posStart, Position posEnd): base(nodeToConv, argNodes, posStart, posEnd) {}}

    public class GetNode: Node{
        public dynamic mainNode;
        public dynamic subNode;

        public GetNode(dynamic mainTok, dynamic subTok,  Position posStart, Position posEnd) {
            this.mainNode = mainTok;
            this.subNode = subTok;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return subNode is not null ? $"{mainNode.repr()}.{subNode.repr()}": $"{mainNode.repr()}";
        }
    }

    public class CallNode: Node {
        public dynamic nodeToCall;
        public Token identifier;
        public ArrayList args;

        public CallNode(dynamic nodeToCall, Token identifier, ArrayList args, Position posStart, Position posEnd) {
            this.nodeToCall = nodeToCall;
            this.identifier = identifier;
            this.args = args;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return (identifier is not null) ? $"CallNode({this.nodeToCall.repr()} -> {this.identifier.repr()})": $"CallNode({this.nodeToCall.repr()})";
        }
    }

    public class VarDefNode: Node {
        public Token identifier;
        public dynamic valueNode;
        public bool isConst;

        public VarDefNode(Token identifier, dynamic valueNode, bool isConst, Position posStart, Position posEnd) {
            this.identifier = identifier;
            this.valueNode = valueNode;
            this.isConst = isConst;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return identifier is not null && !isConst ? $"NewVar({this.identifier.value})": identifier is not null ? $"NewConst({this.identifier.value})": !isConst ? "NewConst": "NewVar";
        }
    }

    public class VarModifyNode: Node {
        public dynamic identifier;
        public dynamic valueNode;

        public VarModifyNode(dynamic identifier, dynamic valueNode, Position posStart, Position posEnd) {
            this.identifier = identifier;
            this.valueNode = valueNode;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return $"ModifyVar({this.identifier.repr()})";
        }
    }

    public class LateDefNode: Node {
        public Token identifier;
        public List<Token> args;
        public CurlNode curlNode;

        public LateDefNode(Token identifier, List<Token> args, CurlNode curlNode, Position posStart, Position posEnd) {
            this.identifier = identifier;
            this.args = args;
            this.curlNode = curlNode;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            ArrayList data = new ArrayList() {this.identifier, this.curlNode};
            string result = data switch
            {
                ArrayList a when a[0] is not null && a[1] is not null => $"Late({this.identifier.value} => {this.curlNode.repr()})",
                ArrayList a when a[0] is not null => $"Late({this.identifier.value})",
                ArrayList a when a[1] is not null => $"Late(<anonymous> => {this.curlNode.repr()})",
                _ => "Late(<anonymous>)",
            }; return result;
        }
    }

    public class WhileNode: Node {
        public dynamic condition;
        public CurlNode curlNode;

        public WhileNode(dynamic condition, CurlNode curlNode, Position posStart, Position posEnd) {
            this.condition = condition;
            this.curlNode = curlNode;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return $"WhileNode({this.condition.repr()} => {this.curlNode.repr()})";
        }
    }

    public class IfNode: Node {
        public ArrayList conditions;
        public List<CurlNode> bodies;

        public IfNode(ArrayList conditions, List<CurlNode> bodies, Position posStart, Position posEnd) {
            this.conditions = conditions;
            this.bodies = bodies;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return $"IfNode({((dynamic) this.conditions[0]).repr()} => {((dynamic) this.bodies[0]).repr()})";
        }
    }

    public class ImportNode: Node {
        public GetNode name;

        public ImportNode(GetNode name, Position posStart, Position posEnd) {
            this.name = name;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return $"UseImport(File: {this.name.repr()})";
        }
    }

    public class ReturnNode: Node {
        public dynamic obj;

        public ReturnNode(dynamic obj, Position posStart, Position posEnd) {
            this.obj = obj;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return $"ReturnNode({this.obj.repr()})";
        }
    }
}
