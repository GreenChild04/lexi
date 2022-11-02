from dataclasses import dataclass
from lexer import *;
from error import *;
from debug import Debug;


@dataclass()
class NumberNode:
    tok: vars

    def __post_init__(self):
        self.posStart = self.tok.posStart
        self.posEnd = self.tok.posEnd

    def __repr__(self):
        return f"{self.tok}"

@dataclass()
class StringNode:
    tok: vars;

    def __post_init__(self):
        self.posStart = self.tok.posStart;
        self.posEnd = self.tok.posEnd;

    def __repr__(self):
        return f"{self.tok}";

@dataclass()
class IterNode:
    elementNodes: list;
    posStart: vars;
    posEnd: vars;

    def inherit(self, other):
        if isinstance(other, IterNode):
            self.elementNodes = other.elementNodes;
            self.posStart = other.posStart;
            self.posEnd = other.posEnd;
        else:
            raise Exception("Cannot inherit from non iternode object");

    def __repr__(self) -> str:
        return f"{self.elementNodes}";

class ListNode(IterNode):
    pass

class TupleNode(IterNode):
    pass

class CurlNode(IterNode):
    pass

@dataclass()
class BinOpNode:
    leftNode: vars
    opTok: vars
    rightNode: vars

    def __post_init__(self):
        self.posStart = self.leftNode.posStart
        self.posEnd = self.rightNode.posEnd

    def __repr__(self):
        return f"({self.leftNode}, {self.opTok}, {self.rightNode})"

@dataclass()
class UnaryOpNode:
    opTok: any
    node: any

    def __post_init__(self):
        self.posStart = self.opTok.posStart
        self.posEnd = self.node.posEnd

    def __repr__(self):
        return f"({self.opTok}, {self.node})"

@dataclass
class IfNode:
    cases: vars;

    def __post_init__(self):
        self.debug = Debug(self);
        self.debug.register(f"cases: {self.cases}");
        self.posStart = self.cases[0][0].posStart;
        self.posEnd = (self.cases[len(self.cases) - 1][0]).posEnd;

@dataclass
class WhileNode:
    condition: vars;
    body: vars;

    def __post_init__(self):
        self.posStart = self.condition.posStart;
        self.posEnd = self.body.posEnd;

@dataclass
class ReturnNode:
    nodeToReturn: vars;
    posStart: vars;
    posEnd: vars;


@dataclass()
class ParseResult:
    error: any = None
    node: any = None


    def __post_init__(self):
        self.advanceCount = 0

    def registerAdvance(self, advance):
        if advance:
            advance();
        self.advanceCount += 1

    def register(self, res):
        if isinstance(res, ParseResult):
            self.advanceCount += res.advanceCount
            if res.error: self.error = res.error
            return res.node
        raise Exception("Registered Non ParseResult Object")

    def success(self, node):
        self.node = node
        return self

    def failure(self, error):
        if not self.error or self.advanceCount == 0:
            self.error = error
        return self


class Parser:
    def __init__(self, tokens):
        self.tokens = tokens;
        self.tokIdx = -1;
        self.currentTok = None;
        self.debug = Debug(self);
        self.advance();

    def log(self, msg):
        if isinstance(msg, list):
            for i in msg:
                self.debug.register([f"{i}; currentTok[{self.currentTok}]"]);
        else:
            self.debug.register(f"{msg}");

    def advance(self):
        self.tokIdx += 1;
        if self.tokIdx < len(self.tokens):
            self.currentTok = self.tokens[self.tokIdx];
        self.log(["Advanced Tokens"]);

    def parse(self):
        res = self.iterExpr(additional=TT_SEMI);
        if not res.error and self.currentTok.type != TT_EOF:
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected '+', '-', '*', '/', '^' or '^^'"
            ))
        return res

    def atom(self):
        res = ParseResult()
        tok = self.currentTok
        self.log("Making Atom");

        if tok.type == TT_LPAREN:
            tupleExpr = res.register(self.tupleExpr());
            if res.error: return res;
            return res.success(tupleExpr);

        elif tok.type == TT_LSQUARE:
            listExpr = res.register(self.listExpr());
            if res.error: return res;
            return res.success(listExpr);

        elif tok.type == TT_LCURL:
            curlExpr = res.register(self.curlExpr());
            if res.error: return res;
            return res.success(curlExpr);

        elif tok.matches(TT_KEYWORD, "if"):
            ifExpr = res.register(self.ifExpr())
            if res.error: return res
            return res.success(ifExpr)

        elif tok.matches(TT_KEYWORD, "while"):
            whileExpr = res.register(self.whileExpr());
            if res.error: return res;
            return res.success(whileExpr);

        elif tok.matches(TT_KEYWORD, "fun"):
            funcDef = res.register(self.funcDef());
            if res.error: return res;
            return res.success(funcDef);

        elif tok.type == TT_IDENTIFIER:
            res.registerAdvance(self.advance())
            return res.success(VarAccessNode(tok))

        elif tok.type == TT_STR:
            res.registerAdvance(self.advance())
            return res.success(StringNode(tok))

        elif tok.type in (TT_INT, TT_FLOAT):
            res.registerAdvance(self.advance())
            return res.success(NumberNode(tok))

        elif tok.type in (TT_MINUS, TT_PLUS):
            return self.factor()

        return res.failure(InvalidSyntaxError(
            tok.posStart, tok.posEnd,
            "Expected int, float, identifier, string, var, while, if, fun, '[', '+', '-', or '('"))

    def factor(self):
        res = ParseResult()
        tok = self.currentTok
        self.log("Making Factor");

        if tok.type in (TT_PLUS, TT_MINUS):
            res.registerAdvance(self.advance())
            atom = res.register(self.atom())
            if res.error: return res
            return res.success(UnaryOpNode(tok, atom))

        return self.tet();

    def listExpr(self):
        res = ParseResult();
        elementNodes = [];
        posStart = self.currentTok.posStart.copy();
        self.log("Making List")

        if self.currentTok.type != TT_LSQUARE:
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected '['"
            ));

        res.registerAdvance(self.advance());

        if self.currentTok.type == TT_RSQUARE:
            res.registerAdvance(self.advance());
        else:
            iterExpr = res.register(self.iterExpr(True, TT_RSQUARE, TT_SEMI));
            if res.error: return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected ']', 'var', 'if', 'while', 'fun', int, float, identifier, '+', '-', ']', if, fun, or '('"
            ))

            elementNodes = iterExpr.elementNodes;

            if self.currentTok.type != TT_RSQUARE:
                return res.failure(InvalidSyntaxError(
                    self.currentTok.posStart, self.currentTok.posEnd,
                    "Expected ',' or ']'"
                ))

            res.registerAdvance(self.advance());

        self.debug.register([f"List Element Nodes: {elementNodes}"]);

        return res.success(ListNode(
            elementNodes, posStart, self.currentTok.posEnd.copy()
        ));

    def iterExpr(self, strict=False, bracket=None, additional=None):
        res = ParseResult();
        elementNodes = [];
        posStart = self.currentTok.posStart.copy();
        self.log("Making Iteratable")

        elementNodes.append(res.register(self.expr()))
        if res.error: return res;
        while self.currentTok.type in (TT_COMMA, additional):
            res.registerAdvance(self.advance());

            if self.currentTok.type not in (TT_EOF, bracket):
                elementNodes.append(res.register(self.expr()));
                if res.error: return res;

        self.debug.register([f"Iteratable Element Nodes: {elementNodes}"]);

        if len(elementNodes) == 1 and not strict:
            return res.success(elementNodes[0]);

        return res.success(IterNode(
            elementNodes, posStart, self.currentTok.posEnd.copy()
        ));
        

    def tupleExpr(self):
        res = ParseResult();
        elementNodes = [];
        posStart = self.currentTok.posStart.copy();
        self.log("Making Tuple")

        if self.currentTok.type != TT_LPAREN:
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected '('"
            ));

        res.registerAdvance(self.advance());

        if self.currentTok.type == TT_RPAREN:
            res.registerAdvance(self.advance());
        else:
            iterExpr = res.register(self.iterExpr(True, TT_RPAREN, TT_SEMI));
            if res.error: return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected ')', 'var', 'if', 'while', 'fun', int, float, identifier, '+', '-', ']', if, fun, or '('"
            ))

            elementNodes = iterExpr.elementNodes;

            if self.currentTok.type != TT_RPAREN:
                return res.failure(InvalidSyntaxError(
                    self.currentTok.posStart, self.currentTok.posEnd,
                    "Expected ',' or ')'"
                ))

            res.registerAdvance(self.advance());

        self.debug.register([f"Tuple Element Nodes: {elementNodes}"]);

        if len(elementNodes) == 1:
            return res.success(elementNodes[0]);

        return res.success(TupleNode(
            elementNodes, posStart, self.currentTok.posEnd.copy()
        ));

    def curlExpr(self):
        res = ParseResult();
        elementNodes = [];
        posStart = self.currentTok.posStart.copy();
        self.log("Making Curl")

        if self.currentTok.type != TT_LCURL:
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected '{'"
            ));

        res.registerAdvance(self.advance());

        if self.currentTok.type == TT_RCURL:
            res.registerAdvance(self.advance());
        else:
            iterExpr = res.register(self.iterExpr(True, TT_RCURL, TT_SEMI));
            if res.error: return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected '}', 'var', 'if', 'while', 'fun', int, float, identifier, '+', '-', ']', if, fun, or '('"
            ))

            elementNodes = iterExpr.elementNodes;

            if self.currentTok.type != TT_RCURL:
                return res.failure(InvalidSyntaxError(
                    self.currentTok.posStart, self.currentTok.posEnd,
                    "Expected ',' or '}'"
                ))

            res.registerAdvance(self.advance());

        self.debug.register([f"Curl Element Nodes: {elementNodes}"]);

        return res.success(CurlNode(
            elementNodes, posStart, self.currentTok.posEnd.copy()
        ));

    def term(self):
        self.log("Making Term");
        return self.binOp(self.factor, (TT_MUL, TT_DIV))

    def arithExpr(self):
        self.log("Making ArithExpr");
        return self.binOp(self.term, (TT_PLUS, TT_MINUS))

    def compExpr(self):
        self.log("Making CompExpr");
        res = ParseResult()

        if self.currentTok.matches(TT_KEYWORD, "not"):
            opTok = self.currentTok
            res.registerAdvance(self.advance())

            node = res.register(self.compExpr())
            if res.error: return res
            return res.success(UnaryOpNode(opTok, node))

        node = res.register(self.binOp(self.arithExpr, (TT_EE, TT_NE, TT_LT, TT_GT, TT_LTE, TT_GTE)))

        if res.error:
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected int, float, identifier, string, '+', '-', '[', 'not', if, fun or '('"
            ))

        return res.success(node)

    def expr(self):
        self.log("Making Expr");
        res = ParseResult()

        if self.currentTok.matches(TT_KEYWORD, "var"):
            res.registerAdvance(self.advance())

            if self.currentTok.type != TT_IDENTIFIER:
                return res.failure(InvalidSyntaxError(
                    self.currentTok.posStart, self.currentTok.posEnd,
                    "Expected Identifier",
                ))

            varName = self.currentTok
            res.registerAdvance(self.advance())

            if self.currentTok.type != TT_SET:
                return res.failure(InvalidSyntaxError(
                    self.currentTok.posStart, self.currentTok.posEnd,
                    "Expected ':'"
                ))

            res.registerAdvance(self.advance())
            expr = res.register(self.expr())
            if res.error: return res
            return res.success(VarAssignNode(varName, expr))

        node = res.register(self.binOp(self.compExpr, ((TT_KEYWORD, "and"), (TT_KEYWORD, "or"))))

        if res.error:
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected 'var', int, float, identifier, '+', '-', '[', if, fun, or '('"
            ))

        return res.success(node)

    def whileExpr(self):
        self.log("Making WhileExpr");
        res = ParseResult();

        if not self.currentTok.matches(TT_KEYWORD, "while"):
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected 'while'"
            ))

        res.registerAdvance(self.advance());

        condition = res.register(self.expr());
        if res.error: return res;

        if not self.currentTok.type == TT_SET:
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected ':' after while condition"
            ))
        
        res.registerAdvance(self.advance());

        expr = res.register(self.expr());
        if res.error: return res;

        return res.success(WhileNode(condition, expr));

    def funcDef(self):
        self.log("Making FuncDef")
        res = ParseResult()

        if not self.currentTok.matches(TT_KEYWORD, "fun"):
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected 'fun' keyword"
            ))

        res.registerAdvance(self.advance())

        if self.currentTok.type == TT_IDENTIFIER:
            varNameTok = self.currentTok;
            res.registerAdvance(self.advance());
        else:
            varNameTok = None;

        argNameToks = [];
        if self.currentTok.type == TT_LPAREN:
            res.registerAdvance(self.advance());

            if self.currentTok.type == TT_IDENTIFIER:
                argNameToks.append(self.currentTok);
                res.registerAdvance(self.advance());

                while self.currentTok.type == TT_COMMA:
                    res.registerAdvance(self.advance());

                    if self.currentTok.type != TT_IDENTIFIER:
                        return res.failure(InvalidSyntaxError(
                            self.currentTok.posStart, self.currentTok.posEnd,
                            "Expected identifier after comma"
                        ));
                    
                    argNameToks.append(self.currentTok);
                    res.registerAdvance(self.advance())
                
                if self.currentTok.type != TT_RPAREN:
                    return res.failure(InvalidSyntaxError(
                        self.currentTok.posStart, self.currentTok.posEnd,
                        "Expected ',' or ')'"
                    ));
            else:
                if self.currentTok.type != TT_RPAREN:
                    return res.failure(InvalidSyntaxError(
                        self.currentTok.posStart, self.currentTok.posEnd,
                        "Expected identifer or ')'"
                    ))

            res.registerAdvance(self.advance());

        if self.currentTok.type != TT_SET:
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected ':'"
            ));

        res.registerAdvance(self.advance());

        curlExpr = res.register(self.expr());
        if res.error: return res;

        return res.success(FuncDefNode(
            varNameTok, argNameToks, curlExpr
        ));

    def ifExpr(self):
        self.log("Making IfExpr");
        res = ParseResult();
        cases = [];

        if not self.currentTok.matches(TT_KEYWORD, "if"):
            return res.failure(
                InvalidSyntaxError(self.currentTok.posStart, self.currentTok.posEnd,
                f"Expected 'if'"
            ))

        res.registerAdvance(self.advance());

        condition = res.register(self.expr());
        if res.error: return res;

        if not self.currentTok.type == TT_SET:
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected ':' after if condition"
            ))

        res.registerAdvance(self.advance());

        expr = res.register(self.expr())
        if res.error: return res;
        cases.append((condition, expr))

        while self.currentTok.matches(TT_KEYWORD, "elif"):
            self.log("Making ElifExpr");
            res.registerAdvance(self.advance());

            condition = res.register(self.expr());
            if res.error: return res;

            if not self.currentTok.type == TT_SET:
                return res.failure(InvalidSyntaxError(
                    self.currentTok.posStart, self.currentTok.posEnd,
                    "Expected ':' after elif statement"
                ))

            res.registerAdvance(self.advance());

            expr = res.register(self.expr());
            if res.error: return res;
            cases.append((condition, expr));

        if self.currentTok.matches(TT_KEYWORD, "else"):
            self.log("Making ElseExpr");
            res.registerAdvance(self.advance());

            if not self.currentTok.type == TT_SET:
                return res.failure(InvalidSyntaxError(
                    self.currentTok.posStart, self.currentTok.posEnd,
                    "Expected ':' after else statement"
                ))

            res.registerAdvance(self.advance());

            posStart = self.currentTok.posStart.copy();
            posEnd = self.currentTok.posEnd.copy();

            expr = res.register(self.expr());
            if res.error: return res;

            old = self.currentTok;

            self.currentTok = Token(TT_IDENTIFIER, "true", posStart=posStart, posEnd=posEnd, debug=self.debug);
            atom = res.register(self.atom());

            self.currentTok = old;

            cases.append((atom, expr));

        return res.success(IfNode(cases))

    def pow(self):
        self.log("Making Pow");
        return self.binOp(self.call, (TT_POW, TT_BLNK), self.factor)

    def tet(self):
        self.log("Making Tetration");
        return self.binOp(self.pow, (TT_TET, TT_BLNK), self.atom)

    def call(self):
        res = ParseResult();
        conv = res.register(self.convExpr());
        if res.error: return res;

        if self.currentTok.type == TT_ARRW:
            res.registerAdvance(self.advance());
            argNodes = [];

            iterExpr = res.register(self.iterExpr(True));
            if iterExpr is not None:
                argNodes = iterExpr.elementNodes;

            return res.success(CallNode(conv, argNodes))
        elif self.currentTok.type == TT_NSET:
            res.registerAdvance(self.advance());
            return res.success(CallNode(conv, []))
        return res.success(conv);

    def convExpr(self):
        res = ParseResult();
        atom = res.register(self.atom());
        if res.error: return res;

        def gen(token, fun):
            res.registerAdvance(self.advance());
            argNodes = [];
            
            if self.currentTok.type == token:
                res.registerAdvance(self.advance());
            else:
                iterExpr = res.register(self.iterExpr(True));
                if res.error: return res;
                argNodes = iterExpr.elementNodes;
                if self.currentTok.type != token:
                    return res.failure(InvalidSyntaxError(
                        self.currentTok.posStart, self.currentTok.posEnd,
                        f"Expected '{token}'"
                    ));
                res.registerAdvance(self.advance());

            return res.success(fun(atom, argNodes));

        if self.currentTok.type == TT_LSQUARE:
            convList = res.register(gen(TT_RSQUARE, ListConvNode));
            return res;
        elif self.currentTok.type == TT_LCURL:
            convList = res.register(gen(TT_RCURL, CurlConvNode));
            return res;
        elif self.currentTok.type == TT_LPAREN:
            convList = res.register(gen(TT_RPAREN, ParenConvNode));
            return res;

        return res.success(atom);

    def binOp(self, func, ops, funcB=None):
        self.log("Making BinOp");
        if funcB is None:
            funcB = func
        res = ParseResult()
        left = res.register(func())
        if res.error: return res

        while self.currentTok.type in ops or (self.currentTok.type, self.currentTok.value) in ops:
            opTok = self.currentTok
            self.advance()
            right = res.register(funcB())
            if res.error: return res
            left = BinOpNode(left, opTok, right)

        return res.success(left)
