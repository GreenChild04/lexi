tokens = {
	"EQ": "==",
	"NE": "!=",
	"LT": "<",
	"GT": ">",
	"LTE": "<=",
	"GTE": ">="
}

currentText = open("interpreter.py", "r").read()

currentInput = ""

def makeBuffer(num):
	return "    " * num

for i in tokens:
	currentInput += f"{makeBuffer(1)}def getComparison{i}(self, other):\n{makeBuffer(2)}if isinstance(other, Number):\n{makeBuffer(3)}return Number(int(self.value {tokens[i]} other.value)).setContext(self.context), None\n\n"

currentText = currentText.replace("~", currentInput)

print(currentText)
input("\n\nContinue?")

open("interpreter.py", "w+").write(currentText)
