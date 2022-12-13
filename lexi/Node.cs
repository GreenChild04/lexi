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

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1003;

        public NumberNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.tok = data.data[2];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(NumberNode.address) {
                this.posStart,
                this.posEnd,
                this.tok,
            };
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

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1004;

        public StringNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.tok = data.data[2];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(StringNode.address) {
                this.posStart,
                this.posEnd,
                this.tok,
            };
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

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1005;

        public UnaryOpNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.opTok = data.data[2];
            this.node = data.data[3];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(UnaryOpNode.address) {
                this.posStart,
                this.posEnd,
                this.opTok,
                this.node,
            };
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

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1006;

        public BinOpNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.leftNode = data.data[2];
            this.opTok = data.data[3];
            this.rightNode = data.data[4];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(BinOpNode.address) {
                this.posStart,
                this.posEnd,
                this.leftNode,
                this.opTok,
                this.rightNode,
            };
        }
    }

    public class IterNode: Node {
        public List<dynamic> elementNodes;

        public IterNode(List<dynamic> elementNodes, Position posStart, Position posEnd) {
            this.elementNodes = elementNodes;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public void inherit(IterNode other) {
            this.elementNodes = other.elementNodes;
            this.posStart = other.posStart;
            this.posEnd = other.posEnd;
        }

        public T to<T>() {
            return (T)Activator.CreateInstance(typeof(T), new object[] {this.elementNodes, this.posStart, this.posEnd});
        }

        public new string repr() {
            string str = "";
            for (int i = 0; i < this.elementNodes.Count; i++) {
                if (i == 0) str += ((dynamic) this.elementNodes[i]).repr();
                if (i != 0) str += ", " + ((dynamic) this.elementNodes[i]).repr();
            }
            return $"({str})";
        }

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1007;

        public IterNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.elementNodes = data.data[2];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(address) {
                this.posStart,
                this.posEnd,
                this.elementNodes,
            };
        }
    }

    public class TupleNode: IterNode {public TupleNode(List<dynamic> elementNodes, Position posStart, Position posEnd): base(elementNodes, posStart, posEnd) {} public TupleNode(SeraLib.SeraData data): base(data) {} public static new uint address = 1008; public new SeraLib.SeraBall seralib() {return new SeraLib.SeraBall(address) {this.posStart,this.posEnd,this.elementNodes,};}}
    public class ListNode: IterNode {public ListNode(List<dynamic> elementNodes, Position posStart, Position posEnd): base(elementNodes, posStart, posEnd) {} public new string repr() {string str = ""; for (int i = 0; i < this.elementNodes.Count; i++) {if (i == 0) str += ((dynamic) this.elementNodes[i]).repr(); if (i != 0) str += ", " + ((dynamic) this.elementNodes[i]).repr();} return $"[{str}]";} public ListNode(SeraLib.SeraData data): base(data) {} public static new uint address = 1009; public new SeraLib.SeraBall seralib() {return new SeraLib.SeraBall(address) {this.posStart,this.posEnd,this.elementNodes,};}}
    public class CurlNode: IterNode {public CurlNode(List<dynamic> elementNodes, Position posStart, Position posEnd): base(elementNodes, posStart, posEnd) {} public new string repr() {string str = ""; for (int i = 0; i < this.elementNodes.Count; i++) {if (i == 0) str += ((dynamic) this.elementNodes[i]).repr(); if (i != 0) str += "; " + ((dynamic) this.elementNodes[i]).repr();} return $"{{{str}}}";} public CurlNode(SeraLib.SeraData data): base(data) {} public static new uint address = 1010; public new SeraLib.SeraBall seralib() {return new SeraLib.SeraBall(address) {this.posStart,this.posEnd,this.elementNodes,};}}

    public class ConvNode: Node {
        public object nodeToConv;
        public List<dynamic> argNodes;

        public ConvNode(object nodeToConv, List<dynamic> argNodes, Position posStart, Position posEnd) {
            this.nodeToConv = nodeToConv;
            this.argNodes = argNodes;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

	    public new string repr() {
		    return $"ConvNode({((dynamic) this.nodeToConv).repr()})";
	    }

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1011;

        public ConvNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.nodeToConv = data.data[2];
            this.argNodes = data.data[3];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(address) {
                this.posStart,
                this.posEnd,
                this.nodeToConv,
                this.argNodes,
            };
        }
    }

    public class ListConvNode: ConvNode {public ListConvNode(object nodeToConv, List<dynamic> argNodes, Position posStart, Position posEnd): base(nodeToConv, argNodes, posStart, posEnd) {} public new static uint address = 1012; public new SeraLib.SeraBall seralib() {return new SeraLib.SeraBall(address) {this.posStart,this.posEnd,this.nodeToConv,this.argNodes,};}}
    public class TupleConvNode: ConvNode {public TupleConvNode(object nodeToConv, List<dynamic> argNodes, Position posStart, Position posEnd): base(nodeToConv, argNodes, posStart, posEnd) {} public new static uint address = 1013; public new SeraLib.SeraBall seralib() {return new SeraLib.SeraBall(address) {this.posStart,this.posEnd,this.nodeToConv,this.argNodes,};}}
    public class CurlConvNode: ConvNode {public CurlConvNode(object nodeToConv, List<dynamic> argNodes, Position posStart, Position posEnd): base(nodeToConv, argNodes, posStart, posEnd) {} public new static uint address = 1014; public new SeraLib.SeraBall seralib() {return new SeraLib.SeraBall(address) {this.posStart,this.posEnd,this.nodeToConv,this.argNodes,};}}

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

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1015;

        public GetNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.mainNode = data.data[2];
            this.subNode = data.data[3];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(GetNode.address) {
                this.posStart,
                this.posEnd,
                this.mainNode,
                this.subNode,
            };
        }
    }

    public class CallNode: Node {
        public dynamic nodeToCall;
        public Token identifier;
        public List<dynamic> args;

        public CallNode(dynamic nodeToCall, Token identifier, List<dynamic> args, Position posStart, Position posEnd) {
            this.nodeToCall = nodeToCall;
            this.identifier = identifier;
            this.args = args;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return (identifier is not null) ? $"CallNode({this.nodeToCall.repr()} -> {this.identifier.repr()})": $"CallNode({this.nodeToCall.repr()})";
        }

        /////////////////////
        // SeraLib Stuff
        /////////////////////
        
        public static uint address = 1016;

        public CallNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.identifier = data.data[2];
            this.args = data.data[3];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(CallNode.address) {
                this.posStart,
                this.posEnd,
                this.identifier,
                this.args,
            };
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

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1017;

        public VarDefNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.identifier = data.data[2];
            this.valueNode = data.data[3];
            this.isConst = bool.Parse(data.data[4]);
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(VarDefNode.address) {
                this.posStart,
                this.posEnd,
                this.identifier,
                this.valueNode,
                this.isConst.ToString(),
            };
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

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1018;

        public VarModifyNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.identifier = data.data[2];
            this.valueNode = data.data[3];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(VarModifyNode.address) {
                this.posStart,
                this.posEnd,
                this.identifier,
                this.valueNode,
            };
        }
    }

    public class LateDefNode: Node {
        public Token identifier;
        public List<dynamic> args;
        public CurlNode curlNode;

        public LateDefNode(Token identifier, List<dynamic> args, CurlNode curlNode, Position posStart, Position posEnd) {
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

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1019;

        public LateDefNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.identifier = data.data[2];
            this.args = data.data[3];
            this.curlNode = data.data[4];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(LateDefNode.address) {
                this.posStart,
                this.posEnd,
                this.identifier,
                this.args,
                this.curlNode,
            };
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

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1020;

        public WhileNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.condition = data.data[2];
            this.curlNode = data.data[3];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(WhileNode.address) {
                this.posStart,
                this.posEnd,
                this.condition,
                this.curlNode,
            };
        }
    }

    public class IfNode: Node {
        public List<dynamic> conditions;
        public List<dynamic> bodies;

        public IfNode(List<dynamic> conditions, List<dynamic> bodies, Position posStart, Position posEnd) {
            this.conditions = conditions;
            this.bodies = bodies;
            this.posStart = posStart;
            this.posEnd = posEnd;
        }

        public new string repr() {
            return $"IfNode({((dynamic) this.conditions[0]).repr()} => {((dynamic) this.bodies[0]).repr()})";
        }

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1021;

        public IfNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.conditions = data.data[2];
            this.bodies = data.data[3];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(IfNode.address) {
                this.posStart,
                this.posEnd,
                this.conditions,
                this.bodies,
            };
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

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1022;

        public ImportNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.name = data.data[2];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(ImportNode.address) {
                this.posStart,
                this.posEnd,
                this.name,
            };
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

        /////////////////////
        // SeraLib Stuff
        /////////////////////

        public static uint address = 1023;
        
        public ReturnNode(SeraLib.SeraData data) {
            this.posStart = data.data[0];
            this.posEnd = data.data[1];
            this.obj = data.data[2];
        }

        public SeraLib.SeraBall seralib() {
            return new SeraLib.SeraBall(ReturnNode.address) {
                this.posStart,
                this.posEnd,
                this.obj,
            };
        }
    }
}
