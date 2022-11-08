using System;


namespace lexi
{
    class Lexi {
        public static void Main(string[] args) {
            run("<stdin>", "23.5");
        }

        public static void run(string fn, string text) {
            Lexer lexer = new Lexer(fn, text);
            LexResult lexed = lexer.makeTokens();
            if (lexed.error is not null) System.Console.WriteLine(lexed.error.repr());

            Parser parser = new Parser((List<Token>) lexed.tok);
            ParseResult ast = parser.parse();
            if (ast.error is not null) System.Console.WriteLine(ast.error.repr());

            // foreach (Token i in (List<Token>) lexed.tok) {
            //     System.Console.WriteLine($"Token[ {i.repr()} ]");
            // }

            System.Console.WriteLine(ast.node.tok.repr());

            System.Console.WriteLine();
        }
    }
}