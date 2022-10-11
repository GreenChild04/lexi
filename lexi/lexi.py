'''Library Imports'''
from dataclasses import dataclass
from termcolor import colored
from time import time;
import sys, os

'''Component Imports'''
from lexer import *
from parser import *
from interpreter import *
from debug import *


global_symbol_table = SymbolTable()
global_symbol_table.set("null", Null())
global_symbol_table.set("true", Boolean(True))
global_symbol_table.set("false", Boolean(False))
global_symbol_table.set("print", BuiltInFunction("print"));
global_symbol_table.set("input", BuiltInFunction("input"));
global_symbol_table.set("clear", BuiltInFunction("clear"));
global_symbol_table.set("type", BuiltInFunction("type"));


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


'''(Command Line Manager) Used to read programming in the command line without files'''
class CLM:
    def __init__(self):
        self.file = {};
        self.digits = "0123456789";
        self.memory = {};

    def run(self):
        while True:
            print();
            try: text = input("<lexi#>");
            except:
                os.system("clear");
                sys.exit();

            print();
            
            self.cmdRead(text);

    def checkIn(self, new, old):
        if new == "" or new == None:
            return False;
        for i in new:
            if i not in old:
                break;
        else:
            return True;
        return False;

    def cmdRead(self, cmd):
        if "`" in cmd:
            sp = cmd.split("`");
            name = list(sp)[0];
            try:
                data = sp[1];
            except: data = None;
            if self.checkIn(name, self.digits):
                self.setNum(int(name), data);
            elif "`" in cmd:
                methodName = f"cmd_{data}";
                getattr(self, methodName, self.cmd_no)();
        else: 
            self.runFile([cmd]);

    def order(self):
        nums = [];
        nonos = [];
        for i in range(len(self.file)):
            self.memory["_order"] = 100;
            file = [];

            for idx in self.file:
                file.append(idx);
            
            for a in self.listRemove(file, nonos):
                self.memory["_order"] = a if a < self.memory["_order"] else self.memory["_order"];
            nums.append(self.memory["_order"]);

            nonos.append(self.memory["_order"]);
        return nums;

    def listRemove(self, lst, idx):
        try:
            if isinstance(idx, list):
                new = lst;
                for i in idx:
                    new.remove(i);
                return new;
            else:
                new = lst;
                new.remove(idx);
                return new;
        except: return lst;

    def runFile(self, data):
        startTime = time();

        if len(data) < 1:
            print("Error: Nothing to run");
        else:
            final = data[0];
            try:
                for i in data[1:]:
                    final += f"\n{i}";
            except: pass

            result, error = run("<stdin>", final);

            if Debug().debugRun:
                print();
                if error: print(colored("Error: ", "red") + error.asString());
                elif result: print(colored(f"Success in [{time() - startTime}]: ", "green") + repr(result));
            else:
                if error: print(error.asString());
                elif result: print(repr(result));

    def cmd_run(self):
        ordered = self.order();
        compact = [];
        for i in ordered:
            compact.append(self.file[i]);
        self.runFile(compact);

    def cmd_ls(self):
        file = self.order();
        for i in file:
            dash = colored(i, "blue");
            print(f"{dash} {self.file[i]}");
    
    def cmd_clear(self):
        self.file = {};
        print("Cleared File");

    def setNum(self, idx, data):
        self.file[idx] = data;

    def cmd_no(self):
        print("Error: Command not found!");
