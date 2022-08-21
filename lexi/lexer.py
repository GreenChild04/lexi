from dataclasses import dataclass
from error import *

# lists all available tokens
TT_INT = "INT"
TT_FLOAT = "FLOAT"
TT_PLUS = "PLUS"
TT_MINUS = "MINUS"
TT_MUL = "MUL"
TT_DIV = "DIV"
TT_LPAREN = "LPAREN"
TT_RPAREN = "RPAREN"
TT_SET = "SET"
TT_POW = "POW"
TT_BLNK = "BLNK"
TT_EOF = "EOF"


class Token:  # Data class to represent the token
    def __init__(self, type, value=None, posStart=None, posEnd=None):
        self.type = type
        self.value = value

        if posStart:
            self.posStart = posStart.copy()
            self.posEnd = posStart.copy()
            self.posEnd.advance()

        if posEnd:
            self.posEnd = posEnd

    def __repr__(self):  # returns the token in a formatted way
        if self.value: return f"{self.type}:{self.value}"
        return f"{self.type}"


@dataclass()
class Position:
    idx: int
    ln: int
    col: int
    fn: str
    ftxt: str

    def advance(self, currentChar=None):
        self.idx += 1
        self.col += 1

        if currentChar == "\n":
            self.ln += 1
            self.col = 0

        return self

    def copy(self):
        return Position(self.idx, self.ln, self.col, self.fn, self.ftxt)


DIGITS = "0123456789"  # sets the digits constant


class Lexer:
    def __init__(self, fn, text):  # runs on initialisation
        self.text = text  # sets the text of the lexer
        self.pos = Position(-1, 0, -1, fn, text)  # sets the current position of the lexer
        self.currentChar = None  # contains the current character
        self.advance()  # advances though the string

    def advance(self):  # used to advance though user input
        self.pos.advance(self.currentChar)  # increases the current position
        self.currentChar = self.text[self.pos.idx] if self.pos.idx < len(self.text) else None  # sets the currentChar

    def makeTokens(self):  # creates the needed tokens
        tokens = []  # the creation of the token list

        while self.currentChar is not None:  # adds tokens to the token list by checking currentChar
            if self.currentChar in " \t":
                self.advance()
            elif self.currentChar in DIGITS:
                tokens.append(self.makeNum())
            elif self.currentChar == "+":
                tokens.append(Token(TT_PLUS, posStart=self.pos))
                self.advance()
            elif self.currentChar == "-":
                tokens.append(Token(TT_MINUS, posStart=self.pos))
                self.advance()
            elif self.currentChar == "*":
                tokens.append(Token(TT_MUL, posStart=self.pos))
                self.advance()
            elif self.currentChar == "/":
                tokens.append(Token(TT_DIV, posStart=self.pos))
                self.advance()
            elif self.currentChar == "^":
                tokens.append(Token(TT_POW, posStart=self.pos))
                self.advance()
            elif self.currentChar == "(":
                tokens.append(Token(TT_LPAREN, posStart=self.pos))
                self.advance()
            elif self.currentChar == ")":
                tokens.append(Token(TT_RPAREN, posStart=self.pos))
                self.advance()
            elif self.currentChar == ":":
                tokens.append(Token(TT_SET, posStart=self.pos))
                self.advance()
            else:
                posStart = self.pos.copy()
                char = self.currentChar
                self.advance()
                return [], IllegalCharError(posStart, self.pos, f"'{char}'")  # returns a fucking error

        tokens.append(Token(TT_EOF, posStart=self.pos))
        return tokens, None

    def makeNum(self):
        numStr = ""
        dotCount = 0
        posStart = self.pos.copy()

        while self.currentChar is not None and self.currentChar in DIGITS + ".":
            if self.currentChar == ".":
                if dotCount == 1: break
                dotCount += 1
                numStr += "."
            else:
                numStr += self.currentChar
            self.advance()

        if dotCount == 0:
            return Token(TT_INT, int(numStr), posStart, self.pos)
        else:
            return Token(TT_FLOAT, float(numStr), posStart, self.pos)
