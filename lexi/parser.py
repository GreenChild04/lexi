from dataclasses import dataclass
from lexer import *
from object import *
from debug import Debug


@dataclass()
class NumberNode:
    tok: vars

    def __post_init__(self):
        self.posStart = self.tok.posStart
        self.posEnd = self.tok.posEnd

    def __repr__(self):
        return f"{self.tok}"


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


@dataclass()
class ParseResult:
    error: any = None
    node: any = None

    def __post_init__(self):
        self.advanceCount = 0

    def registerAdvance(self, advance):
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
        self.tokens = tokens
        self.tokIdx = -1
        self.currentTok = None
        self.debug = Debug(self);
        self.advance()

    def log(self, msg):
        self.debug.register(f"{msg}; currentTok[{self.currentTok}]");

    def advance(self):
        self.tokIdx += 1
        if self.tokIdx < len(self.tokens):
            self.currentTok = self.tokens[self.tokIdx]
        return self.currentTok

    def parse(self):
        res = self.expr()
        if not res.error and self.currentTok.type != TT_EOF:
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected '+', '-', '*', '/' or '^'"
            ))
        return res

    def atom(self):
        res = ParseResult()
        tok = self.currentTok
        self.log(f"Initiating Atom");

        if tok.type == TT_LPAREN:
            self.log(f"Found LPAREN");
            res.registerAdvance(self.advance())
            expr = res.register(self.expr())
            if res.error: return res
            if self.currentTok.type == TT_RPAREN:
                res.registerAdvance(self.advance())
                return res.success(expr)
            else:
                return res.failure(InvalidSyntaxError(
                    self.currentTok.posStart, self.currentTok.posEnd,
                    "Expected ')' to end parentheses"
                ))

        elif tok.type == TT_IDENTIFIER:
            self.log(f"Found Identifier")
            res.registerAdvance(self.advance())
            return res.success(VarAccessNode(tok))

        elif tok.type in (TT_INT, TT_FLOAT):
            self.log(f"Found Int/Float")
            res.registerAdvance(self.advance())
            return res.success(NumberNode(tok))

        elif tok.type in (TT_MINUS, TT_PLUS):
            self.log(f"Found Plus/Minus")
            return self.factor()

        return res.failure(InvalidSyntaxError(
            tok.posStart, tok.posEnd,
            "Expected int, float, identifier, string, '+', '-' or '('"))

    def factor(self):
        res = ParseResult()
        tok = self.currentTok
        self.log(f"Initiating factor");

        if tok.type in (TT_PLUS, TT_MINUS):
            self.log(f"Found Plus/Minus")
            res.registerAdvance(self.advance())
            atom = res.register(self.atom())
            if res.error: return res
            return res.success(UnaryOpNode(tok, atom))

        return self.pow()

    def term(self):
        self.log("Initiating term");
        return self.binOp(self.factor, (TT_MUL, TT_DIV))

    def arithExpr(self):
        self.log("initiating arithExpr");
        return self.binOp(self.term, (TT_PLUS, TT_MINUS))

    def compExpr(self):
        self.log(f"Initiating a compExpr")
        res = ParseResult()

        if self.currentTok.matches(TT_KEYWORD, "not"):
            self.log("Found 'not' keyword")
            opTok = self.currentTok
            res.registerAdvance(self.advance())

            node = res.register(self.compExpr())
            if res.error: return res
            self.log(f"Not Node")
            return res.success(UnaryOpNode(opTok, node))

        node = res.register(self.binOp(self.arithExpr, (TT_EQ, TT_NE, TT_LT, TT_GT, TT_LTE, TT_GTE)))

        if res.error:
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected int, float, identifier, string, '+', '-', 'not' or '('"
            ))

        return res.success(node)

    def expr(self):
        self.log(f"Initiating an expr")
        res = ParseResult()

        if self.currentTok.matches(TT_KEYWORD, "var"):
            self.log("Found var keyword");
            res.registerAdvance(self.advance())

            if self.currentTok.type != TT_IDENTIFIER:
                self.log("Identifier not found");
                return res.failure(InvalidSyntaxError(
                    self.currentTok.posStart, self.currentTok.posEnd,
                    "Expected Identifier",
                ))

            varName = self.currentTok
            self.log(f"Found var identifier; '{varName.value}'");
            res.registerAdvance(self.advance())

            if self.currentTok.type != TT_SET:
                self.log("Set Token not found");
                return res.failure(InvalidSyntaxError(
                    self.currentTok.posStart, self.currentTok.posEnd,
                    "Expected ':'"
                ))

            self.log("Found Set Token")

            res.registerAdvance(self.advance())
            expr = res.register(self.expr())
            if res.error: return res
            return res.success(VarAssignNode(varName, expr))

        node = res.register(self.binOp(self.compExpr, ((TT_KEYWORD, "and"), (TT_KEYWORD, "or"))))

        if res.error:
            return res.failure(InvalidSyntaxError(
                self.currentTok.posStart, self.currentTok.posEnd,
                "Expected 'var', int, float, identifier, '+', '-', or '('"
            ))

        return res.success(node)

    def pow(self):
        self.log("Initiating pow operator");
        return self.binOp(self.atom, (TT_POW, TT_BLNK), self.factor)

    def binOp(self, func, ops, funcB=None):
        self.log("Initiating BinOp Operations");
        if funcB is None:
            self.log("FuncB not found, falling back to funcA");
            funcB = func
        res = ParseResult()
        left = res.register(func())
        if res.error: return res

        while self.currentTok.type in ops or (self.currentTok.type, self.currentTok.value) in ops:
            self.log(f"CurrentToken is in [{ops}] (means that it is a opTok)")
            opTok = self.currentTok
            self.advance()
            right = res.register(funcB())
            if res.error: return res
            left = BinOpNode(left, opTok, right)

        return res.success(left)
