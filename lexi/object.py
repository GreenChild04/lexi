'''Library Imports'''
from dataclasses import dataclass

'''Component Imports'''
from debug import *;


###########################
# Parser Nodes
###########################

'''Functions'''

@dataclass
class FuncDefNode:
    varNameTok: vars;
    argNameToks: list;
    curlNode: vars;

    def __post_init__(self):
        if self.varNameTok:
            self.posStart = self.varNameTok.posStart;
        elif len(self.argNameToks) > 0:
            self.posStart = self.argNameToks[0].posStart;
        else:
            self.posStart = self.curlNode.posStart;
            
        self.posEnd = self.curlNode.posEnd;

@dataclass
class CallNode:
    nodeToCall: vars;
    argNodes: vars;

    def __post_init__(self):
        self.posStart = self.nodeToCall.posStart;
        if len(self.argNodes) > 0:
            self.posEnd = self.argNodes[-1].posEnd;
        else:
            self.posEnd = self.nodeToCall.posEnd;


'''Converts'''

@dataclass
class ConvNode:
    nodeToConv: vars;
    argNodes: vars;
    
    def __post_init__(self):
        self.posStart = self.nodeToConv.posStart;
        self.posEnd = self.argNodes[-1].posEnd if len(self.argNodes) > 0 else self.nodeToConv.posEnd;

class ListConvNode(ConvNode):
    pass;

class CurlConvNode(ConvNode):
    pass;

class ParenConvNode(ConvNode):
    pass;


'''Variables''';

@dataclass()
class VarAccessNode:
    varNameTok: vars

    def __post_init__(self):
        self.posStart = self.varNameTok.posStart
        self.posEnd = self.varNameTok.posEnd

@dataclass()
class VarModifyNode:
    varNameTok: vars;
    valueNode: vars;

    def __post_init__(self):
        self.posStart = self.varNameTok.posStart;
        self.posEnd = self.valueNode.posEnd;

@dataclass()
class VarAssignNode:
    varNameTok: vars
    valueNode: vars

    def __post_init__(self):
        self.posStart = self.varNameTok.posStart
        self.posEnd = self.valueNode.posEnd


###########################
# Symbol Table
###########################

@dataclass()
class SymbolTable:
    parent: vars = None
    symbols = {}

    def get(self, name):
        value = self.symbols.get(name, None)
        if value is None and self.parent:
            return self.parent.get(name)
        return value

    def set(self, name, value):
        self.symbols[name] = value

    def remove(self, name):
        del self.symbols[name]
