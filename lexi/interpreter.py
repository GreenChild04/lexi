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

    def visit_StringNode(self, node, context):
        return RTResult().success(
            String(node.tok.value).setContext(context).setPos(node.posStart, node.posEnd)
        )

    def visit_VarAccessNode(self, node, context):
        res = RTResult();
        varName = node.varNameTok.value
        self.debug.register([f"Accessing var '{varName}'"]);
        value = context.symbolTable.get(varName)
        self.debug.register([f"Found value '{value}' for var '{varName}'"])
        self.debug.register([f"Found type '{type(value)} for var '{varName}'"])

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

    def visit_WhileNode(self, node, context):
        res = RTResult();

        while True:
            condition = res.register(self.visit(node.condition, context));
            if res.error: return res;
            
            if not condition.isTrue(): break;

            res.register(self.visit(node.body, context));
            if res.error: return res;

        return res.success(None);

    def visit_FuncDefNode(self, node, context):
        res = RTResult();

        funcName = node.varNameTok.value if node.varNameTok else None;
        bodyNode = node.bodyNode;
        argNames = [argName.value for argName in node.argNameToks];
        funcValue = Function(funcName, bodyNode, argNames).setContext(context).setPos(node.posStart, node.posEnd);

        if node.varNameTok:
            context.symbolTable.set(funcName, funcValue);

        return res.success(funcValue);

    def visit_CallNode(self, node, context):
        res = RTResult();
        args = [];

        valueToCall = res.register(self.visit(node.nodeToCall, context));
        if res.error: return res;
        valueToCall = valueToCall.copy().setPos(node.posStart, node.posEnd);

        for argNode in node.argNodes:
            args.append(res.register(self.visit(argNode, context)));
            if res.error: return res;

        returnValue = res.register(valueToCall.execute(args));
        if res.error: return res;

        return res.success(returnValue);

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


###########################
# Values
###########################

class Value:
    def __init__(self):
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
        return None, self.illegalOperation(other);

    def subbedBy(self, other):
        return None, self.illegalOperation(other);

    def multedBy(self, other):
        return None, self.illegalOperation(other);

    def divedBy(self, other):
        return None, self.illegalOperation(other);

    def andedBy(self, other):
        return None, self.illegalOperation(other);

    def oredBy(self, other):
        return None, self.illegalOperation(other);

    def powedBy(self, other):
        return None, self.illegalOperation(other);
    
    def tetedBy(self, other):
        return None, self.illegalOperation(other);

    def getComparisonEQ(self, other):
        return None, self.illegalOperation(other);

    def getComparisonNE(self, other):
        return None, self.illegalOperation(other);

    def getComparisonLT(self, other):
        return None, self.illegalOperation(other);

    def getComparisonGT(self, other):
        return None, self.illegalOperation(other);

    def getComparisonLTE(self, other):
        return None, self.illegalOperation(other);

    def getComparisonGTE(self, other):
        return None, self.illegalOperation(other);

    def notted(self):
        return None, self.illegal_operation();

    def execute(self, args):
        return RTResult().failure(self.illegalOperation())

    def isTrue(self):
        return False;

    def copy(self):
        raise Exception("No copy method defined")

    def illegalOperation(self, other=None):
        if not other: other = self
        return RTError(
            self.posStart, other.posEnd,
            "Illegal operation",
            self.context
        )

    def __repr__(self):
        return str()

class Null(Value):
    def copy(self):
        copy = Null();
        copy.setPos(self.posStart, self.posEnd);
        copy.setContext(self.context);
    
    def __repr__(self):
        return "<null>"

class Boolean(Value):
    def __init__(self, value):
        super().__init__()
        self.value = value;

    def notted(self):
        return Boolean(True if self.value == False else False).setContext(self.context), None

    def isTrue(self):
        return self.value;

    def copy(self):
        copy = Boolean(self.value);
        copy.setPos(self.posStart, self.posEnd);
        copy.setContext(self.context);
        return copy;

    def __repr__(self):
        return str(self.value).lower();

class String(Value):
    def __init__(self, value):
        super().__init__()
        self.value = value;

    def addedTo(self, other):
        if isinstance(other, String):
            return String(self.value + other.value).setContext(self.context), None;
        else:
            return None, self.illegalOperation(other);
    
    def multedBy(self, other):
        if isinstance(other, Number):
            return String(self.value * other.value).setContext(self.context), None;
        else:
            return None, self.illegalOperation(other);

    def getComparisonEQ(self, other):
        if isinstance(other, String):
            return Boolean(self.value == other.value), None;
        else:
            return None, self.illegalOperation(other);

    def getComparisonNE(self, other):
        if isinstance(other, String):
            return Boolean(self.value != other.value), None;
        else:
            return None, self.illegalOperation(other);

    def copy(self):
        copy = String(self.value);
        copy.setPos(self.posStart, self.posEnd);
        copy.setContext(self.context);
        return copy;

    def isTrue(self):
        return len(self.value) > 0;

    def __repr__(self):
        return self.value;

class Function(Value):
    def __init__(self, name, bodyNode, argNames):
        super().__init__();
        self.name = name or "<anonymous>";
        self.bodyNode = bodyNode;
        self.argNames = argNames;

    def execute(self, args):
        res = RTResult();
        interpreter = Interpreter();
        newContext = Context(self.name, self.context, self.posStart);
        newContext.symbolTable = SymbolTable(newContext.parent.symbolTable);

        if len(args) > len(self.argNames):
            return res.failure(RTError(
                self.posStart, self.posEnd,
                f"{len(args) - len(self.argNames)} too many args passed into '{self.name}'",
                self.context
            ));

        if len(args) < len(self.argNames):
            return res.failure(RTError(
                self.posStart, self.posEnd,
                f"{len(self.argNames) - len(args)} too few args passed into '{self.name}'",
                self.context
            ));

        for i in range(len(args)):
            argName = self.argNames[i];
            argValue = args[i];
            argValue.setContext(newContext);
            newContext.symbolTable.set(argName, argValue)

        value = res.register(interpreter.visit(self.bodyNode, newContext))
        if res.error: return res;
        return res.success(value);

    def isTrue(self):
        return True;

    def copy(self):
        copy = Function(self.name, self.bodyNode, self.argNames);
        copy.setContext(self.context);
        copy.setPos(self.posStart, self.posEnd);
        return copy;

    def __repr__(self):
        return f"<function {self.name}>"

class Number(Value):
    def __init__(self, value):
        super().__init__();
        self.value = value;

    def addedTo(self, other):
        if isinstance(other, Number):
            return Number(self.value + other.value).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);

    def subbedBy(self, other):
        if isinstance(other, Number):
            return Number(self.value - other.value).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);

    def multedBy(self, other):
        if isinstance(other, Number):
            return Number(self.value * other.value).setContext(self.context), None
        elif isinstance(other, Number):
            return String(self.value * other.value).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);

    def divedBy(self, other):
        if isinstance(other, Number):
            if other.value == 0:
                return None, RTError(other.posStart, other.posEnd, "Division by zero", self.context)
            return Number(self.value / other.value).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);

    def andedBy(self, other):
        if isinstance(other, Number):
            return Number(int(self.value and other.value)).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);

    def oredBy(self, other):
        if isinstance(other, Number):
            return Number(int(self.value or other.value)).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);

    def powedBy(self, other):
        if isinstance(other, Number):
            return Number(self.value ** other.value).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);
    
    def tetedBy(self, other):
        if isinstance(other, Number):

            def tet2(x, n):
                """ Tetration, ^nx, by recursion. """
                if n == 0:
                    return 1
                return x**tet2(x, n-1)

            
            return Number(tet2(self.value, other.value)).setContext(self.context), None;
        else:
            return None, self.illegalOperation(other);

    def getComparisonEQ(self, other):
        if isinstance(other, Number):
            return Boolean(self.value == other.value).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);

    def getComparisonNE(self, other):
        if isinstance(other, Number):
            return Boolean(self.value != other.value).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);

    def getComparisonLT(self, other):
        if isinstance(other, Number):
            return Boolean(self.value < other.value).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);

    def getComparisonGT(self, other):
        if isinstance(other, Number):
            return Boolean(self.value > other.value).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);

    def getComparisonLTE(self, other):
        if isinstance(other, Number):
            return Boolean(self.value <= other.value).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);

    def getComparisonGTE(self, other):
        if isinstance(other, Number):
            return Boolean(self.value >= other.value).setContext(self.context), None
        else:
            return None, self.illegalOperation(other);

    def isTrue(self):
        return True;

    def copy(self):
        copy = Number(self.value);
        copy.setPos(self.posStart, self.posEnd);
        copy.setContext(self.context);
        return copy;

    def __repr__(self):
        return str(self.value);
