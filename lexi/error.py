from dataclasses import dataclass
from strings_with_arrows import *
from object import *


@dataclass()
class Error:
    posStart: any
    posEnd: any
    name: str
    details: str

    def asString(self):
        result = f"Error[ {self.name}: {self.details} ]\n"
        result += f"File[ {self.posStart.fn} ], Line[ {self.posStart.ln + 1} ]"
        result += f"\n\n{string_with_arrows(self.posStart.ftxt, self.posStart, self.posEnd)}"
        return result


class IllegalCharError(Error):
    def __init__(self, posStart, posEnd, details):
        super().__init__(posStart, posEnd, "Illegal Character", details)


class InvalidSyntaxError(Error):
    def __init__(self, posStart, posEnd, details):
        super().__init__(posStart, posEnd, "Invalid Syntax", details)


class ExpectedCharError(Error):
    def __init__(self, posStart, posEnd, details):
        super().__init__(posStart, posEnd, "Expected Character", details)


class RTError(Error):
    def __init__(self, posStart, posEnd, details, context):
        super().__init__(posStart, posEnd, "Runtime Error", details)
        self.context = context

    def asString(self):
        result = self.generateTraceback()
        result += f"Error[ {self.name}: {self.details} ]\n"
        result += f"\n\n{string_with_arrows(self.posStart.ftxt, self.posStart, self.posEnd)}"
        return result

    def generateTraceback(self):
        result = ""
        pos = self.posStart
        ctx = self.context

        while ctx:
            result += f"    File[ {pos.fn} ], Line[ {pos.ln + 1} ], In[ {ctx.displayName} ]\n{result}"
            pos = ctx.parentEntryPos
            ctx = ctx.parent

        return f"Traceback (most recent call last): {'{'}\n{result}{'}'}\n"


@dataclass()
class Context:
    displayName: str
    parent: vars = None
    parentEntryPos: vars = None

    def __post_init__(self):
        self.symbolTable = None
