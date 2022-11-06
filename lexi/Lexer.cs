using System;
using System.Collections;


namespace lexi
{
    // Lexer Result Storage
    public class LexResult {
        public Error error = null;
        public object tok = null;
        public bool found = false;
        
        public object register(LexResult res) {
            if (res.error is not null) this.error = res.error;
            return res.tok;
        }

        public LexResult success(object node) {
            this.tok = node;
            return this;
        }

        public LexResult failure(Error error) {
            if (this.error is null) this.error = error;
            return this;
        }
    }

    // Lexer Class
    public class Lexer {
        // Class
        public static string DIGITS = "0123456789";
        public static string LETTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
        public static string LETTERS_DIGITS = Lexer.DIGITS + Lexer.LETTERS;

        // Object
        public string text;
        public Position pos;
        public string currentChar;
        public LexResult res = new LexResult();

        public Lexer(string fn, string text) {
            this.text = text;
            this.pos = new Position(-1, 0, -1, fn, text);
            this.currentChar = null;
            this.advance();
        }

        public void advance() {
            this.pos.advance(this.currentChar);
            this.currentChar = this.pos.idx < this.text.Length ? this.text[this.pos.idx].ToString(): null;
        }

        public LexResult makeTokens() {
            List<Token> tokens = new List<Token>();

            while (this.currentChar is not null) {
                // Skippable characters
                if (this.currentChar is null) break;
                if (" \t\n".Contains(this.currentChar)) this.advance();

                // Single Character Tokens
                if (this.currentChar is null) break;
                Token.Lexer_CharTok("+", Token.PLUS, this, tokens);
                Token.Lexer_CharTok("*", Token.MUL, this, tokens);
                Token.Lexer_CharTok("(", Token.LPAREN, this, tokens);
                Token.Lexer_CharTok(")", Token.RPAREN, this, tokens);
                Token.Lexer_CharTok(",", Token.COMMA, this, tokens);
                Token.Lexer_CharTok(";", Token.SEMI, this, tokens);
                Token.Lexer_CharTok("[", Token.LSQUARE, this, tokens);
                Token.Lexer_CharTok("]", Token.RSQUARE, this, tokens);
                Token.Lexer_CharTok("{", Token.LCURL, this, tokens);
                Token.Lexer_CharTok("}", Token.RCURL, this, tokens);
                Token.Lexer_CharTok(".", Token.DOT, this, tokens);

                // Generated Digit & String Tokens
                if (this.currentChar is null) break;
                this.res.register(Token.Lexer_FuncTok(Lexer.DIGITS.Contains(this.currentChar), this.makeNum, tokens, this));
                if (this.currentChar is null) break;
                this.res.register(Token.Lexer_FuncTok(Lexer.LETTERS.Contains(this.currentChar), this.makeIdentifier, tokens, this));

                // Generated Symbol Tokens
                this.res.register(Token.Lexer_FuncTok(this.currentChar == "\"", this.makeStr, tokens, this));
                this.res.register(Token.Lexer_FuncTok(this.currentChar == "/", this.makeFSlash, tokens, this));
                this.res.register(Token.Lexer_FuncTok(this.currentChar == ":", this.makeColon, tokens, this));
                this.res.register(Token.Lexer_FuncTok(this.currentChar == "!", this.makeNotEquals, tokens, this));
                this.res.register(Token.Lexer_FuncTok(this.currentChar == "=", this.makeEquals, tokens, this));
                this.res.register(Token.Lexer_FuncTok(this.currentChar == "-", this.makeDash, tokens, this));
                this.res.register(Token.Lexer_FuncTok(this.currentChar == "^", this.makePower, tokens, this));
                this.res.register(Token.Lexer_FuncTok(this.currentChar == "<", this.makeLesserThan, tokens, this));
                this.res.register(Token.Lexer_FuncTok(this.currentChar == ">", this.makeGreaterThan, tokens, this));

                // Invalid Character Error
                if (!(res.found)) {
                    Position posStart = this.pos.copy();
                    string currentChar = this.currentChar;
                    this.advance();

                    res.failure(new Error(
                        posStart, this.pos,
                        Error.InvalidCharError,
                        $"'{currentChar}'"
                    ));
                } res.found = false;
            }

            Token.Lexer_AppendTok(true, this, Token.EOF, tokens);
            return res.success(tokens);
        }

        // Token Making Functions
        public LexResult makeNum() {
            LexResult res = new LexResult();
            string numStr = "";
            byte dotCount = 0;
            Position posStart = this.pos.copy();

            while (this.currentChar is not null && (Lexer.DIGITS + ".").Contains(this.currentChar)) {
                if (this.currentChar == ".") {
                    if (dotCount == 1) break;
                    dotCount += 1;
                    numStr += ".";
                } else numStr += this.currentChar;
                this.advance();
            }

            if (dotCount == 0)
                return res.success(new Token(Token.INT, int.Parse(numStr), posStart, this.pos));
            else
                return res.success(new Token(Token.FLOAT, float.Parse(numStr), posStart, this.pos));
        }

        public LexResult makeStr() {
            LexResult res = new LexResult();
            string str = "";
            Position posStart = this.pos.copy();
            bool escapeCharacter = false;
            this.advance();

            Dictionary<string, string> escapeCharacters = new Dictionary<string, string>{
                {"n", "\n"},
                {"t", "\t"},
            };

            while (this.currentChar is not null && (this.currentChar != "\"" || escapeCharacter)) {
                if (escapeCharacter) {
                    str += escapeCharacters.GetValueOrDefault(this.currentChar, this.currentChar);
                    escapeCharacter = false;
                } else {
                    if (this.currentChar == "\\") escapeCharacter = true;
                    else str += this.currentChar;
                }
                this.advance();
            }

            if (this.currentChar != "\"") {
                return res.failure(new Error(
                    posStart, this.pos,
                    Error.InvalidSyntaxError,
                    "String wasn't closed"
                ));
            }

            this.advance();
            return res.success(new Token(Token.STR, str, posStart, this.pos));
        }

        public LexResult makeFSlash() {
            LexResult res = new LexResult();
            this.advance();

            if (this.currentChar != "/")
                return res.success(new Token(Token.DIV, posStart: this.pos));

            res.register(this.skipComment());
            return res;
        }

        public LexResult skipComment() {
            LexResult res = new LexResult();
            this.advance();

            while (!((new string[2] {"\n", null}).Contains(this.currentChar))) {
                this.advance();
            }
            this.advance();

            return res;
        }

        public LexResult makeIdentifier() {
            LexResult res = new LexResult();
            Position posStart = this.pos.copy();
            string str = "";

            while (this.currentChar is not null && Lexer.LETTERS_DIGITS.Contains(this.currentChar)) {
                str += this.currentChar;
                this.advance();
            }

            string tokType = Token.KEYWORDS.Contains(str) ? Token.KEYWORD: Token.IDENTIFIER;
            return res.success(new Token(tokType, str, posStart, this.pos));
        }

        public LexResult makeColon() {
            LexResult res = new LexResult();
            Position posStart = this.pos.copy();
            this.advance();

            if (this.currentChar == ";")
                return res.success(new Token(Token.NSET, posStart: posStart, posEnd: this.pos));

            return res.success(new Token(Token.SET, posStart: posStart, posEnd: this.pos));
        }

        public LexResult makeNotEquals() {
            LexResult res = new LexResult();
            Position posStart = this.pos.copy();
            this.advance();

            if (this.currentChar == "=") {
                this.advance();
                return res.success(new Token(Token.NE, posStart: posStart, posEnd: this.pos));
            }

            this.advance();
            return res.failure(new Error(
                posStart, this.pos,
                Error.ExpectedCharError,
                "'=' (after '!')"
            ));
        }

        public LexResult makeEquals() {
            LexResult res = new LexResult();
            Position posStart = this.pos.copy();
            this.advance();

            if (this.currentChar == "=") {
                this.advance();
                return res.success(new Token(Token.EE, posStart: posStart, posEnd: this.pos));
            }

            return res.success(new Token(Token.EQ, posStart: posStart, posEnd: this.pos));
        }

        public LexResult makeDash() {
            LexResult res = new LexResult();
            Position posStart = this.pos.copy();
            this.advance();

            if (this.currentChar == ">") {
                this.advance();
                return res.success(new Token(Token.ARRW, posStart: posStart, posEnd: this.pos));
            }

            return res.success(new Token(Token.MINUS, posStart: posStart, posEnd: this.pos));
        }

        public LexResult makePower() {
            LexResult res = new LexResult();
            Position posStart = this.pos.copy();
            this.advance();

            if (this.currentChar == "^") {
                this.advance();
                return res.success(new Token(Token.TET, posStart: posStart, posEnd: this.pos));
            }

            return res.success(new Token(Token.POW, posStart: posStart, posEnd: this.pos));
        }

        public LexResult makeLesserThan() {
            LexResult res = new LexResult();
            Position posStart = this.pos.copy();
            this.advance();

            if (this.currentChar == "=") {
                this.advance();
                return res.success(new Token(Token.LTE, posStart: posStart, posEnd: this.pos));
            }

            return res.success(new Token(Token.LT, posStart: posStart, posEnd: this.pos));
        }

        public LexResult makeGreaterThan() {
            LexResult res = new LexResult();
            Position posStart = this.pos.copy();
            this.advance();

            if (this.currentChar == "=") {
                this.advance();
                return res.success(new Token(Token.GTE, posStart: posStart, posEnd: this.pos));
            }

            return res.success(new Token(Token.GT, posStart: posStart, posEnd: this.pos));
        }
    }
}