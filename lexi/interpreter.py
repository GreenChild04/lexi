from dataclasses import dataclass
from lexer import *
from parser import *
from debug import Debug
import os;


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

    def visit_ListNode(self, node, context):
        res = RTResult();
        elements = [];

        for elementNode in node.elementNodes:
            elements.append(res.register(self.visit(elementNode, context)));
            if res.error: return res;

        return res.success(
            List(elements).setContext(context).setPos(node.posStart, node.posEnd)
        );

    def visit_TupleNode(self, node, context):
        res = RTResult();
        elements = [];

        for elementNode in node.elementNodes:
            elements.append(res.register(self.visit(elementNode, context)));
            if res.error: return res;

        return res.success(
            Tuple(elements).setContext(context).setPos(node.posStart, node.posEnd)
        );

    def visit_IterNode(self, node, context):
        res = RTResult();
        elements = res.register(self.visit_TupleNode(node, context));
        if res.error: return res;
        return res.success(elements);

    def visit_CurlNode(self, node, context):
        res = RTResult();
        elements = [];

        elements.extend(node.elementNodes);

        return res.success(
            Curl(elements).setContext(context).setPos(node.posStart, node.posEnd)
        );

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

        value = value.copy().setPos(node.posStart, node.posEnd).setContext(context);
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
        elements = [];

        while True:
            condition = res.register(self.visit(node.condition, context));
            if res.error: return res;
            
            if not condition.isTrue(): break;

            elements.append(res.register(self.visit(node.body, context)));
            if res.error: return res;

        return res.success(
            List(elements).setContext(context).setPos(node.posStart, node.posEnd)
        );

    def visit_FuncDefNode(self, node, context):
        res = RTResult();

        funcName = node.varNameTok.value if node.varNameTok else None;
        curlNode = res.register(self.visit(node.curlNode, context));
        if res.error: return res;
        argNames = [argName.value for argName in node.argNameToks];
        funcValue = Function(funcName, curlNode, argNames).setContext(context).setPos(node.posStart, node.posEnd);

        if node.varNameTok:
            context.symbolTable.set(funcName, funcValue);

        return res.success(funcValue);

    def visit_ListConvNode(self, node, context):
        res = RTResult();
        args = [];
        nodeToConv = res.register(self.visit(node.nodeToConv, context));
        if res.error: return res;
        nodeToConv = nodeToConv.copy().setPos(node.posStart, node.posEnd);

        for argNode in node.argNodes:
            args.append(res.register(self.visit(argNode, context)));
            if res.error: return res;

        returnValue = res.register(nodeToConv.convList(args));
        if res.error: return res;

        return res.success(returnValue);

    def visit_CurlConvNode(self, node, context):
        res = RTResult();
        args = [];
        nodeToConv = res.register(self.visit(node.nodeToConv, context));
        if res.error: return res;
        nodeToConv = nodeToConv.copy().setPos(node.posStart, node.posEnd);

        for argNode in node.argNodes:
            args.append(res.register(self.visit(argNode, context)));
            if res.error: return res;

        returnValue = res.register(nodeToConv.convCurl(args));
        if res.error: return res;

        return res.success(returnValue);

    def visit_ParenConvNode(self, node, context):
        res = RTResult();
        args = [];
        nodeToConv = res.register(self.visit(node.nodeToConv, context));
        if res.error: return res;
        nodeToConv = nodeToConv.copy().setPos(node.posStart, node.posEnd);

        for argNode in node.argNodes:
            args.append(res.register(self.visit(argNode, context)));
            if res.error: return res;

        returnValue = res.register(nodeToConv.convParen(args));
        if res.error: return res;

        return res.success(returnValue);

    def visit_CallNode(self, node, context):
        res = RTResult();
        args = [];

        valueToCall = res.register(self.visit(node.nodeToCall, context));
        if res.error: return res;
        valueToCall = valueToCall.copy().setPos(node.posStart, node.posEnd);

        for argNode in node.argNodes:
            args.append(res.register(self.visit(argNode, context)));
            if res.error: return res;

        returnValue = res.register(valueToCall.execute(context, args=args));
        if res.error: return res;
        returnValue = returnValue.copy().setPos(node.posStart, node.posEnd).setContext(context);

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


class RTResult:
    def __init__(self) -> None:
        self.reset();

    def reset(self):
        self.value = None;
        self.error = None;
        self.returnValue = None;

    def register(self, res):
        if res.error: self.error = res.error
        self.returnValue = res.returnValue;
        return res.value

    def success(self, value):
        self.reset();
        self.value = value
        return self

    def successReturn(self, value):
        self.reset();
        self.returnValue = value;
        return self;

    def failure(self, error):
        self.reset();
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
        self.attributes = (("None", None),);

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
        return Boolean(False), None

    def getComparisonNE(self, other):
        return Boolean(True), None

    def getComparisonLT(self, other):
        return None, self.illegalOperation(other);

    def getComparisonGT(self, other):
        return None, self.illegalOperation(other);

    def getComparisonLTE(self, other):
        return None, self.illegalOperation(other);

    def getComparisonGTE(self, other):
        return None, self.illegalOperation(other);

    def notted(self):
        return Boolean(not self.isTrue()), None;

    def convList(self, args):
        return RTResult().failure(self.illegalOperation());

    def convCurl(self, args):
        return RTResult().failure(self.illegalOperation());

    def convParen(self, args):
        return RTResult().failure(self.illegalOperation());

    def execute(self, args):
        return RTResult().failure(self.illegalOperation());

    def isTrue(self):
        return True;

    def findAttribute(self, attribute):
        for i in self.attributes:
            if i[0] == attribute:
                return i[1];
        return None;

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
        return copy;

    def getComparisonEQ(self, other):
        if isinstance(other, Null):
            return Boolean(True), None;
        else:
            return Boolean(False), None;

    def getComparisonNE(self, other):
        if isinstance(other, Null):
            return Boolean(False), None;
        else:
            return Boolean(True), None;

    def isTrue():
        return False;
    
    def __repr__(self):
        return "<null>"

class Boolean(Value):
    def __init__(self, value):
        super().__init__()
        self.value = value;

    def isTrue(self):
        return self.value;

    def getComparisonEQ(self, other):
        if isinstance(other, Boolean):
            return Boolean(self.value == other.value), None
        else:
            return Boolean(False), None

    def getComparisonNE(self, other):
        if isinstance(other, Boolean):
            return Boolean(self.value != other.value), None
        else:
            return Boolean(True), None

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
        self.attributes = (("None", None), ("itter", True));

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
            return Boolean(False), None;

    def getComparisonNE(self, other):
        if isinstance(other, String):
            return Boolean(self.value != other.value), None;
        else:
            return Boolean(True), None;

    def convList(self, args):
        res = RTResult();
        newList = [];

        if len(args) == 0:
            return res.success(List(self.value[:]).setContext(self.context).setPos(self.posStart, self.posEnd));

        for i in args:
            try:
                newList.append(String(self.value[i.value]).setContext(self.context).setPos(self.posStart, self.posEnd));
            except:
                return res.failure(RTError(
                    self.posStart, self.posEnd,
                    "Tuple index out of range",
                    self.context
                ));

        if len(newList) > 1:
            return res.success(Tuple(newList).setContext(self.context).setPos(self.posStart, self.posEnd));
        else:
            return res.success(newList[0]);

    def convCurl(self, args):
        res = RTResult();
        newCurl = [];

        if len(args) == 0:
            return res.success(Curl(self.value[:]).setContext(self.context).setPos(self.posStart, self.posEnd));

        for i in args:
            try:
                newCurl.append(String(self.value[i.value]).setContext(self.context).setPos(self.posStart, self.posEnd));
            except:
                return res.failure(RTError(
                    self.posStart, self.posEnd,
                    "Tuple index out of range",
                    self.context
                ));

        return res.success(Curl(newCurl).setContext(self.context).setPos(self.posStart, self.posEnd));

    def convParen(self, args):
        res = RTResult();
        newList = [];

        if len(args) == 0:
            return res.success(Tuple(self.value[:]).setContext(self.context).setPos(self.posStart, self.posEnd));

        for i in args:
            try:
                newList.append(String(self.value[i.value]).setContext(self.context).setPos(self.posStart, self.posEnd));
            except:
                return res.failure(RTError(
                    self.posStart, self.posEnd,
                    "Tuple index out of range",
                    self.context
                ));

        if len(newList) > 1:
            return res.success(Tuple(newList).setContext(self.context).setPos(self.posStart, self.posEnd));
        else:
            return res.success(newList[0]);

    def copy(self):
        copy = String(self.value);
        copy.setPos(self.posStart, self.posEnd);
        copy.setContext(self.context);
        return copy;

    def isTrue(self):
        return len(self.value) > 0;

    def __str__(self) -> str:
        return self.value;

    def __repr__(self):
        return f"\"{self.value}\"";

class List(Value):
    def __init__(self, elements):
        super().__init__();
        self.elements = elements;
        self.attributes = (("none", None), ("itter", True), ("collection", True));

    def addedTo(self, other):
        newList = self.copy();
        newList.elements.append(other);
        return newList, None;

    def multedBy(self, other):
        if isinstance(other, List):
            newList = self.copy();
            newList.elements.extend(other.elements);
            return newList, None;
        else:
            return None, self.illegalOperation(other);

    def subbedBy(self, other):
        if isinstance(other, Number):
            newList = self.copy();
            try:
                newList.elements.pop(other.value);
                return newList, None;
            except:
                return None, RTError(
                    other.posStart, other.posEnd,
                    "List index out of range"
                )
        else:
            return None, self.illegalOperation(other);

    def convList(self, args):
        res = RTResult();
        newList = [];
        
        if len(args) == 0:
            return res.success(self.copy());

        for i in args:
            try:
                newList.append(self.elements[i.value]);
            except:
                return res.failure(RTError(
                    self.posStart, self.posEnd,
                    f"List index out of range",
                    self.context
                ))

        if len(newList) > 1:
            return res.success(Tuple(newList).setContext(self.context).setPos(self.posStart, self.posEnd));
        else:
            return res.success(newList[0]);

    def convCurl(self, args):
        res = RTResult();
        newCurl = [];

        if len(args) == 0:
            return res.success(Curl(self.elements[:]).setContext(self.context).setPos(self.posStart, self.posEnd));
        
        for i in args:
            try:
                newCurl.append(self.elements[i.value]);
            except:
                return res.failure(RTError(
                    self.posStart, self.posEnd,
                    "List index out of range",
                    self.context
                ));

        return res.success(Curl(newCurl).setContext(self.context).setPos(self.posStart, self.posEnd));

    def convParen(self, args):
        res = RTResult();
        newTuple = [];

        if len(args) == 0:
            return res.success(Tuple(self.elements[:]).setContext(self.context).setPos(self.posStart, self.posEnd));

        for i in args:
            try:
                newTuple.append(self.elements[i.value]);
            except:
                return res.failure(RTError(
                    self.posStart, self.posEnd,
                    "Tuple index out of range",
                    self.context
                ));

        if len(newTuple) > 1:
            return res.success(Tuple(newTuple).setContext(self.context).setPos(self.posStart, self.posEnd));
        else:
            return res.success(newTuple[0])

    def copy(self):
        copy = List(self.elements);
        copy.setContext(self.context);
        copy.setPos(self.posStart, self.posEnd);
        return copy;

    def __repr__(self):
        return f"[{', '.join([str(x) for x in self.elements])}]";

class Tuple(Value):
    def __init__(self, elements):
        super().__init__();
        self.elements = elements;
        self.attributes = (("None", None), ("itter", True), ("collection", True));

    def convList(self, args):
        res = RTResult();
        newList = [];

        if len(args) == 0:
            return res.success(List(self.elements[:]).setContext(self.context).setPos(self.posStart, self.posEnd));

        for i in args:
            try:
                newList.append(self.elements[i.value]);
            except:
                return res.failure(RTError(
                    self.posStart, self.posEnd,
                    "Tuple index out of range",
                    self.context
                ));

        if len(newList) > 1:
            return res.success(Tuple(newList).setContext(self.context).setPos(self.posStart, self.posEnd));
        else:
            return res.success(newList[0])

    def convCurl(self, args):
        res = RTResult();
        newCurl = [];

        if len(args) == 0:
            return res.success(Curl(self.elements[:]).setContext(self.context).setPos(self.posStart, self.posEnd));

        for i in args:
            try:
                newCurl.append(self.elements[i.value]);
            except:
                return res.failure(RTError(
                    self.posStart, self.posEnd,
                    "Tuple index out of range",
                    self.context
                ));

        return res.success(Curl(newCurl).setContext(self.context).setPos(self.posStart, self.posEnd));

    def convParen(self, args):
        res = RTResult();
        newTuple = [];

        if len(args) == 0:
            return res.success(self.copy());

        for i in args:
            try:
                newTuple.append(self.elements[i.value]);
            except:
                return res.failure(RTError(
                    self.posStart, self.posEnd,
                    "Tuple index out of range",
                    self.context
                ));

        if len(newTuple) > 1:
            return res.success(Tuple(newTuple).setContext(self.context).setPos(self.posStart, self.posEnd));
        else:
            return res.success(newTuple[0])

    def copy(self):
        copy = Tuple(self.elements);
        copy.setContext(self.context);
        self.setPos(self.posStart, self.posEnd);
        return copy;
    
    def __repr__(self):
        return f"({', '.join([str(x) for x in self.elements])})";

class Curl(Value):
    def __init__(self, elements):
        super().__init__();
        self.elements = elements;
        self.attributes = (("None", None), ("itter", True), ("collection", True));

    def addedTo(self, other):
        newCurl = self.copy();
        newCurl.elements.append(other);
        return newCurl, None;

    def multedBy(self, other):
        if isinstance(other, Curl):
            newCurl = self.copy();
            newCurl.elements.extend(other.elements);
            return newCurl, None;
        else:
            return None, self.illegalOperation(other);

    def subbedBy(self, other):
        if isinstance(other, Number):
            newCurl = self.copy();
            try:
                newCurl.elements.pop(other.value);
                return newCurl, None;
            except:
                return None, RTError(
                    other.posStart, other.posEnd,
                    "Curl index out of range"
                )
        else:
            return None, self.illegalOperation(other);

    def convList(self, args):
        res = RTResult();
        interpreter = Interpreter();
        newList = [];

        if len(args) == 0:
            for i in self.elements:
                expr = res.register(interpreter.visit(i, self.context));
                if res.error: return res;
                newList.append(expr);
            return res.success(List(newList).setContext(self.context).setPos(self.posStart, self.posEnd));
        
        for i in args:
            try:
                element = self.elements[i.value];
                visited = res.register(interpreter.visit(element, self.context));
                newList.append(visited);
            except Exception as error:
                return res.failure(RTError(
                    self.posStart, self.posEnd,
                    "Curl index out of range",
                    self.context
                ))

        if len(newList) > 1:
            return res.success(Tuple(newList).setContext(self.context).setPos(self.posStart, self.posEnd));
        else:
            return res.success(newList[0]);

    def convCurl(self, args):
        res = RTResult();
        newCurl = [];

        if len(args) == 0:
            return res.success(self.copy());

        for i in args:
            try:
                newCurl.append(self.elements[i.value]);
            except:
                return res.failure(RTError(
                    self.posStart, self.posEnd,
                    "Curl index out of range",
                    self.context
                ));

        return res.success(Curl(newCurl).setContext(self.context).setPos(self.posStart, self.posEnd));

    def convParen(self, args):
        res = RTResult();
        interpreter = Interpreter();
        newParen = [];

        if len(args) == 0:
            for i in self.elements:
                expr = res.register(interpreter.visit(i, self.context));
                if res.error: return res;
                newParen.append(expr);
            return res.success(Tuple(newParen).setContext(self.context).setPos(self.posStart, self.posEnd));
        
        for i in args:
            try:
                element = self.elements[i.value];
                visited = res.register(interpreter.visit(element, self.context));
                newParen.append(visited);
            except Exception as error:
                return res.failure(RTError(
                    self.posStart, self.posEnd,
                    "Curl index out of range",
                    self.context
                ))

        if len(newParen) > 1:
            return res.success(Tuple(newParen).setContext(self.context).setPos(self.posStart, self.posEnd));
        else:
            return res.success(newParen[0]);

    def execute(self, context, args=[]):
        res = RTResult();
        interpreter = Interpreter();

        if len(args) > 0:
            return res.failure(RTError(
                self.postart, self.posEnd,
                f"{len(args)} too many args pass into curl",
            ))

        for i in self.elements:
            expr = res.register(interpreter.visit(i, context));
            if res.error: return res;
        return res.success(expr);

    def copy(self):
        copy = Curl(self.elements);
        copy.setContext(self.context);
        copy.setPos(self.posStart, self.posEnd);
        return copy;

    def __repr__(self):
        # return f"{'{'}{', '.join([str(x) for x in self.elements])}{'}'}";
        return f"<curl container>"

class BaseFunction(Value):
    def __init__(self, name):
        super().__init__();
        self.name = name or "<anonymous>";

    def generateNewContext(self):
        newContext = Context(self.name, self.context, self.posStart);
        newContext.symbolTable = SymbolTable(newContext.parent.symbolTable);
        return newContext;

    def checkArgs(self, argNames, args):
        res = RTResult();

        if len(args) > len(argNames):
            return res.failure(RTError(
                self.posStart, self.posEnd,
                f"{len(args) - len(self.argNames)} too many args passed into '{self.name}'",
                self.context
            ));

        if len(args) < len(argNames):
            return res.failure(RTError(
                self.posStart, self.posEnd,
                f"{len(argNames) - len(args)} too few args passed into '{self.name}'",
                self.context
            ));

        return res.success(None);

    def populateArgs(self, argNames, args, context):
        for i in range(len(args)):
            argName = argNames[i];
            argValue = args[i];
            argValue.setContext(context);
            context.symbolTable.set(argName, argValue);

    def checkAndPopulateArgs(self, argNames, args, context):
        res = RTResult();
        res.register(self.checkArgs(argNames, args));
        if res.error: return res;
        self.populateArgs(argNames, args, context);
        return res.success(None);

class BuiltInFunction(BaseFunction):
    def __init__(self, name):
        super().__init__(name);

    def execute(self, context, args):
        res = RTResult();
        newContext = self.generateNewContext();

        methodName = f"execute_{self.name}";
        method = getattr(self, methodName, self.noVisitMethod);

        res.register(self.checkAndPopulateArgs(method.argNames, args, newContext));
        if res.error: return res;

        returnValue = res.register(method(newContext));
        if res.error: return res;
        return res.success(returnValue);

    def convParen(self, args):
        res = RTResult();
        newContext = self.generateNewContext();

        methodName = f"execute_{self.name}";
        method = getattr(self, methodName, self.noVisitMethod);

        res.register(self.checkAndPopulateArgs(method.argNames, args, newContext));
        if res.error: return res;

        returnValue = res.register(method(newContext));
        if res.error: return res;
        return res.success(returnValue);

    def noVisitMethod(self, context):
        raise Exception(f"No execute_{self.name} method defined");

    def copy(self):
        copy = BuiltInFunction(self.name);
        copy.setContext(self.context);
        copy.setPos(self.posStart, self.posEnd);
        return copy;

    def __repr__(self):
        return f"<built-in function {self.name}>";

    '''Built in functions'''

    def execute_print(self, context):
        print(str(context.symbolTable.get("value")));
        return RTResult().success(Null());
    execute_print.argNames = ["value"];

    def execute_input(self, context):
        text = input(str(context.symbolTable.get("prompt")));
        return RTResult().success(String(text));
    execute_input.argNames = ["prompt"];

    def execute_clear(self, context):
        os.system("cls" if os.name == "nt" else "clear");
        return RTResult().success(Null);
    execute_clear.argNames = [];

    def execute_type(self, context):
        obj = context.symbolTable.get("object");
        return RTResult().success(String(type(obj).__name__));
    execute_type.argNames = ["object"];

    def execute_len(self, context):
        iter_ = context.symbolTable.get("iter");
        
        if not iter_.findAttribute("itter"):
            return RTResult().failure(RTError(
                self.posStart, self.posEnd,
                "Argument must be a itteratable",
                context,
            ));

        return RTResult().success(Number(len(iter_.elements)))
    execute_len.argNames = ["iter"];

    def execute_run(self, context):
        fn = context.symbolTable.get("fn");

        if not isinstance(fn, String):
            return RTResult.failure(RTError(
                self.posStart, self.posEnd,
                "Argument must be string",
                context
            ));

        fn = fn.value;

        try:
            with open(fn, "r") as file:
                script = file.read();
        except Exception as error:
            return RTResult().failure(RTError(
                self.posStart, self.posEnd,
                f"Failed to load script \"fn\"\n{str(error)}",
                context,
            ));

        from lexi import run;
        _, error = run(fn, script);

        if error:
            return RTResult().failure(RTError(
                self.posStart, self.posEnd,
                f"Failed to finish executing script \"{fn}\"\n{error.asString()}",
                context,
            ))

        return RTResult().success(Null());
    execute_run.argNames = ["fn"];

class Function(BaseFunction):
    def __init__(self, name, curlNode, argNames):
        super().__init__(name);
        self.body = curlNode;
        self.argNames = argNames;

    def execute(self, context, args):
        res = RTResult();
        interpreter = Interpreter();
        newContext = self.generateNewContext();

        res.register(self.checkAndPopulateArgs(self.argNames, args, newContext));
        if res.error: return res;

        if type(self.body) in (Curl,):
            value = res.register(self.body.execute(newContext));
            if res.error: return res;
        elif self.body.findAttribute("collection"):
            value = self.body.elements[0];
        else:
            value = self.body;

        return res.success(Null());

    def convCurl(self, args):
        res = RTResult();
        newCurl = [];

        if len(args) == 0:
            return res.success(Curl([self.body]).setContext(self.context).setPos(self.posStart, self.posEnd));

        for i in args:
            return res.failure(RTError(
                self.posStart, self.posEnd,
                "Index out of range",
                self.context
            ))

        return res.success(Curl(newCurl).setContext(self.context).setPos(self.posStart, self.posEnd));

    def convParen(self, args):
        res = RTResult();
        interpreter = Interpreter();
        newContext = self.generateNewContext();

        res.register(self.checkAndPopulateArgs(self.argNames, args, newContext));
        if res.error: return res;

        if type(self.body) in (Curl,):
            value = res.register(self.body.execute(newContext));
            if res.error: return res;
        elif self.body.findAttribute("collection"):
            value = self.body.elements[0];
        else:
            value = self.body;

        return res.success(Null());

    def isTrue(self):
        return True;

    def copy(self):
        copy = Function(self.name, self.body, self.argNames);
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
            return Boolean(False), None

    def getComparisonNE(self, other):
        if isinstance(other, Number):
            return Boolean(self.value != other.value).setContext(self.context), None
        else:
            return Boolean(True), None

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
