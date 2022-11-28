using System.Collections;
using dom;
using lexi;
using System.Reflection;

namespace comterpreter
{
    public class Compiler {
        public static dynamic visit(dynamic node) {
            string methodName = $"visit_{node.GetType().Name}";
            MethodInfo method = typeof(Compiler).GetMethod(methodName);
            return method.Invoke(null, new object[] {node});     
        }

        public static Number visit_NumberNode(NumberNode node) {
            return new Number(node.tok, node.posStart, node.posEnd);
        }

        public static dom.String visit_StringNode(StringNode node) {
            return new dom.String(node.tok, node.posStart, node.posEnd);
        }

        public static BinOp visit_BinOpNode(BinOpNode node) {
            dynamic left = Compiler.visit(node.leftNode);
            dynamic right = Compiler.visit(node.rightNode);

            return new BinOp(left, right, node.opTok);
        }

        public static UnaryOp visit_UnaryOpNode(UnaryOpNode node) {
            dynamic right = Compiler.visit(node.node);
            return new UnaryOp(right, node.opTok, node.posStart, node.posEnd);
        }
    }
}