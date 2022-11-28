using System.Reflection;

namespace lexi
{
    public static class Interpreter {
        public static RTResult visit(dynamic node, object context) {
            string methodName = $"visit_{node.GetType().Name}";
            RTResult res;

            // try {
                MethodInfo method = typeof(Interpreter).GetMethod(methodName);
                res = (RTResult) method.Invoke(null, new object[] {node, context});
            // } catch {
                // throw new Exception($"No method '{methodName}' defined (Interpreter Visit)");
            // }

            return res;
        }

        ///////////////////////
        // Visit Methods
        ///////////////////////

        public static RTResult visit_NumberNode(NumberNode node, Context context) {
            return new RTResult().success(
                new late.primative._number.Number(node.tok.value).setContext(context).setPos(node.posStart, node.posEnd)
            );
        }

        public static RTResult visit_StringNode(StringNode node, Context context) {
            return new RTResult().success(
                new late.primative._string.String(node.tok.value).setContext(context).setPos(node.posStart, node.posEnd)
            );
        }

        public static RTResult visit_BinOpNode(BinOpNode node, Context context) {
            RTResult res = new RTResult();

            dynamic left = res.register(Interpreter.visit(node.leftNode, context));
            if (res.error is not null) return res;
            dynamic right = res.register(Interpreter.visit(node.rightNode, context));
            if (res.error is not null) return res;

            if (node.opTok.type == Token.PLUS) res = left.BinOp_addedTo(right);
            else if (node.opTok.type == Token.MINUS) res = left.BinOp_subbedBy(right);
            else if (node.opTok.type == Token.MUL) res = left.BinOp_multedBy(right);
            else if (node.opTok.type == Token.DIV) res = left.BinOp_divedBy(right);
            else if (node.opTok.type == Token.POW) res = left.BinOp_powedBy(right);
            else if (node.opTok.type == Token.TET) res = left.BinOp_tetedBy(right);
            if (res.error is not null) return res;

            if (res.value is null && res.error is null)
                throw new Exception($"'BinOp for {node.opTok.type}' was not defined or run");

            return res.success(res.value.setPos(node.posStart, node.posEnd));
        }

        public static RTResult visit_UnaryOpNode(UnaryOpNode node, Context context) {
            RTResult res = new RTResult();
            dynamic right = res.register(Interpreter.visit(node.node, context));
            if (res.error is not null) return res;

            if (node.opTok.type == Token.MINUS) {
                right = res.register(right.BinOp_multedBy(new late.primative._number.Number(-1)));
            } else if (node.opTok.matches(Token.KEYWORD, "not")) {
                right = res.register(right.UnaryOp_notted());
            }

            if (res.error is not null) return res;
            return res.success(right.setPos(node.posStart, node.posEnd).setContext(context));
        }

        public static RTResult visit_TupleNode(TupleNode node, Context context) {
            RTResult res = new RTResult();
            List<dynamic> elements = new List<dynamic>();

            for (int i = 0; i < node.elementNodes.Count; i++) {
                elements.Add(res.register(Interpreter.visit(node.elementNodes[i], context)));
                if (res.error is not null) return res;
            }

            return res.success(new late.primative._iteration.Tuple(elements).setContext(context).setPos(node.posStart, node.posEnd));
        }
    }
}