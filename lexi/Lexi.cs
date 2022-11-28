using System;
using comterpreter;

namespace lexi
{
    class Lexi {
        public static void Main(string[] args) {
            while (true) {
                System.Console.Write("<lexi#>");
                string input = System.Console.ReadLine();
                System.Console.WriteLine();
                run("<stdin>", input);
            }
        }

        public static object run(string fn, string text) {
            Lexer lexer = new Lexer(fn, text);
            LexResult lexed = lexer.makeTokens();
            if (lexed.error is not null) {
                System.Console.WriteLine(lexed.error.repr()); 
                return null;
            }

            foreach (Token i in (List<Token>) lexed.tok) {
                System.Console.WriteLine($"Token[ {i.repr()} ]");
            }

            System.Console.WriteLine();

            Parser parser = new Parser((List<Token>) lexed.tok);
            ParseResult ast = parser.parse();
            if (ast.error is not null) {
                System.Console.WriteLine(ast.error.repr());
                return null;
            }

            System.Console.WriteLine();
            System.Console.WriteLine(ast.node.repr());
            System.Console.WriteLine();

            // Context context = new Context("<program>");
            // context.symbolTable = new SymbolTable();
            // RTResult result = Interpreter.visit(ast.node, context);
            // if (result.error is not null) {
            //     System.Console.WriteLine(result.error.repr());
            //     return null;
            // }

            // dynamic result = Compiler.visit(ast.node);

            // System.Console.WriteLine();
            // System.Console.WriteLine(result.value.repr());
            // System.Console.WriteLine();

            return null;
        }
    }
}