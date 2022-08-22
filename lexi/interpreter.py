from dataclasses import dataclass
from lexer import *
from parser import *


class Interpreter:
    def visit(self, node, context):
        methodName = f"visit_{type(node).__name__}"
        method = getattr(self, methodName, self.noVisitMethod)
        return method(node, context)

    def noVisitMethod(self, node, context):
        raise Exception(f"No visit_{type(node).__name__} method defined!")

    def visit_NumberNode(self, node, context):
        return RTResult().success(
            Number(node.tok.value).setContext(context).setPos(node.posStart, node.posEnd)
        )

    def visit_VarAccessNode(self, node, context):
        res = RTResult()
        varName = node.varNameTok.value
        value = context.symbolTable.get(varName)

        if not value:
            return res.failure(RTError(
                node.posStart, node.posEnd,
                f"Object '{varName}' is not defined",
                context
            ))

        value = value.copy().setPos(node.posStart, node.posEnd)
        return res.success(value)

    def visit_VarAssignNode(self, node, context):
        res = RTResult()
        varName = node.varNameTok.value
        value = res.register(self.visit(node.valueNode, context))
        if res.error: return res

        context.symbolTable.set(varName, value)
        return res.success(value)

    def visit_BinOpNode(self, node, context):
        res = RTResult()
        left = res.register(self.visit(node.leftNode, context))
        if res.error: return res
        right = res.register(self.visit(node.rightNode, context))

        if node.opTok.type == TT_PLUS:
            result, error = left.addedTo(right)
        if node.opTok.type == TT_MINUS:
            result, error = left.subbedBy(right)
        if node.opTok.type == TT_MUL:
            result, error = left.multedBy(right)
        if node.opTok.type == TT_DIV:
            result, error = left.divedBy(right)
        if node.opTok.type == TT_POW:
            result, error = left.powedBy(right)

        if error:
            return res.failure(error)
        else:
            return res.success(result.setPos(node.posStart, node.posEnd))

    def visit_UnaryOpNode(self, node, context):
        res = RTResult()
        number = res.register(self.visit(node.node, context))
        if res.error: return res

        error = None

        if node.opTok.type == TT_MINUS:
            number, error = number.multedBy(Number(-1))

        if error:
            return res.failure(error)
        else:
            return res.success(number.setPos (node.posStart, node.posEnd))


@dataclass()
class Number:
    value: float

    def __post_init__(self):
        self.setPos()
        self.setContext()

    def setPos(self, posStart=None, posEnd=None):
        self.posStart = posStart
        self.posEnd = posEnd
        return self

    def setContext(self, context=None):
        self.context = context
        return self

    def addedTo(self, other):
        if isinstance(other, Number):
            return Number(self.value + other.value).setContext(self.context), None

    def subbedBy(self, other):
        if isinstance(other, Number):
            return Number(self.value - other.value).setContext(self.context), None

    def multedBy(self, other):
        if isinstance(other, Number):
            return Number(self.value * other.value).setContext(self.context), None

    def divedBy(self, other):
        if isinstance(other, Number):
            if other.value == 0:
                return None, RTError(other.posStart, other.posEnd, "Division by zero", self.context)
            return Number(self.value / other.value).setContext(self.context), None

    def copy(self):
        copy = Number(self.value)
        copy.setPos(self.posStart, self.posEnd)
        copy.setContext(self.context)
        return copy

    def powedBy(self, other):
        if isinstance(other, Number):
            return Number(self.value ** other.value).setContext(self.context), None

    def __repr__(self):
        return str(self.value)


@dataclass()
class RTResult:
    value: vars = None
    error: vars = None

    def register(self, res):
        if res.error: self.error = res.error
        return res.value

    def success(self, value):
        self.value = value
        return self

    def failure(self, error):
        self.error = error
        return self
