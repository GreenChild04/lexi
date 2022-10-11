from dataclasses import dataclass
from termcolor import colored


@dataclass()
class Debug:
    origin: vars = None;

    def __post_init__(self):
        self.name = type(self.origin).__name__;
        self.objects = ("Debug", "Parser", "Lexer", "Token", "Interpreter", "IfNode");
        self.logNum = 0;
        self.debugRun = False;

    def register(self, res):
        if isinstance(res, Debug):
            self.name = res.name;
            self.objects = res.objects;
            return self
        elif type(res).__name__ in self.objects:
            self.origin = res;
            self.name = type(res).__name__;
            return self

        if self.debugRun:
            if isinstance(res, str):
                self.log(res);
            elif isinstance(res, list):
                for i in res:
                    self.pLog(i);
            else:
                raise Exception(f"Debug Error: Cannot Register [{res}] Object")

    def log(self, msg):
        if self.objects.__contains__(self.name):
            if self.logNum == 0:
                print("");
            print(colored(f"Debug [{self.name}]({self.logNum}): ", "yellow") + msg);
            self.logNum += 1;
        else:
            pass;
    
    def pLog(self, msg):
        if self.objects.__contains__(self.name):
            if self.logNum == 0:
                print("");
            print(colored(f"Debug [{self.name}]({self.logNum}): ", "blue") + msg);
            self.logNum += 1;
        else:
            pass;

    def copy(self):
        return Debug(self.origin);
