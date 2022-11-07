// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!

// Change log:
// Last updated: 9/8, updated for non-nullable types

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        //Fields
        private List<string> tokens;
        private Func<string, string> normalize;
        private Func<string, bool> isValid;

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            //Store normalizer && isValid for use in evaluate
            this.normalize = normalize;
            this.isValid = isValid;
            //Tokenize
            tokens = GetTokens(formula).ToList();
            //Check syntax
            CheckSyntax();
        }

        /// <summary>
        /// Checks the syntax of this formula based on its token list.
        /// </summary>
        /// <param name="isValid">Delegate to check for valid variables.</param>
        /// <exception cref="FormulaFormatException">If any part of the formula is found to be invalid.</exception>
        private void CheckSyntax()
        {
            //One Token Rule
            if (tokens.Count <= 0)
                throw new FormulaFormatException("Given formula was empty.");
            //Starting Token Rule
            if (IsOperator(tokens[0]))
                throw new FormulaFormatException("Starting token may not be and operator.");
            //Ending Token Rule
            if (tokens[tokens.Count - 1] == "*" || tokens[tokens.Count - 1] == "+" || tokens[tokens.Count - 1] == "-" || tokens[tokens.Count - 1] == "/")
                throw new FormulaFormatException("Ending token may not be and operator.");
            //Only Valid Tokens && Other Rules
            int open = 0;
            int close = 0;
            for(int i = 0; i < tokens.Count - 1; i++)
            {
                switch (tokens[i])
                {
                    case "(":
                        open++;
                        //Parenthesis Rule
                        if (IsOperator(tokens[i + 1]))
                            throw new FormulaFormatException("Token after an open paren may not be an operator.");
                        break;
                    case ")":
                        close++;
                        //Right Parentheses Rule
                        if (close > open)
                            throw new FormulaFormatException("Number of closing parens exceeds number of opening parens.");
                        //Extra Following Rule
                        if (tokens[i + 1] != ")" && !IsOperator(tokens[i + 1]))
                            throw new FormulaFormatException("Token after closing paren must be another closing paren or operator.");
                        break;
                    case "*":
                    case "+":
                    case "-":
                    case "/":
                        //Operator Rule
                        if (IsOperator(tokens[i + 1]))
                            throw new FormulaFormatException("You may not have 2 operators in a row.");
                        break;
                    default:
                        //Extra Following Rule
                        if (Double.TryParse(tokens[i], out double d))
                        {
                            if (tokens[i + 1] != ")" && !IsOperator(tokens[i + 1]))
                                throw new FormulaFormatException("You must have a closing paren or operator following a number.");
                        }
                        else if (isValid(normalize(tokens[i])))
                        {
                            if(tokens[i + 1] != ")" && !IsOperator(tokens[i + 1]))
                                throw new FormulaFormatException("You must have a closing paren or operator following a variable.");
                        }
                        else
                        {
                            throw new FormulaFormatException("Invalid token in formula.");
                        }
                        break;
                }
            }
            //Must do some more checks on last token
            if (tokens[tokens.Count - 1] == ")")
                close++;
            if (IsVar(normalize(tokens[tokens.Count - 1])) && !isValid(tokens[tokens.Count - 1]))
                throw new FormulaFormatException("Invalid token in formula.");

            //Balanced Parentheses Rule
            if (close != open)
                throw new FormulaFormatException("Number of opening parens must equal number of closing parens.");
        }

        private bool IsVar(string s)
        {
            return !IsOperator(s) && !Double.TryParse(s, out double x) && s != "(" && s != ")";
        }

        /// <summary>
        /// Determines if the given string is a mathmatical operator.
        /// </summary>
        /// <param name="s">The string to be evaluated.</param>
        /// <returns>T/F depending on if it is an operator.</returns>
        private bool IsOperator(string s)
        {
            return s == "+" || s == "-" || s == "*" || s == "/";
        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            //Setup Stacks
            Stack<double> vals = new();
            Stack<string> ops = new();

            //Loop on each token
            foreach (String s in tokens)
            {
                //if int
                if (Double.TryParse(s, out double d) || IsVar(s))
                {
                    //evaluate if var
                    if (IsVar(s) && isValid(normalize(s)))
                    {
                        try { d = lookup(normalize(s)); }
                        //Handle errors
                        catch (ArgumentException e) { return new FormulaError(e.ToString()); }
                    }
                    //if top op is */, operate on top val and d
                    if (OnTop(ops, "*") || OnTop(ops, "/"))
                    {
                        //Since we are dividing there is the possibility of an error.
                        Object o = Operate(d, vals.Pop(), ops.Pop());
                        if (o.GetType() == typeof(FormulaError))
                            return o;

                        vals.Push((double)o);
                    }
                    //else push d
                    else
                    {
                        vals.Push(d);
                    }
                }
                //if +-
                else if (s == "+" || s == "-")
                {
                    //if top op is +-, pop 2 top vals and operate using top op
                    if (OnTop(ops, "+") || OnTop(ops, "-"))
                        vals.Push((double)Operate(vals.Pop(), vals.Pop(), ops.Pop()));
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
                    if (OnTop(ops, "+") || OnTop(ops, "-"))
                        vals.Push((double)Operate(vals.Pop(), vals.Pop(), ops.Pop()));

                    //Top op should be (, pop it
                    ops.Pop();

                    //If top op is */, operator with top op and top 2 vals
                    if (OnTop(ops, "*") || OnTop(ops, "/"))
                    {
                        //Since we are dividing there is the possibility of an error
                        Object o = Operate(vals.Pop(), vals.Pop(), ops.Pop());
                        if (o.GetType() == typeof(FormulaError))
                            return o;

                        vals.Push((double)o);
                    }
                }
            }
            //If no ops should be only one value for return
            if (ops.Count() == 0)
                return vals.Pop();

            //operate on the top vals with the op
            return Operate(vals.Pop(), vals.Pop(), ops.Pop());
        }

        /// <summary>
        /// Checks to see if the top op is the same as the one we are looking for.
        /// </summary>
        /// <param name="stack">The stack we are checking.</param>
        /// <param name="op">The string we are looking for.</param>
        /// <returns>True/False depending on if the to value was the same as op.</returns>
        static bool OnTop(Stack<string> stack, string op)
        {
            return stack.Count() > 0 && stack.Peek() == op;
        }

        /// <summary>
        /// Operates the left-hand side by the right-hand side using and operator.
        /// </summary>
        /// <param name="l">The left-hand side</param>
        /// <param name="r">The right-hand side</param>
        /// <param name="op">The operator</param>
        /// <returns>result of operation</returns>
        private Object Operate(double r, double l, string op)
        {
                if (op == "*")
                    return l * r;
                else if (op == "/")
                {
                    if (r == 0)
                        return new FormulaError("Tried to divide by 0.");
                    return l / r;
                }
                else if (op == "+")
                    return l + r;
                else
                    return l - r;
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            List<string> vars = new();
            foreach (string s in tokens)
                if (IsVar(s) && isValid(normalize(s)))
                    if(!vars.Contains(normalize(s)))
                        vars.Add(normalize(s));
            return vars;

        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach(string s in tokens)
            {
                if (isValid(s) && IsVar(s))
                    sb.Append(normalize(s));
                else if (Double.TryParse(s, out double d))
                    sb.Append(d.ToString());
                else
                    sb.Append(s);
            }

            return sb.ToString();
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object? obj)
        {
            //Check Null
            if(obj == null)
                return false;
            //Check Is Formula
            if (obj.GetType() != this.GetType())
                return false;
            //Check Equality
            return this.ToString() == obj.ToString();
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            return f1.ToString() == f2.ToString();
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            return f1.ToString() != f2.ToString();
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }

    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}