using SeraLib;
using lexi;
using System.Reflection;

namespace comterpreter
{
    public class Compiler {
        public static Dictionary<uint, Type> mapped = new Dictionary<uint, Type>() {
            {Header.address, typeof(Header)},
            {SeraDict.address, typeof(SeraDict)},
            {Token.address, typeof(Token)},
            {Position.address, typeof(Position)},
            {NumberNode.address, typeof(NumberNode)},
            {StringNode.address, typeof(StringNode)},
            {UnaryOpNode.address, typeof(UnaryOpNode)},
            {BinOpNode.address, typeof(BinOpNode)},
            {IterNode.address, typeof(IterNode)},
            {TupleNode.address, typeof(TupleNode)},
            {ListNode.address, typeof(ListNode)},
            {CurlNode.address, typeof(CurlNode)},
            {ListConvNode.address, typeof(ListConvNode)},
            {TupleConvNode.address, typeof(TupleConvNode)},
            {CurlConvNode.address, typeof(CurlConvNode)},
            {GetNode.address, typeof(GetNode)},
            {CallNode.address, typeof(CallNode)},
            {VarDefNode.address, typeof(VarDefNode)},
            {VarModifyNode.address, typeof(VarModifyNode)},
            {LateDefNode.address, typeof(LateDefNode)},
            {WhileNode.address, typeof(WhileNode)},
            {IfNode.address, typeof(IfNode)},
            {ImportNode.address, typeof(ImportNode)},
            {ReturnNode.address, typeof(ReturnNode)},
        };

        public static void compile(string fn, dynamic data) {
            string result = new Header(data).seralib().compile();
            File.WriteAllText(fn.Split('.')[0] + ".slo", result);
        }

        public static dynamic parse(string loc) {
            string text = File.ReadAllText(loc);
            SeraLib.Parser parser = new SeraLib.Parser(text, Compiler.mapped);
            return parser.parse().node;
        }
    }
}