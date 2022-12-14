using System;
using comterpreter;

namespace lexi
{
    class Lexi {
        public static void Main(string[] args) {
            dynamic ast = Compiler.parse("program.slo");
            System.Console.WriteLine(ast.repr());
            // try {
            //     compile(args[0], File.ReadAllText(args[0]));
            // } catch (Exception) {
            //     while (true) {
            //         System.Console.Write("<lexi#>");
            //         string input = System.Console.ReadLine();
            //         System.Console.WriteLine();
            //         run("<stdin>", input);
            //     }
            // }
        }

        public static int run(string fn, string text) {
            Lexer lexer = new Lexer(fn, text);
            LexResult lexed = lexer.makeTokens();
            if (lexed.error is not null) {
                System.Console.WriteLine(lexed.error.repr()); 
                return 1;
            }

            foreach (Token i in (List<Token>) lexed.tok) {
                System.Console.WriteLine($"Token[ {i.repr()} ]");
            }

            System.Console.WriteLine();

            Parser parser = new Parser((List<Token>) lexed.tok);
            ParseResult ast = parser.parse();
            if (ast.error is not null) {
                System.Console.WriteLine(ast.error.repr());
                return 1;
            }
            
            System.Console.WriteLine();
            System.Console.WriteLine(ast.node.repr());
            System.Console.WriteLine();

            Context context = new Context("<program>");
            context.symbolTable = new SymbolTable();
            RTResult result = Interpreter.visit(ast.node, context);
            if (result.error is not null) {
                System.Console.WriteLine(result.error.repr());
                return 1;
            }

            // dynamic result = Compiler.visit(ast.node);

            System.Console.WriteLine();
            System.Console.WriteLine(result.value.repr());
            System.Console.WriteLine();

            return 0;
        }

        public static int compile(string fn, string text) {
            Lexer lexer = new Lexer(fn, text);
            LexResult lexed = lexer.makeTokens();
            if (lexed.error is not null) {
                System.Console.WriteLine(lexed.error.repr()); 
                return 1;
            }

            Parser parser = new Parser((List<Token>) lexed.tok);
            ParseResult ast = parser.parse();
            if (ast.error is not null) {
                System.Console.WriteLine(ast.error.repr());
                return 1;
            } Compiler.compile(fn, ast.node);

            return 0;
        }
    }
}