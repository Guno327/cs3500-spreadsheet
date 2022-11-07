using FormulaEvaluator;

static int simpleEval(string s)
{
    return 1;
}


string testing;
int result;
try
{
    //Test empty
    try
    {
        testing = "";
        result = Evaluator.Evaluate(testing, null);
    }
    catch (ArgumentException e)
    {
        Console.WriteLine("Passed empty expression.");
    }
    //Test basic function no ws.
    testing = "2+2*10+8";
    result = Evaluator.Evaluate(testing, null);
    if (result == 30)
        Console.WriteLine("Passed basic function no ws.");
    else
        throw new ArgumentException("Failed basic function no ws." + "expected 30, got " + result);

    //Test basic function with ws.
    testing = "2 + 2 * 10 + 8 - 10";
    result = Evaluator.Evaluate(testing, null);
    if (result == 20)
        Console.WriteLine("Passed basic function with ws.");
    else
        throw new ArgumentException("Failed basic function with ws." + "expected 20, got " + result);

    //Test () no ws.
    testing = "((2+8)*6)/3";
    result = Evaluator.Evaluate(testing, null);
    if (result == 20)
        Console.WriteLine("Passed () function no ws.");
    else
        throw new ArgumentException("Failed () function no ws." + "expected 20, got " + result);

    //Test () with ws.
    testing = "( ( 2 + 8 ) * 6 ) / ( 2 + 1)  ";
    result = Evaluator.Evaluate(testing, null);
    if (result == 20)
        Console.WriteLine("Passed () function with ws.");
    else
        throw new ArgumentException("Failed () function with ws." + "expected 20, got " + result);

    //Test Complex Function no ws.
    testing = "(2/2)+2*((3-2)+2*(4/2))";
    result = Evaluator.Evaluate(testing, null);
    if (result == 11)
        Console.WriteLine("Passed complex function no ws.");
    else
        throw new ArgumentException("Failed complex function no ws." + "expected 11, got " + result);

    //Test complex Function with ws.
    testing = "   (8 / 2) + 4 * ((3 - 2) + 2 * (2 / 2))";
    result = Evaluator.Evaluate(testing, null);
    if (result == 16)
        Console.WriteLine("Passed complex function with ws.");
    else
        throw new ArgumentException("Failed complex function with ws." + "expected 16, got " + result);

    //Test vars no ws
    testing = "A2+c3+H999-GGh3";
    result = Evaluator.Evaluate(testing, simpleEval);
    if (result == 2)
        Console.WriteLine("Passed vars function no ws.");
    else
        throw new ArgumentException("Failed vars function no ws." + "expected 2, got " + result);

    //Test vars with ws.
    testing = "A2 + c3 + H999 - GGh3";
    result = Evaluator.Evaluate(testing, simpleEval);
    if (result == 2)
        Console.WriteLine("Passed vars function with ws.");
    else
        throw new ArgumentException("Failed vars function with ws." + "expected 2, got " + result);

    Console.WriteLine("Passed all tests!");
    Console.ReadLine();
}

catch (ArgumentException e)
{
    Console.WriteLine(e.ToString());
}
