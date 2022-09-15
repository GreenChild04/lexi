import lexi
from termcolor import colored

while True:
    print()
    text = input("<lexi#>")
    result, error = lexi.run("<stdin>", text)

    print()

    if error: print(colored("Error: ", "red") + error.asString());
    else: print(colored("Success: ", "green") + str(result.value));
