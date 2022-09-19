from dataclasses import dataclass
from error import *
import string
from debug import Debug

# lists all available tokens
TT_INT = "INT";
TT_FLOAT = "FLOAT";
TT_PLUS = "PLUS";
TT_MINUS = "MINUS";
TT_MUL = "MUL";
TT_DIV = "DIV";
TT_LPAREN = "LPAREN";
TT_RPAREN = "RPAREN";
TT_SET = "SET";
TT_IDENTIFIER = "IDENTIFIER";
TT_KEYWORD = "KEYWORD";
TT_POW = "POW";
TT_BLNK = "BLNK";
TT_EQ = "EQ";
TT_EE = "EE";
TT_NE = "NE";
TT_LT = "LT";
TT_GT = "GT";
TT_LTE = "LTE";
TT_GTE = "GTE";
TT_TET = "TET";
TT_EOF = "EOF";

KEYWORDS = [
    "var",
    "if",
    "stru",
    "and",
    "or",
    "not",
    "if",
    "elif",
    "else",
]


class Token:  # Data class to represent the token
    def __init__(self, type, value=None, posStart=None, posEnd=None, debug=None):
        self.type = type
        self.value = value
        self.debug = debug.register(self);

        if posStart:
            self.posStart = posStart.copy()
            self.posEnd = posStart.copy()
            self.posEnd.advance()

        if posEnd:
            self.posEnd = posEnd

        self.debug.register(f"Token '{self}' found at location [{self.posStart}]");

    def matches(self, type_, value):
        return self.type == type_ and self.value == value

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
LETTERS = string.ascii_letters
LETTERS_DIGITS = LETTERS + DIGITS


class Lexer:
    def __init__(self, fn, text):  # runs on initialisation
        self.text = text  # sets the text of the lexer
        self.pos = Position(-1, 0, -1, fn, text)  # sets the current position of the lexer
        self.currentChar = None  # contains the current character
        self.debug = Debug(self);
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
            elif self.currentChar in LETTERS:
                tokens.append(self.makeIdentifier())
            elif self.currentChar == "+":
                tokens.append(Token(TT_PLUS, posStart=self.pos, debug=self.debug))
                self.advance()
            elif self.currentChar == "-":
                tokens.append(Token(TT_MINUS, posStart=self.pos, debug=self.debug))
                self.advance()
            elif self.currentChar == "*":
                tokens.append(Token(TT_MUL, posStart=self.pos, debug=self.debug))
                self.advance()
            elif self.currentChar == "/":
                tokens.append(Token(TT_DIV, posStart=self.pos, debug=self.debug))
                self.advance()
            elif self.currentChar == "(":
                tokens.append(Token(TT_LPAREN, posStart=self.pos, debug=self.debug))
                self.advance()
            elif self.currentChar == ")":
                tokens.append(Token(TT_RPAREN, posStart=self.pos, debug=self.debug))
                self.advance()
            elif self.currentChar == ":":
                tokens.append(Token(TT_SET, posStart=self.pos, debug=self.debug))
                self.advance()
            elif self.currentChar == "^":
                tok, error = self.makePower();
                if error: return [], error;
                tokens.append(tok);
            elif self.currentChar == "=":
                tok, error = self.makeEquals();
                if error: return [], error;
                tokens.append(tok);
            elif self.currentChar == "!":
                tok, error = self.makeNotEquals()
                if error: return [], error
                tokens.append(tok)
            elif self.currentChar == "<":
                tokens.append(self.makeLessThan())
            elif self.currentChar == ">":
                tokens.append(self.makeGreaterThan())
            else:
                posStart = self.pos.copy()
                char = self.currentChar
                self.advance()
                return [], IllegalCharError(posStart, self.pos, f"'{char}'")  # returns a fucking error

        tokens.append(Token(TT_EOF, posStart=self.pos, debug=self.debug))
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
            return Token(TT_INT, int(numStr), posStart, self.pos, debug=self.debug)
        else:
            return Token(TT_FLOAT, float(numStr), posStart, self.pos, debug=self.debug)

    def makeIdentifier(self):
        idStr = ""
        posStart = self.pos.copy()

        while self.currentChar is not None and self.currentChar in LETTERS_DIGITS + "_":
            idStr += self.currentChar
            self.advance()

        tokType = TT_KEYWORD if idStr in KEYWORDS else TT_IDENTIFIER
        return Token(tokType, idStr, posStart, self.pos, debug=self.debug)

    def makeNotEquals(self):
        posStart = self.pos.copy()
        self.advance()

        if self.currentChar == "=":
            self.advance();
            return Token(TT_NE, posStart=posStart, posEnd=self.pos, debug=self.debug), None

        self.advance()
        return None, ExpectedCharError(posStart, self.pos, "'=' (after '!')")

    def makeEquals(self):
        tokType = TT_EQ;
        posStart = self.pos.copy();
        self.advance();

        if self.currentChar == '=':
            self.advance();
            tokType = TT_EE;
            
        return Token(tokType, posStart=posStart, posEnd=self.pos, debug=self.debug), None;

    def makePower(self):
        tokType = TT_POW;
        posStart = self.pos.copy();
        self.advance();

        if self.currentChar == "^":
            self.advance();
            tokType = TT_TET;
            return Token(tokType, posStart=posStart, posEnd=self.pos, debug=self.debug), None;
        
        return Token(tokType, posStart=posStart, posEnd=self.pos, debug=self.debug), None;

    def makeLessThan(self):
        tokType = TT_LT
        posStart = self.pos.copy()
        self.advance()

        if self.currentChar == "=":
            self.advance()
            tokType = TT_LTE

        return Token(tokType, posStart=posStart, posEnd=self.pos, debug=self.debug)

    def makeGreaterThan(self):
        tokType = TT_GT
        posStart = self.pos.copy()
        self.advance()

        if self.currentChar == "=":
            self.advance()
            tokType = TT_GTE

        return Token(tokType, posStart=posStart, posEnd=self.pos, debug=self.debug)
