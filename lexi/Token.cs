using System;
using System.Collections;


namespace lexi
{
    public class Token {
        // Class
        public static string INT = "INT";
        public static string FLOAT = "FLOAT";
        public static string PLUS = "PLUS";
        public static string MINUS = "MINUS";
        public static string MUL = "MUL";
        public static string DIV = "DIV";
        public static string LPAREN = "LPAREN";
        public static string RPAREN = "RPAREN";
        public static string SET = "SET";
        public static string IDENTIFIER = "IDENTIFIER";
        public static string LSQUARE = "LSQUARE";
        public static string RSQUARE = "RSQUARE";
        public static string LCURL = "LCURL";
        public static string RCURL = "RCURL";
        public static string STR = "STR";
        public static string KEYWORD = "KEYWORD";
        public static string POW = "POW";
        public static string BLNK = "BLNK";
        public static string EQ = "EQ";
        public static string EE = "EE";
        public static string NE = "NE";
        public static string LT = "LT";
        public static string GT = "GT";
        public static string LTE = "LTE";
        public static string GTE = "GTE";
        public static string TET = "TET";
        public static string SEMI = "SEMI";
        public static string COMMA = "COMMA";
        public static string ARRW = "ARRW";
        public static string NSET = "NSET";
        public static string DOT = "DOT";
        public static string EOF = "EOF";

        public static string[] KEYWORDS = {
            "var",
            "if",
            "stru",
            "and",
            "or",
            "not",
            "elif",
            "else",
            "while",
            "fun",
            "return",
            "continue",
            "break",
        };

        // Object
        public string type;
        public object value;
        public Position posStart;
        public Position posEnd;

        public Token(string type, object value=null, Position posStart=null, Position posEnd=null) {
            this.type = type;
            this.value = value;

            if (posStart is not null) {
                this.posStart = posStart.copy();
                this.posEnd = posStart.copy();
                this.posEnd.advance();
            }

            if (posEnd is not null) {
                this.posEnd = posEnd;
            }
        }

        public bool matches(string type, object value) {
            return this.type == type && this.value == value;
        }

        public string repr() {
            if (this.value is not null) return $"{this.type}:{this.value}";
            return $"{this.type}";
        }

        // For Lexer Shortening
        public static void Lexer_CharTok(string tokenChar, string tokenType, Lexer lexer, List<Token> tokenList) {
            Token.Lexer_AppendTok(lexer.currentChar == tokenChar, lexer, tokenType, tokenList);
        }

        public static LexResult Lexer_FuncTok(bool condition, Func<LexResult> fun, List<Token> tokenList, Lexer lexer) {
            if (condition) {
                lexer.res.found = true;
                LexResult res = new LexResult();
                LexResult output = fun();

                Token tok = (Token) res.register(output);
                if (res.error is not null) return res;

                tokenList.Add(tok);

                return res;
            }
            return new LexResult();
        }

        public static void Lexer_AppendTok(bool req, Lexer lexer, string tokenType, List<Token> tokenList) {
            if (req) {
                lexer.res.found = true;
                tokenList.Add(new Token(tokenType, posStart: lexer.pos));
                lexer.advance();
            }
        }
    }
}