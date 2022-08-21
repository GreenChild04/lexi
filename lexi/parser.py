from dataclasses import dataclass
from lexer import *


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

    def register(self, res):
        if isinstance(res, ParseResult):
            if res.error: self.error = res.error
            return res.node

        return res

    def success(self, node):
        self.node = node
        return self

    def failure(self, error):
        self.error = error
        return self


class Parser:
    def __init__(self, tokens):
        self.tokens = tokens
        self.tokIdx = -1
        self.currentTok = None
        self.advance()

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

        if tok.type == TT_LPAREN:
            res.register(self.advance())
            expr = res.register(self.expr())
            if res.error: return res
            if self.currentTok.type == TT_RPAREN:
                res.register(self.advance())
                return res.success(expr)
            else:
                return res.failure(InvalidSyntaxError(
                    self.currentTok.posStart, self.currentTok.posEnd,
                    "Expected ')' to end parentheses"
                ))

        elif tok.type in (TT_INT, TT_FLOAT):
            res.register(self.advance())
            return res.success(NumberNode(tok))

        return res.failure(InvalidSyntaxError(tok.posStart, tok.posEnd, "Expected int or float"))

    def factor(self):
        res = ParseResult()
        tok = self.currentTok

        if tok.type in (TT_PLUS, TT_MINUS):
            res.register(self.advance())
            atom = res.register(self.atom())
            if res.error: return res
            return res.success(UnaryOpNode(tok, atom))

        else:
            redirect = self.atom()
            res.register(redirect)
            if res.error: return res
            return res.success(redirect.node)

    def term(self):
        return self.binOp(self.factor, (TT_MUL, TT_DIV))

    def expr(self):
        return self.binOp(self.term, (TT_PLUS, TT_MINUS))

    def pow(self):
        return self.binOp(self.factor, (TT_POW, TT_BLNK))

    def binOp(self, func, ops):
        res = ParseResult()
        left = res.register(func())
        if res.error: return res

        while self.currentTok.type in ops:
            opTok = self.currentTok
            self.advance()
            right = res.register(func())
            if res.error: return res
            left = BinOpNode(left, opTok, right)

        return res.success(left)
