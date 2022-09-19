from dataclasses import dataclass
from lexer import *
from parser import *
from debug import Debug


class Interpreter:
    def __init__(self):
        self.debug = Debug(self);

    def visit(self, node, context):
        methodName = f"visit_{type(node).__name__}"
        method = getattr(self, methodName, self.noVisitMethod);
        res = method(node, context);
        self.debug.register(f"Visiting [{methodName}] method; Node [{node}]");
        return res;

    def noVisitMethod(self, node, context):
        raise Exception(f"No visit_{type(node).__name__} method defined!")

    def visit_NumberNode(self, node, context):
        return RTResult().success(
            Number(node.tok.value).setContext(context).setPos(node.posStart, node.posEnd)
        )

    def visit_VarAccessNode(self, node, context):
        res = RTResult()
        varName = node.varNameTok.value
        self.debug.register([f"Accessing var '{varName}'"]);
        value = context.symbolTable.get(varName)
        self.debug.register([f"Found value '{value}' for var '{varName}'"])

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

        self.debug.register([f"Setting varible '{varName}' to '{value}'"]);

        context.symbolTable.set(varName, value)
        return res.success(value)

    def visit_IfNode(self, node, context):
        res = RTResult();

        for condition, expr in node.cases:
            conditionValue = res.register(self.visit(condition, context));
            if res.error: return res;

            if conditionValue.isTrue():
                exprValue = res.register(self.visit(expr, context));
                if res.error: return res;
                return res.success(exprValue);

        return res.success(None);

    def visit_BinOpNode(self, node, context):
        res = RTResult()
        left = res.register(self.visit(node.leftNode, context))
        if res.error: return res
        right = res.register(self.visit(node.rightNode, context))
        error, result = None, None

        self.debug.register([f"Running {node.opTok.type} Operation"])

        if node.opTok.type == TT_PLUS:
            result, error = left.addedTo(right)
        elif node.opTok.type == TT_MINUS:
            result, error = left.subbedBy(right)
        elif node.opTok.type == TT_MUL:
            result, error = left.multedBy(right)
        elif node.opTok.type == TT_DIV:
            result, error = left.divedBy(right)
        elif node.opTok.type == TT_POW:
            result, error = left.powedBy(right)
        elif node.opTok.type == TT_TET:
            result, error = left.tetedBy(right);
        elif node.opTok.type == TT_EE:
            result, error = left.getComparisonEQ(right)
        elif node.opTok.type == TT_NE:
            result, error = left.getComparisonNE(right)
        elif node.opTok.type == TT_LT:
            result, error = left.getComparisonLT(right)
        elif node.opTok.type == TT_GT:
            result, error = left.getComparisonGT(right)
        elif node.opTok.type == TT_LTE:
            result, error = left.getComparisonLTE(right)
        elif node.opTok.type == TT_GTE:
            result, error = left.getComparisonGTE(right)
        elif node.opTok.matches(TT_KEYWORD, "and"):
            result, error = left.andedBy(right)
        elif node.opTok.matches(TT_KEYWORD, "or"):
            result, error = left.oredBy(right)
        elif node.opTok.matches(TT_KEYWORD, "not"):
            result, error = left.notted(right);

        if result is None and error is None:
            raise Exception(f"'{node.opTok.type}' was not defined or run")

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
            self.debug.register(["Found 'Minus' Token"]);
            number, error = number.multedBy(Number(-1))
        elif node.opTok.matches(TT_KEYWORD, "not"):
            self.debug.register(["Found 'Not' Keyword"]);
            number, error = number.notted()

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
        self.debug = Debug(self);

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

    def notted(self):
        return Number(1 if self.value == 0 else 0).setContext(self.context), None

    def andedBy(self, other):
        if isinstance(other, Number):
            return Number(int(self.value and other.value)).setContext(self.context), None

    def oredBy(self, other):
        if isinstance(other, Number):
            return Number(int(self.value or other.value)).setContext(self.context), None

    def powedBy(self, other):
        if isinstance(other, Number):
            return Number(self.value ** other.value).setContext(self.context), None
    
    def tetedBy(self, other):
        if isinstance(other, Number):

            def tet2(x, n):
                """ Tetration, ^nx, by recursion. """
                if n == 0:
                    return 1
                return x**tet2(x, n-1)

            
            return Number(tet2(self.value, other.value)).setContext(self.context), None;

    def getComparisonEQ(self, other):
        if isinstance(other, Number):
            return Number(int(self.value == other.value)).setContext(self.context), None

    def getComparisonNE(self, other):
        if isinstance(other, Number):
            return Number(int(self.value != other.value)).setContext(self.context), None

    def getComparisonLT(self, other):
        if isinstance(other, Number):
            return Number(int(self.value < other.value)).setContext(self.context), None

    def getComparisonGT(self, other):
        if isinstance(other, Number):
            return Number(int(self.value > other.value)).setContext(self.context), None

    def getComparisonLTE(self, other):
        if isinstance(other, Number):
            return Number(int(self.value <= other.value)).setContext(self.context), None

    def getComparisonGTE(self, other):
        if isinstance(other, Number):
            return Number(int(self.value >= other.value)).setContext(self.context), None

    def isTrue(self):
        return self.value != 0;

    def copy(self):
        copy = Number(self.value)
        copy.setPos(self.posStart, self.posEnd)
        copy.setContext(self.context)
        return copy

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
