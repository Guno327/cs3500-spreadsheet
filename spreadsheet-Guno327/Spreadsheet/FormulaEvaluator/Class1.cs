using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    public static class Evaluator
    {
        public delegate int Lookup(String v);

        /// <summary>
        /// Evaluates an infix expression.
        /// </summary>
        /// <param name="exp">The expression to be evaluated</param>
        /// <param name="variableEvaluator">A variable lookup function to be used</param>
        /// <returns>The result of evalutaion as an int</returns>
        /// <exception cref="Exception">If an error occurs during evalution.</exception>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            //Remove white space
            exp = exp.Replace(" ", "");
            //Tokenize
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            //Setup Stacks
            Stack<int> vals = new Stack<int>();
            Stack<string> ops = new Stack<string>();

            //Loop on each token
            foreach (String s in substrings)
            {
                //Ignore empty strings made by replace
                if (s == "")
                    continue;
                //if int
                else if (Regex.IsMatch(s, @"^\d+$") || isVar(s))
                {
                    //Convert the string to an int
                    int i = 0;
                    if (isVar(s))
                        i = variableEvaluator(s);
                    else
                        i = int.Parse(s);
                    //if top op is */, operate on top val and s
                    if (ops.OnTop("*") || ops.OnTop("/"))
                    {
                        //Error if vals is empty, or divisor is 0
                        if (vals.Count() == 0)
                            throw new ArgumentException("vals was empty when it expected 1 value. (1)");
                        if (ops.Peek() == "/" && i == 0)
                            throw new ArgumentException("tried to divide by 0. (2)");

                        vals.Push(Operate(i, vals.Pop(), ops.Pop()));
                    }
                    //else push s as int
                    else
                    {
                        vals.Push(i);
                    }
                }
                //if +-
                else if (s == "+" || s == "-")
                {
                    //if top op is +-, pop 2 top vals and operate using top op
                    if (ops.OnTop("+") || ops.OnTop("-"))
                    {
                        //Error if vals doesnt have at least 2 values
                        if (vals.Count() < 2)
                            throw new ArgumentException("vals expected to contain 2 values, but contained less. (3)");

                        vals.Push(Operate(vals.Pop(), vals.Pop(), ops.Pop()));
                    }
                    ops.Push(s);
                }
                //if */(, push to ops
                else if (s == "*" || s == "/" || s == "(")
                {
                    ops.Push(s);
                }
                //if )
                else if (s == ")")
                {
                    //If +- on top of ops, operate with to op and top 2 vals
                    if(ops.OnTop("+") || ops.OnTop("-"))
                    {
                        //Error if vals doesnt have at least 2 values
                        if (vals.Count() < 2)
                            throw new ArgumentException("vals expected to contain 2 values, but contained less. (4)");

                        vals.Push(Operate(vals.Pop(), vals.Pop(), ops.Pop()));
                    }

                    //Top op should be (
                    if (ops.OnTop("("))
                        ops.Pop();
                    else
                        throw new ArgumentException("Expected ( on stack, but was not found.");

                    //If top op is */, operator with top op and top 2 vals
                    if(ops.OnTop("*") || ops.OnTop("/"))
                    {
                        if (vals.Count() < 2)
                            throw new ArgumentException("vals expected to contain 2 values, but contained less. (5)");

                        vals.Push(Operate(vals.Pop(), vals.Pop(), ops.Pop()));
                    }
                }
                //Otherwise and error has occured
                else
                {
                    throw new ArgumentException("Invalid token used in expression. (6)");
                }

            }

            //If no ops should be only one value for return, if not error
            if (ops.Count() == 0)
            {
                if (vals.Count() != 1)
                    throw new ArgumentException("vals was empty when it expected one value. (7)");

                return vals.Pop();
            }

            //If not, should be one op and two vals, if not error, if so
            //operate on the top vals with the op
            if (ops.Count() != 1)
                throw new ArgumentException("ops expected to contain 1 value, but did not. (8)");
            if(vals.Count != 2)
                throw new ArgumentException("vals expected to contain 2 values, but did not. (9)");
            return Operate(vals.Pop(), vals.Pop(), ops.Pop());
        }

        /// <summary>
        /// Operates the left-hand side by the right-hand side using and operator.
        /// </summary>
        /// <param name="l">The left-hand side</param>
        /// <param name="r">The right-hand side</param>
        /// <param name="op">The operator</param>
        /// <returns>result of operation</returns>
        /// <exception cref="Exception">If the operator is invalid</exception>
        private static int Operate(int r, int l, string op)
        {
            //operate l by r using op
            switch (op)
            {
                case ("*"):
                    return l * r;
                case ("/"):
                    return l / r;
                case ("+"):
                    return l + r;
                case ("-"):
                    return l - r;
            }

            //If we get here op is invalid
            throw new ArgumentException("Invalid token used in expression . (0)");
        }

        /// <summary>
        /// Determines if the given string meets the formating requirments to be a variable.
        /// </summary>
        /// <param name="s">The given string</param>
        /// <returns>True/False depending on wether or not s is a variable</returns>
        private static bool isVar(string s)
        {
            //String is a variable if the first char is a letter and the last is a number
            if (char.IsLetter(s[0]) && char.IsDigit(s[s.Length - 1]))
                return true;

            return false;
        }

        /// <summary>
        /// Extension for string stack that checks to see if the top string is the same as op.
        /// </summary>
        /// <param name="stack">The stack we are checking.</param>
        /// <param name="op">The string we are looking for.</param>
        /// <returns>True/False depending on if the to value was the same as op.</returns>
        static bool OnTop(this Stack<string> stack, string op)
        {
            return stack.Count() > 0 && stack.Peek() == op;
        }
    }
}