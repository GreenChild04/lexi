using System.Collections;

namespace lexi
{
    // Main Parser Class
    public class Parser {
        public List<Token> tokens;
        public int tokIdx;
        public Token currentTok;

        public Parser(List<Token> tokens) {
            this.tokens = tokens;
            this.tokIdx = -1;
            this.currentTok = null;
            this.advance();
        }

        public object advance() {
            this.tokIdx++;
            if (this.tokIdx < this.tokens.Count)
                this.currentTok = this.tokens[this.tokIdx];
            return null;
        }

        public ParseResult parse() {
            ParseResult res = this.expr();
            if (res.error is null && this.currentTok.type != Token.EOF) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.InvalidSyntaxError,
                    ErrorMsg.ISE001
                ));
            }
            return res;
        }


        ///////////////////////
        // Parser Templates
        ///////////////////////

        public static ParseResult tPlate_BinOpNode(Parser self, Func<ParseResult> func, ArrayList ops, Func<ParseResult> funcB=null) {
            if (funcB is null) funcB = func;

            ParseResult res = new ParseResult();
            dynamic left = res.register(func());
            if (res.error is not null) return res;

            while (ops.Contains(self.currentTok.type) || Parser.opsContains(ops, new ArrayList() {self.currentTok.type, self.currentTok.value})) {
                Token opTok = self.currentTok;
                self.advance();
                dynamic right = res.register(funcB());
                if (res.error is not null) return res;
                left = new BinOpNode(left, opTok, right, left.posStart, right.posEnd);
            }

            return res.success(left);
        }

        public static bool opsContains(ArrayList list, dynamic obj) {
            bool result = false;
            foreach (dynamic i in list) {
                if (i[0].Equals(obj[0]) && i[1].Equals(obj[1])) result = true;
            } 
            return result;
        }


        ///////////////////////
        // Parser Methods
        ///////////////////////

        // (The functions are ordered according to the parse hierarchy)

        public ParseResult iterExpr(bool strict=false, string bracket=null) {
            ParseResult res = new ParseResult();
            ArrayList elementNodes = new ArrayList();
            Position posStart = this.currentTok.posStart.copy();

            elementNodes.Add(res.register(this.expr()));
            if (res.error is not null) return res;
            while (this.currentTok.type == Token.COMMA) {
                res.registerAdvance(this.advance());

                if (!(new List<string>() {Token.EOF, bracket}.Contains(this.currentTok.type))) {
                    elementNodes.Add(res.register(this.expr()));
                    if (res.error is not null) return res;
                }
            }

            if (elementNodes.Count == 1 && !strict) {
                return res.success(elementNodes[0]);
            }

            return res.success(new IterNode(
                elementNodes, posStart, this.currentTok.posEnd
            ));
        }

        public ParseResult expr() {
            ParseResult res = new ParseResult();
            return Parser.tPlate_BinOpNode(this, this.boolOp, new ArrayList() {new ArrayList() {Token.KEYWORD, "and"}, new ArrayList() {Token.KEYWORD, "or"}});
        }

        public ParseResult boolOp() {
            ParseResult res = new ParseResult();

            if (this.currentTok.matches(Token.KEYWORD, "not")) {
                Token opTok = this.currentTok;
                res.registerAdvance(this.advance());

                dynamic node = res.register(this.boolOp());
                if (res.error is not null) return res;
                return res.success(new UnaryOpNode(opTok, node, opTok.posStart, node.posEnd));
            }

            return Parser.tPlate_BinOpNode(this, this.binOp, new ArrayList() {Token.EE, Token.NE, Token.GT, Token.LT, Token.GTE, Token.LTE});
        }

        public ParseResult binOp() {return Parser.tPlate_BinOpNode(this, this.factor, new ArrayList() {Token.PLUS, Token.MINUS});}

        public ParseResult factor() {return Parser.tPlate_BinOpNode(this, this.power, new ArrayList() {Token.MUL, Token.DIV});}

        public ParseResult power() {return Parser.tPlate_BinOpNode(this, this.atom, new ArrayList() {Token.POW, Token.TET});}

        public ParseResult atom() {
            ParseResult res = new ParseResult();
            Token tok = this.currentTok;

            if (tok.type == Token.INT || tok.type == Token.FLOAT) {
                res.registerAdvance(this.advance());
                return res.success(new NumberNode(tok, tok.posStart, tok.posEnd));
            } else if (tok.type == Token.STR) {
                res.registerAdvance(this.advance());
                return res.success(new StringNode(tok, tok.posStart, tok.posEnd));
            } else if (tok.type == Token.MINUS || tok.type == Token.PLUS || tok.matches(Token.KEYWORD, "not")) {
                return this.unaryOp();
            }

            return res.failure(new Error(
                tok.posStart, tok.posEnd,
                Error.InvalidSyntaxError,
                ErrorMsg.ISE004
            ));
        }

        public ParseResult unaryOp() {
            ParseResult res = new ParseResult();
            Token tok = this.currentTok;

            if (tok.type == Token.MINUS || tok.type == Token.PLUS || tok.matches(Token.KEYWORD, "not")) {
                res.registerAdvance(this.advance());
                dynamic node = res.register(this.atom());
                if (res.error is not null) return res;
                return res.success(new UnaryOpNode(tok, node, tok.posStart, node.posEnd));
            }

            return res.failure(new Error(
                tok.posStart, tok.posEnd,
                Error.InvalidSyntaxError,
                ErrorMsg.ISE004
            ));
        }
    }
}