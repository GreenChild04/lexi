import lexi

while True:
    print()
    text = input("<lexi#>")
    result, error = lexi.run("<stdin>", text)

    print()

    if error: print(error.asString())
    else: print(result)
