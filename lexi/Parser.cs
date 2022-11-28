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
            ParseResult res = this.iterStmt();
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

        public ParseResult iterStmt(bool strict=false, string bracket=null) {
            ParseResult res = new ParseResult();
            ArrayList elementNodes = new ArrayList();
            Position posStart = this.currentTok.posStart.copy();

            elementNodes.Add(res.register(this.stmt()));
            if (res.error is not null) return res;
            while (this.currentTok.type == Token.SEMI) {
                res.registerAdvance(this.advance());

                if (!(new List<string>() {Token.EOF, bracket}.Contains(this.currentTok.type))) {
                    elementNodes.Add(res.register(this.stmt()));
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

        public ParseResult stmt() {
            ParseResult res = new ParseResult();

            if (this.currentTok.matches(Token.KEYWORD, "col")) {
                return this.colExpr();
            } else if (this.currentTok.matches(Token.KEYWORD, "late")) {
                return this.lateDef();
            } else if (this.currentTok.matches(Token.KEYWORD, "while")) {
                return this.whileExpr();
            } else if (this.currentTok.matches(Token.KEYWORD, "if")) {
                return this.ifExpr();
            } else if (this.currentTok.matches(Token.KEYWORD, "live") || this.currentTok.matches(Token.KEYWORD, "const")) {
                return this.varDef();
            } else if (this.currentTok.type == Token.KEYWORD)
                return this.reservedKey();
            
            dynamic expr = res.register(this.iterExpr());
            if (res.error is not null) return res;

            return res.success(expr);
        }

        public ParseResult reservedKey() {
            ParseResult res = new ParseResult();
            Position posStart = this.currentTok.posStart;

            if (this.currentTok.matches(Token.KEYWORD, "use")) {
                res.registerAdvance(this.advance());

                if (this.currentTok.type != Token.IDENTIFIER) {
                    return res.failure(new Error(
                        this.currentTok.posStart, this.currentTok.posEnd,
                        Error.InvalidSyntaxError,
                        ErrorMsg.ISE016
                    ));
                } GetNode identifier = (GetNode) res.register(this.getExpr());

                return res.success(new ImportNode(identifier, posStart, this.currentTok.posEnd));
            } else if (this.currentTok.matches(Token.KEYWORD, "return")) {
                res.registerAdvance(this.advance());
                dynamic returnValue = res.register(this.iterExpr());

                return res.success(new ReturnNode(returnValue, posStart, this.currentTok.posEnd));
            }

            return res.failure(new Error(
                posStart, posStart,
                Error.InvalidSyntaxError,
                ErrorMsg.ISE017
            ));
        }

        public ParseResult colExpr() {
            ParseResult res = new ParseResult();

            if (!(this.currentTok.matches(Token.KEYWORD, "col"))) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.InvalidSyntaxError,
                    ErrorMsg.ISE005
                ));
            }
                
            res.registerAdvance(this.advance());

            if (this.currentTok.type == Token.IDENTIFIER) {
                res.registerAdvance(this.advance());
            } if (this.currentTok.type == Token.NSET) {
                res.registerAdvance(this.advance());
                return res;
            }

            if (this.currentTok.type != Token.SET) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.InvalidCharError,
                    ErrorMsg.ICE001
                ));
            } res.registerAdvance(this.advance());

            IterNode curlExpr = (IterNode) res.register(this.curlExpr());
            if (res.error is not null) return res;

            return res.success(new IterNode(curlExpr.elementNodes, curlExpr.posStart, curlExpr.posEnd));
        }

        public ParseResult ifExpr() {
            ParseResult res = new ParseResult();
            Position posStart = this.currentTok.posStart;
            ArrayList conditions = new ArrayList();
            List<CurlNode> bodies = new List<CurlNode>();

            if (!this.currentTok.matches(Token.KEYWORD, "if")) {
                return res.failure(new Error(
                    posStart, this.currentTok.posEnd,
                    Error.InvalidSyntaxError,
                    ErrorMsg.ISE014
                ));
            } res.registerAdvance(this.advance());

            Position condPosition = this.currentTok.posStart;
            dynamic condition = res.register(this.compExpr());
            if (res.error is not null) {
                res.error = new Error(
                    condPosition, this.currentTok.posEnd,
                    Error.InvalidSyntaxError,
                    ErrorMsg.ISE018
                ); return res;
            }

            if (!(this.currentTok.type == Token.SET)) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.ExpectedCharError,
                    ErrorMsg.ICE001
                ));
            } res.registerAdvance(this.advance());

            dynamic curlExpr = res.register(this.curlExpr());
            if (res.error is not null) return res;
            conditions.Add(condition);
            bodies.Add(curlExpr);

            while (this.currentTok.matches(Token.KEYWORD, "else")) {
                res.registerAdvance(this.advance());

                if (this.currentTok.type == Token.SET) {
                    condition = new GetNode(new Token(Token.IDENTIFIER, "true", this.currentTok.posStart, this.currentTok.posEnd), null, this.currentTok.posStart, this.currentTok.posEnd);
                } else {
                    condition = res.register(this.compExpr());
                    if (res.error is not null) return res;
                } res.registerAdvance(this.advance());

                curlExpr = res.register(this.curlExpr());
                if (res.error is not null) return res;

                conditions.Add(condition);
                bodies.Add(curlExpr);
            }

            return res.success(new IfNode(conditions, bodies, posStart, this.currentTok.posEnd));
        }

        public ParseResult whileExpr() {
            ParseResult res = new ParseResult();
            Position posStart = this.currentTok.posStart;

            if (!this.currentTok.matches(Token.KEYWORD, "while")) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.InvalidSyntaxError,
                    ErrorMsg.ISE013
                ));
            } res.registerAdvance(this.advance());

            dynamic condition = res.register(this.compExpr());
            if (res.error is not null) return res;

            if (this.currentTok.type != Token.SET) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.ExpectedCharError,
                    ErrorMsg.ICE001
                ));
            } res.registerAdvance(this.advance());

            CurlNode curlNode = (CurlNode) res.register(this.curlExpr());
            if (res.error is not null) return res;

            return res.success(new WhileNode(
                condition, curlNode,
                posStart, curlNode.posEnd
            ));
        }

        public ParseResult lateDef() {
            ParseResult res = new ParseResult();
            Position posStart = this.currentTok.posStart;

            if (!(this.currentTok.matches(Token.KEYWORD, "late"))) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.InvalidSyntaxError,
                    ErrorMsg.ISE011
                ));
            } res.registerAdvance(this.advance());

            Token identifier;
            if (this.currentTok.type == Token.IDENTIFIER) {
                identifier = this.currentTok;
                res.registerAdvance(this.advance());
            } else identifier = null;

            List<Token> args = new List<Token>();
            if (this.currentTok.type == Token.LPAREN) {
                res.registerAdvance(this.advance());

                if (this.currentTok.type == Token.IDENTIFIER) {
                    args.Append(this.currentTok);
                    res.registerAdvance(this.advance());

                    while (this.currentTok.type == Token.COMMA) {
                        res.registerAdvance(this.advance());

                        if (this.currentTok.type != Token.IDENTIFIER)
                            return res.failure(new Error(
                                this.currentTok.posStart, this.currentTok.posEnd,
                                Error.InvalidSyntaxError,
                                ErrorMsg.ISE012
                            ));

                        args.Append(this.currentTok);
                        res.registerAdvance(this.advance());
                    } if (this.currentTok.type != Token.RPAREN)
                        return res.failure(new Error(
                            this.currentTok.posStart, this.currentTok.posEnd,
                            Error.ExpectedCharError,
                            ErrorMsg.ICE005
                        ));
                } else if (this.currentTok.type != Token.RPAREN)
                    return res.failure(new Error(
                        this.currentTok.posStart, this.currentTok.posEnd,
                        Error.ExpectedCharError,
                        ErrorMsg.ICE011
                    ));

                res.registerAdvance(this.advance());
            }

            if (this.currentTok.type == Token.NSET) {
                res.registerAdvance(this.advance());
                return res.success(new LateDefNode(
                    identifier, null, null, posStart, this.currentTok.posEnd
                ));
            } else if (this.currentTok.type != Token.SET) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.ExpectedCharError,
                    ErrorMsg.ICE001
                ));
            } res.registerAdvance(this.advance());

            CurlNode curlNode = (CurlNode) res.register(this.curlExpr());
            if (res.error is not null) return res;

            return res.success(new LateDefNode(
                identifier, args, curlNode,
                posStart, this.currentTok.posEnd
            ));
        }

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
            if (this.currentTok.matches(Token.KEYWORD, "encap"))
                return this.encap();

            return this.callExpr();
        }

        public ParseResult encap() {
            ParseResult res = new ParseResult();
            
            if (!this.currentTok.matches(Token.KEYWORD, "encap")) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.InvalidSyntaxError,
                    ErrorMsg.ISE015
                ));
            } res.registerAdvance(this.advance());

            return this.stmt();
        }

        public ParseResult varDef() {
            ParseResult res = new ParseResult();
            Position posStart = this.currentTok.posStart;

            bool isConst = false;
            if (!(this.currentTok.matches(Token.KEYWORD, "live") || this.currentTok.matches(Token.KEYWORD, "const"))) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.InvalidSyntaxError,
                    ErrorMsg.ISE010
                ));
            } else if (this.currentTok.matches(Token.KEYWORD, "const"))
                isConst = true;

            res.registerAdvance(this.advance());

            Token identifier;
            if (this.currentTok.type == Token.IDENTIFIER) {
                identifier = this.currentTok;
                res.registerAdvance(this.advance());
            } else identifier = null;

            if (this.currentTok.type == Token.NSET) {
                res.registerAdvance(this.advance());
                return res.success(new VarDefNode(
                    identifier, null, isConst, posStart, this.currentTok.posEnd
                ));
            } else if (this.currentTok.type == Token.EQ) {
                dynamic varMod = res.register(this.varMod(posStart, new GetNode(identifier, null, identifier.posStart, identifier.posEnd)));
                if (res.error is not null) return res;

                return res.success(new IterNode(
                    new ArrayList() {
                        new VarDefNode(identifier, null, isConst, posStart, this.currentTok.posEnd),
                        varMod
                    },
                    posStart, this.currentTok.posEnd
                ));
            } else if (this.currentTok.type != Token.SET) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.ExpectedCharError,
                    ErrorMsg.ICE009
                ));
            }

            res.registerAdvance(this.advance());

            dynamic value = res.register(this.iterExpr());
            if (res.error is not null) return res;
            
            return res.success(new VarDefNode(
                identifier, value, isConst, posStart, this.currentTok.posEnd
            ));
        }

        public ParseResult callExpr() {
            ParseResult res = new ParseResult();
            Token identifier = null;
            Position posStart = this.currentTok.posStart;

            dynamic compExpr = res.register(this.compExpr());
            if (res.error is not null) return res;

            if (this.currentTok.type == Token.IDENTIFIER) {
                identifier = this.currentTok;
                res.registerAdvance(this.advance());
            } else if (this.currentTok.type == Token.EQ) {
                return this.varMod(posStart, compExpr);
            }

            if (this.currentTok.type == Token.NSET) {
                res.registerAdvance(this.advance());
                return res.success(new CallNode(compExpr, identifier, new ArrayList(), posStart, this.currentTok.posEnd));
            }

            if (this.currentTok.type == Token.SET) {
                res.registerAdvance(this.advance());
                ArrayList argNodes = new ArrayList();

                IterNode iterNode = (IterNode) res.register(this.iterExpr(true));
                if (res.error is not null) return res;
                if (iterNode is not null)
                    argNodes = iterNode.elementNodes;

                return res.success(new CallNode(compExpr, identifier, argNodes, posStart, iterNode is not null ? iterNode.posEnd: this.currentTok.posEnd));
            } else if (identifier is not null)
                return res.failure(new Error(
                    identifier.posStart,
                    identifier.posEnd,
                    Error.InvalidSyntaxError,
                    ErrorMsg.ISE001
                ));
            
            return res.success(compExpr);
        }

        public ParseResult varMod(Position posStart, dynamic compExpr) {
            ParseResult res = new ParseResult();

            if (this.currentTok.type != Token.EQ) {
                return res.failure(new Error(
                    this.currentTok.posStart, this.currentTok.posEnd,
                    Error.ExpectedCharError,
                    ErrorMsg.ICE010
                ));
            } res.registerAdvance(this.advance());

            dynamic value = res.register(this.iterExpr());
            if (res.error is not null) return res;

            return res.success(new VarModifyNode(
                compExpr, value,
                posStart, this.currentTok.posEnd
            ));
        }

        public ParseResult compExpr() {return Parser.tPlate_BinOpNode(this, this.boolOp, new ArrayList() {new ArrayList() {Token.KEYWORD, "and"}, new ArrayList() {Token.KEYWORD, "or"}});}

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

        public ParseResult power() {return Parser.tPlate_BinOpNode(this, this.getExpr, new ArrayList() {Token.POW, Token.TET});}

        public ParseResult getExpr() {
            ParseResult res = new ParseResult();
            dynamic output = res.register(this.convExpr());
            if (res.error is not null) return res;
            
            while (this.currentTok.type == Token.DOT) {
                res.registerAdvance(this.advance());
                dynamic conv = res.register(this.convExpr());
                if (res.error is not null) return res;
                output = new GetNode(output, conv, output.posStart, conv.posEnd);
            }

            return res.success(output);
        }

        public ParseResult convExpr() {
            ParseResult res = new ParseResult();
            dynamic atom = res.register(this.atom());
            if (res.error is not null) return res;

            while (new List<string>() {Token.LPAREN, Token.LSQUARE, Token.LCURL}.Contains(this.currentTok.type)) {
                dynamic iterType = res.register(this.atom());
                if (res.error is not null) return res;

                if (iterType is ListNode)
                    atom = new ListConvNode(atom, iterType.elementNodes, atom.posStart, iterType.posEnd);
                else if (iterType is TupleNode)
                    atom = new TupleConvNode(atom, iterType.elementNodes, atom.posStart, iterType.posEnd);
                else if (iterType is CurlNode)
                    atom = new CurlConvNode(atom, iterType.elementNodes, atom.posStart, iterType.posEnd);
            }
            
            return res.success(atom);
        }

        public ParseResult atom() {
            ParseResult res = new ParseResult();
            Token tok = this.currentTok;

            if (tok.type == Token.INT || tok.type == Token.FLOAT) {
                res.registerAdvance(this.advance());
                return res.success(new NumberNode(tok, tok.posStart, tok.posEnd));
            } else if (tok.type == Token.STR) {
                res.registerAdvance(this.advance());
                return res.success(new StringNode(tok, tok.posStart, tok.posEnd));
            } else if (tok.type == Token.IDENTIFIER) {
                res.registerAdvance(this.advance());
                return res.success(new GetNode(tok, null, tok.posStart, tok.posEnd));
            } else if (tok.type == Token.MINUS || tok.type == Token.PLUS || tok.matches(Token.KEYWORD, "not")) {
                return this.unaryOp();
            } else if (tok.type == Token.LPAREN) {
                return this.tupleExpr();
            } else if (tok.type == Token.LSQUARE) {
                return this.listExpr();
            } else if (tok.type == Token.LCURL) {
                return this.curlExpr();
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

        public ParseResult tupleExpr() {
            ParseResult res = new ParseResult();
            ArrayList elementNodes = new ArrayList();
            Position posStart = this.currentTok.posStart.copy();

            if (this.currentTok.type != Token.LPAREN) {
                return res.failure(new Error(
                    posStart, this.currentTok.posEnd,
                    Error.ExpectedCharError,
                    ErrorMsg.ICE004
                ));
            } res.registerAdvance(this.advance());

            if (this.currentTok.type == Token.RPAREN) res.registerAdvance(this.advance());
            else {
                IterNode iterExpr = (IterNode) res.register(this.iterExpr(true, Token.RPAREN));
                if (res.error is not null) return res.failure(new Error(
                    posStart, this.currentTok.posEnd,
                    Error.InvalidSyntaxError,
                    ErrorMsg.ISE006
                ));

                elementNodes = iterExpr.elementNodes;

                if (this.currentTok.type != Token.RPAREN) {
                    return res.failure(new Error(
                        this.currentTok.posStart, this.currentTok.posEnd,
                        Error.ExpectedCharError,
                        ErrorMsg.ICE005
                    ));
                } res.registerAdvance(this.advance());
            }

            if (elementNodes.Count == 1)
                return res.success(elementNodes[0]);

            return res.success(new TupleNode(elementNodes, posStart, this.currentTok.posEnd.copy()));
        }

        public ParseResult listExpr() {
            ParseResult res = new ParseResult();
            ArrayList elementNodes = new ArrayList();
            Position posStart = this.currentTok.posStart.copy();

            if (this.currentTok.type != Token.LSQUARE) {
                return res.failure(new Error(
                    posStart, this.currentTok.posEnd,
                    Error.ExpectedCharError,
                    ErrorMsg.ICE008
                ));
            } res.registerAdvance(this.advance());

            if (this.currentTok.type == Token.RSQUARE) res.registerAdvance(this.advance());
            else {
                IterNode iterExpr = (IterNode) res.register(this.iterExpr(true, Token.RSQUARE));
                if (res.error is not null) return res.failure(new Error(
                    posStart, this.currentTok.posEnd,
                    Error.InvalidSyntaxError,
                    ErrorMsg.ISE007
                ));

                elementNodes = iterExpr.elementNodes;

                if (this.currentTok.type != Token.RSQUARE) {
                    return res.failure(new Error(
                        this.currentTok.posStart, this.currentTok.posEnd,
                        Error.ExpectedCharError,
                        ErrorMsg.ICE006
                    ));
                } res.registerAdvance(this.advance());
            }

            return res.success(new ListNode(elementNodes, posStart, this.currentTok.posEnd.copy()));
        }

        public ParseResult curlExpr() {
            ParseResult res = new ParseResult();
            ArrayList elementNodes = new ArrayList();
            Position posStart = this.currentTok.posStart.copy();

            if (this.currentTok.type != Token.LCURL) {
                return res.failure(new Error(
                    posStart, this.currentTok.posEnd,
                    Error.ExpectedCharError,
                    ErrorMsg.ICE002
                ));
            } res.registerAdvance(this.advance());

            if (this.currentTok.type == Token.RCURL) res.registerAdvance(this.advance());
            else {
                IterNode iterStmt = (IterNode) res.register(this.iterStmt(true, Token.RCURL));
                if (res.error is not null) return res.failure(new Error(
                    posStart, this.currentTok.posEnd,
                    Error.InvalidSyntaxError,
                    ErrorMsg.ISE008
                ));

                elementNodes = iterStmt.elementNodes;

                if (this.currentTok.type != Token.RCURL) {
                    return res.failure(new Error(
                        this.currentTok.posStart, this.currentTok.posEnd,
                        Error.ExpectedCharError,
                        ErrorMsg.ICE007
                    ));
                } res.registerAdvance(this.advance());
            }

            return res.success(new CurlNode(elementNodes, posStart, this.currentTok.posEnd.copy()));
        }
    }
}
