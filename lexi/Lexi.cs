using System;


namespace lexi
{
    class Lexi {
        public static void Main(string[] args) {
            run("<stdin>", "var answr = 37");
        }

        public static void run(string fn, string text) {
            Lexer lexer = new Lexer(fn, text);
            LexResult lexed = lexer.makeTokens();

            if (lexed.error is not null) System.Console.WriteLine(lexed.error.repr());

            foreach (Token i in (List<Token>) lexed.tok) {
                System.Console.WriteLine($"Token[ {i.repr()} ]");
            }
        }
    }
}