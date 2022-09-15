import lexi
from termcolor import colored
import sys

while True:
    print();
    try: text = input("<lexi#>");
    except:
        print("\n");
        sys.exit();
    result, error = lexi.run("<stdin>", text);

    print();

    if error: print(colored("Error: ", "red") + error.asString());
    else: print(colored("Success: ", "green") + str(result.value));
