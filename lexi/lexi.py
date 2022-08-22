from lexer import *
from parser import *
from interpreter import *


global_symbol_table = SymbolTable()
global_symbol_table.set("null", Number(0))


def run(fn, text):
    lexer = Lexer(fn, text)
    tokens, error = lexer.makeTokens()
    if error: return None, error

    parser = Parser(tokens)
    ast = parser.parse()
    if ast.error: return None, ast.error

    interpreter = Interpreter()
    context = Context("<program>")
    context.symbolTable = global_symbol_table
    result = interpreter.visit(ast.node, context)

    return result.value, result.error
